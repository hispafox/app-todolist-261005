using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Services;

public sealed class TodoService
{
    public const string DuplicateCategoryNameError = "Ya existe una categoría con ese nombre.";

    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await _db.TodoItems
            .Include(todo => todo.Category)
            .OrderBy(todo => todo.Id)
            .ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _db.TodoItems
            .Include(todo => todo.Category)
            .SingleOrDefaultAsync(todo => todo.Id == id);
    }

    public async Task<(bool Success, TodoItem? Todo, string? ErrorMessage)> CreateAsync(TodoRequest input)
    {
        var title = NormalizeTitle(input.Title);
        if (title is null)
        {
            return (false, null, "El título es obligatorio.");
        }

        var category = await FindCategoryAsync(input.CategoryId);
        if (input.CategoryId is not null && category is null)
        {
            return (false, null, "La categoría seleccionada no existe.");
        }

        var todo = new TodoItem
        {
            Title = title,
            IsCompleted = input.IsCompleted,
            CreatedAt = input.CreatedAt is null || input.CreatedAt == default
                ? DateTime.UtcNow
                : input.CreatedAt.Value,
            CategoryId = input.CategoryId,
            Category = category
        };

        _db.TodoItems.Add(todo);
        await _db.SaveChangesAsync();

        return (true, todo, null);
    }

    public async Task<(bool Success, TodoItem? Todo, string? ErrorMessage)> UpdateAsync(int id, TodoRequest input)
    {
        var todo = await _db.TodoItems.FindAsync(id);
        if (todo is null)
        {
            return (false, null, null);
        }

        var title = NormalizeTitle(input.Title);
        if (title is null)
        {
            return (false, null, "El título es obligatorio.");
        }

        var category = await FindCategoryAsync(input.CategoryId);
        if (input.CategoryId is not null && category is null)
        {
            return (false, null, "La categoría seleccionada no existe.");
        }

        todo.Title = title;
        todo.IsCompleted = input.IsCompleted;
        todo.CategoryId = input.CategoryId;
        todo.Category = category;
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

    public async Task<List<TodoCategory>> GetCategoriesAsync()
    {
        return await _db.TodoCategories.OrderBy(category => category.Name).ToListAsync();
    }

    public async Task<TodoCategory?> GetCategoryByIdAsync(int id)
    {
        return await _db.TodoCategories.FindAsync(id);
    }

    public async Task<(bool Success, TodoCategory? Category, string? ErrorMessage)> CreateCategoryAsync(string? name)
    {
        var normalizedName = NormalizeCategoryName(name);
        if (normalizedName is null)
        {
            return (false, null, "El nombre de la categoría es obligatorio.");
        }

        if (await CategoryNameExistsAsync(normalizedName))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        var category = new TodoCategory { Name = normalizedName };
        _db.TodoCategories.Add(category);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueCategoryViolation(exception))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        return (true, category, null);
    }

    public async Task<(bool Success, TodoCategory? Category, string? ErrorMessage)> UpdateCategoryAsync(int id, string? name)
    {
        var category = await _db.TodoCategories.FindAsync(id);
        if (category is null)
        {
            return (false, null, null);
        }

        var normalizedName = NormalizeCategoryName(name);
        if (normalizedName is null)
        {
            return (false, null, "El nombre de la categoría es obligatorio.");
        }

        if (await CategoryNameExistsAsync(normalizedName, id))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        category.Name = normalizedName;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueCategoryViolation(exception))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        return (true, category, null);
    }

    public async Task<(bool Exists, bool InUse)> DeleteCategoryAsync(int id)
    {
        var category = await _db.TodoCategories.FindAsync(id);
        if (category is null)
        {
            return (false, false);
        }

        if (await _db.TodoItems.AnyAsync(todo => todo.CategoryId == id))
        {
            return (true, true);
        }

        _db.TodoCategories.Remove(category);
        await _db.SaveChangesAsync();
        return (true, false);
    }

    public static string? NormalizeCategoryName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private async Task<TodoCategory?> FindCategoryAsync(int? categoryId)
    {
        return categoryId is null ? null : await _db.TodoCategories.FindAsync(categoryId.Value);
    }

    private async Task<bool> CategoryNameExistsAsync(string name, int? exceptId = null)
    {
        var names = await _db.TodoCategories
            .Where(category => exceptId == null || category.Id != exceptId)
            .Select(category => category.Name)
            .ToListAsync();

        return names.Any(existingName => string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUniqueCategoryViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };
}
