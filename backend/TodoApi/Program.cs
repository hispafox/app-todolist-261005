using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
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
    Results.Ok(await service.GetAllAsync()));

app.MapGet("/api/todos/{id:int}", async (int id, TodoService service) =>
    await service.GetByIdAsync(id) is TodoItem todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapPost("/api/todos", async (TodoItem todo, TodoService service) =>
{
    var result = await service.CreateAsync(todo);
    if (!result.Success)
    {
        return Results.BadRequest(new { message = result.ErrorMessage });
    }

    return Results.Created($"/api/todos/{result.Todo!.Id}", result.Todo);
});

app.MapPut("/api/todos/{id:int}", async (int id, TodoItem input, TodoService service) =>
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

    return Results.Ok(result.Todo);
});

app.MapDelete("/api/todos/{id:int}", async (int id, TodoService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
