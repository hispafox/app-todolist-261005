using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

try
{
    Log.ForContext("SourceContext", "TodoApi.Host").Information("{EventType}", "HostStarting");

    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Filter.ByIncludingOnly(IsAllowedLogEvent));

    builder.Services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<TodoService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();
    app.Lifetime.ApplicationStarted.Register(() =>
        Log.ForContext("SourceContext", "TodoApi.Host").Information("{EventType}", "HostStarted"));
    app.Lifetime.ApplicationStopped.Register(() =>
        Log.ForContext("SourceContext", "TodoApi.Host").Information("{EventType}", "HostStopped"));

    using (var startupScope = app.Services.CreateScope())
    {
        var startupDb = startupScope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var pendingMigrations = (await startupDb.Database.GetPendingMigrationsAsync()).ToList();
        if (pendingMigrations.Count > 0)
        {
            Log.ForContext("SourceContext", "TodoApi.Host")
                .Warning(
                    "Pending database migrations detected {EventType} {PendingMigrationCount} {PendingMigrations}",
                    "PendingMigrations",
                    pendingMigrations.Count,
                    pendingMigrations);
        }
    }

    app.Use(async (context, next) =>
    {
        var logger = Log.ForContext("SourceContext", "TodoApi.Http");
        var startedAt = Stopwatch.GetTimestamp();
        Exception? unexpectedException = null;
        var requestCancelled = false;

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            requestCancelled = true;
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode is >= 400 and < 500)
        {
            unexpectedException = exception;
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = exception.StatusCode;
            await context.Response.WriteAsJsonAsync(
                new { message = "La solicitud no es válida." },
                cancellationToken: CancellationToken.None);
            unexpectedException = null;
        }
        catch (Exception exception)
        {
            unexpectedException = exception;
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(
                new { message = "Se ha producido un error interno." },
                cancellationToken: CancellationToken.None);
        }
        finally
        {
            var statusCode = requestCancelled ? 499 : context.Response.StatusCode;
            var routeTemplate = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched";
            var requestMethod = GetSafeHttpMethod(context.Request.Method);
            var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            var traceId = context.TraceIdentifier;

            if (unexpectedException is not null)
            {
                logger.Error(
                    unexpectedException,
                    "HTTP request failed {EventType} {RequestMethod} {RouteTemplate} {StatusCode} {ElapsedMs} {TraceId} {ExceptionType}",
                    "HttpRequestFailed",
                    requestMethod,
                    routeTemplate,
                    statusCode,
                    elapsedMs,
                    traceId,
                    unexpectedException.GetType().FullName);
            }
            else if (requestCancelled)
            {
                logger.Information(
                    "HTTP request cancelled {EventType} {RequestMethod} {RouteTemplate} {StatusCode} {ElapsedMs} {TraceId}",
                    "HttpRequestCancelled",
                    requestMethod,
                    routeTemplate,
                    statusCode,
                    elapsedMs,
                    traceId);
            }
            else if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.Error(
                    "HTTP request completed {EventType} {RequestMethod} {RouteTemplate} {StatusCode} {ElapsedMs} {TraceId}",
                    "HttpRequestCompleted",
                    requestMethod,
                    routeTemplate,
                    statusCode,
                    elapsedMs,
                    traceId);
            }
            else if (statusCode >= StatusCodes.Status400BadRequest)
            {
                logger.Warning(
                    "HTTP request completed {EventType} {RequestMethod} {RouteTemplate} {StatusCode} {ElapsedMs} {TraceId}",
                    "HttpRequestCompleted",
                    requestMethod,
                    routeTemplate,
                    statusCode,
                    elapsedMs,
                    traceId);
            }
            else
            {
                logger.Information(
                    "HTTP request completed {EventType} {RequestMethod} {RouteTemplate} {StatusCode} {ElapsedMs} {TraceId}",
                    "HttpRequestCompleted",
                    requestMethod,
                    routeTemplate,
                    statusCode,
                    elapsedMs,
                    traceId);
            }
        }
    });

    app.UseCors("Frontend");
    app.UseHttpsRedirection();

    app.MapGet("/api/todos", async (TodoService service, CancellationToken cancellationToken) =>
        Results.Ok((await service.GetAllAsync(cancellationToken)).Select(ToTodoResponse)));

    app.MapGet("/api/todos/{id:int}", async (int id, TodoService service, CancellationToken cancellationToken) =>
        await service.GetByIdAsync(id, cancellationToken) is TodoItem todo
            ? Results.Ok(ToTodoResponse(todo))
            : Results.NotFound());

    app.MapPost("/api/todos", async (TodoRequest todo, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.CreateAsync(todo, cancellationToken);
        if (!result.Success)
        {
            return Results.BadRequest(new { message = result.ErrorMessage });
        }

        return Results.Created($"/api/todos/{result.Todo!.Id}", ToTodoResponse(result.Todo));
    });

    app.MapPut("/api/todos/{id:int}", async (int id, TodoRequest input, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.UpdateAsync(id, input, cancellationToken);
        if (!result.Success)
        {
            if (result.ErrorMessage is not null)
            {
                return Results.BadRequest(new { message = result.ErrorMessage });
            }

            return Results.NotFound();
        }

        return Results.Ok(ToTodoResponse(result.Todo!));
    });

    app.MapDelete("/api/todos/{id:int}", async (int id, TodoService service, CancellationToken cancellationToken) =>
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    });

    app.MapGet("/api/categories", async (TodoService service, CancellationToken cancellationToken) =>
        Results.Ok((await service.GetCategoriesAsync(cancellationToken)).Select(ToCategoryResponse)));

    app.MapGet("/api/categories/{id:int}", async (int id, TodoService service, CancellationToken cancellationToken) =>
        await service.GetCategoryByIdAsync(id, cancellationToken) is TodoCategory category
            ? Results.Ok(ToCategoryResponse(category))
            : Results.NotFound());

    app.MapPost("/api/categories", async (CategoryRequest input, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.CreateCategoryAsync(input.Name, cancellationToken);
        if (!result.Success)
        {
            return result.ErrorMessage == TodoService.DuplicateCategoryNameError
                ? Results.Conflict(new { message = result.ErrorMessage })
                : Results.BadRequest(new { message = result.ErrorMessage });
        }

        return Results.Created($"/api/categories/{result.Category!.Id}", ToCategoryResponse(result.Category));
    });

    app.MapPut("/api/categories/{id:int}", async (int id, CategoryRequest input, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.UpdateCategoryAsync(id, input.Name, cancellationToken);
        if (!result.Success)
        {
            if (result.ErrorMessage is null)
            {
                return Results.NotFound();
            }

            return result.ErrorMessage == TodoService.DuplicateCategoryNameError
                ? Results.Conflict(new { message = result.ErrorMessage })
                : Results.BadRequest(new { message = result.ErrorMessage });
        }

        return Results.Ok(ToCategoryResponse(result.Category!));
    });

    app.MapDelete("/api/categories/{id:int}", async (int id, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.DeleteCategoryAsync(id, cancellationToken);
        if (!result.Exists)
        {
            return Results.NotFound();
        }

        return result.InUse
            ? Results.Conflict(new { message = "No se puede eliminar una categoría que tiene tareas asociadas." })
            : Results.NoContent();
    });

    app.MapGet("/api/users", async (TodoService service, CancellationToken cancellationToken) =>
        Results.Ok((await service.GetUsersAsync(cancellationToken)).Select(ToUserResponse)));

    app.MapGet("/api/users/{id:int}", async (int id, TodoService service, CancellationToken cancellationToken) =>
        await service.GetUserByIdAsync(id, cancellationToken) is TodoUser user
            ? Results.Ok(ToUserResponse(user))
            : Results.NotFound());

    app.MapPost("/api/users", async (UserRequest input, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.CreateUserAsync(input.Name, cancellationToken);
        if (!result.Success)
        {
            return Results.BadRequest(new { message = result.ErrorMessage });
        }

        return Results.Created($"/api/users/{result.User!.Id}", ToUserResponse(result.User));
    });

    app.MapPut("/api/users/{id:int}", async (int id, UserRequest input, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.UpdateUserAsync(id, input.Name, cancellationToken);
        if (!result.Success)
        {
            if (result.ErrorMessage is null)
            {
                return Results.NotFound();
            }

            return Results.BadRequest(new { message = result.ErrorMessage });
        }

        return Results.Ok(ToUserResponse(result.User!));
    });

    app.MapDelete("/api/users/{id:int}", async (int id, TodoService service, CancellationToken cancellationToken) =>
    {
        var result = await service.DeleteUserAsync(id, cancellationToken);
        if (!result.Exists)
        {
            return Results.NotFound();
        }

        return result.InUse
            ? Results.Conflict(new { message = TodoService.UserInUseError })
            : Results.NoContent();
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    await app.RunAsync();
}
catch (HostAbortedException)
{
    // EF Core tooling intentionally aborts the bootstrap host after reading its services.
}
catch (Exception exception)
{
    Log.ForContext("SourceContext", "TodoApi.Host")
        .Error("Host failed {EventType} {ExceptionType}", "HostFailed", exception.GetType().FullName);
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

static bool IsAllowedLogEvent(LogEvent logEvent)
{
    return logEvent.Properties.TryGetValue("SourceContext", out var sourceContext)
        && sourceContext is ScalarValue { Value: string source }
        && (source == "TodoApi.Http"
            || source.StartsWith("TodoApi.Http.", StringComparison.Ordinal)
            || source == "TodoApi.Host"
            || source.StartsWith("TodoApi.Host.", StringComparison.Ordinal));
}

static string GetSafeHttpMethod(string method)
{
    return HttpMethods.IsGet(method) ? HttpMethods.Get
        : HttpMethods.IsPost(method) ? HttpMethods.Post
        : HttpMethods.IsPut(method) ? HttpMethods.Put
        : HttpMethods.IsDelete(method) ? HttpMethods.Delete
        : HttpMethods.IsPatch(method) ? HttpMethods.Patch
        : HttpMethods.IsHead(method) ? HttpMethods.Head
        : HttpMethods.IsOptions(method) ? HttpMethods.Options
        : HttpMethods.IsTrace(method) ? HttpMethods.Trace
        : HttpMethods.IsConnect(method) ? HttpMethods.Connect
        : "OTHER";
}

static TodoResponse ToTodoResponse(TodoItem todo) =>
    new(todo.Id, todo.Title, todo.IsCompleted, todo.CreatedAt, todo.CategoryId, todo.Category?.Name, todo.UserId, todo.User?.Name);

static CategoryResponse ToCategoryResponse(TodoCategory category) =>
    new(category.Id, category.Name);

static UserResponse ToUserResponse(TodoUser user) =>
    new(user.Id, user.Name);
