using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.UseHttpsRedirection();

app.MapGet("/api/todos", async (TodoService service) =>
    Results.Ok((await service.GetAllAsync()).Select(ToTodoResponse)));

app.MapGet("/api/todos/{id:int}", async (int id, TodoService service) =>
    await service.GetByIdAsync(id) is TodoItem todo
        ? Results.Ok(ToTodoResponse(todo))
        : Results.NotFound());

app.MapPost("/api/todos", async (TodoRequest todo, TodoService service) =>
{
    var result = await service.CreateAsync(todo);
    if (!result.Success)
    {
        return Results.BadRequest(new { message = result.ErrorMessage });
    }

    return Results.Created($"/api/todos/{result.Todo!.Id}", ToTodoResponse(result.Todo));
});

app.MapPut("/api/todos/{id:int}", async (int id, TodoRequest input, TodoService service) =>
{
    var result = await service.UpdateAsync(id, input);
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

app.MapDelete("/api/todos/{id:int}", async (int id, TodoService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/categories", async (TodoService service) =>
    Results.Ok((await service.GetCategoriesAsync()).Select(ToCategoryResponse)));

app.MapGet("/api/categories/{id:int}", async (int id, TodoService service) =>
    await service.GetCategoryByIdAsync(id) is TodoCategory category
        ? Results.Ok(ToCategoryResponse(category))
        : Results.NotFound());

app.MapPost("/api/categories", async (CategoryRequest input, TodoService service) =>
{
    var result = await service.CreateCategoryAsync(input.Name);
    if (!result.Success)
    {
        return result.ErrorMessage == TodoService.DuplicateCategoryNameError
            ? Results.Conflict(new { message = result.ErrorMessage })
            : Results.BadRequest(new { message = result.ErrorMessage });
    }

    return Results.Created($"/api/categories/{result.Category!.Id}", ToCategoryResponse(result.Category));
});

app.MapPut("/api/categories/{id:int}", async (int id, CategoryRequest input, TodoService service) =>
{
    var result = await service.UpdateCategoryAsync(id, input.Name);
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

app.MapDelete("/api/categories/{id:int}", async (int id, TodoService service) =>
{
    var result = await service.DeleteCategoryAsync(id);
    if (!result.Exists)
    {
        return Results.NotFound();
    }

    return result.InUse
        ? Results.Conflict(new { message = "No se puede eliminar una categoría que tiene tareas asociadas." })
        : Results.NoContent();
});

app.Run();

static TodoResponse ToTodoResponse(TodoItem todo) =>
    new(todo.Id, todo.Title, todo.IsCompleted, todo.CreatedAt, todo.CategoryId, todo.Category?.Name);

static CategoryResponse ToCategoryResponse(TodoCategory category) =>
    new(category.Id, category.Name);
