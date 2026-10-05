using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services;

public sealed class TodoService
{
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await _db.TodoItems.OrderBy(todo => todo.Id).ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _db.TodoItems.FindAsync(id);
    }

    public async Task<(bool Success, TodoItem? Todo, string? ErrorMessage)> CreateAsync(TodoItem input)
    {
        var title = NormalizeTitle(input.Title);
        if (title is null)
        {
            return (false, null, "Title is required.");
        }

        var todo = new TodoItem
        {
            Title = title,
            IsCompleted = input.IsCompleted,
            CreatedAt = input.CreatedAt == default ? DateTime.UtcNow : input.CreatedAt
        };

        _db.TodoItems.Add(todo);
        await _db.SaveChangesAsync();

        return (true, todo, null);
    }

    public async Task<(bool Success, TodoItem? Todo, string? ErrorMessage)> UpdateAsync(int id, TodoItem input)
    {
        var todo = await _db.TodoItems.FindAsync(id);
        if (todo is null)
        {
            return (false, null, null);
        }

        var title = NormalizeTitle(input.Title);
        if (title is null)
        {
            return (false, null, "Title is required.");
        }

        todo.Title = title;
        todo.IsCompleted = input.IsCompleted;
        await _db.SaveChangesAsync();

        return (true, todo, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todo = await _db.TodoItems.FindAsync(id);
        if (todo is null)
        {
            return false;
        }

        _db.TodoItems.Remove(todo);
        await _db.SaveChangesAsync();
        return true;
    }

    public static string? NormalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        return title.Trim();
    }
}
