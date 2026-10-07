using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Services;

public sealed class TodoService
{
    public const string DuplicateCategoryNameError = "Ya existe una categoría con ese nombre.";
    public const string InvalidUserError = "El usuario seleccionado no existe.";
    public const string UserInUseError = "No se puede eliminar un usuario que tiene tareas asignadas.";

    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.TodoItems
            .Include(todo => todo.Category)
            .Include(todo => todo.User)
            .OrderBy(todo => todo.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.TodoItems
            .Include(todo => todo.Category)
            .Include(todo => todo.User)
            .SingleOrDefaultAsync(todo => todo.Id == id, cancellationToken);
    }

    public async Task<(bool Success, TodoItem? Todo, string? ErrorMessage)> CreateAsync(
        TodoRequest input,
        CancellationToken cancellationToken = default)
    {
        var title = NormalizeTitle(input.Title);
        if (title is null)
        {
            return (false, null, "El título es obligatorio.");
        }

        var category = await FindCategoryAsync(input.CategoryId, cancellationToken);
        if (input.CategoryId is not null && category is null)
        {
            return (false, null, "La categoría seleccionada no existe.");
        }

        var user = await FindUserAsync(input.UserId, cancellationToken);
        if (input.UserId is not null && (input.UserId <= 0 || user is null))
        {
            return (false, null, InvalidUserError);
        }

        var todo = new TodoItem
        {
            Title = title,
            IsCompleted = input.IsCompleted,
            CreatedAt = input.CreatedAt is null || input.CreatedAt == default
                ? DateTime.UtcNow
                : input.CreatedAt.Value,
            CategoryId = input.CategoryId,
            Category = category,
            UserId = input.UserId,
            User = user
        };

        _db.TodoItems.Add(todo);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            _db.Entry(todo).State = EntityState.Detached;
            if (input.UserId is not int userId
                || await _db.TodoUsers.AnyAsync(item => item.Id == userId, cancellationToken))
            {
                throw;
            }

            return (false, null, InvalidUserError);
        }

        return (true, todo, null);
    }

    public async Task<(bool Success, TodoItem? Todo, string? ErrorMessage)> UpdateAsync(
        int id,
        TodoRequest input,
        CancellationToken cancellationToken = default)
    {
        var todo = await _db.TodoItems.FindAsync([id], cancellationToken);
        if (todo is null)
        {
            return (false, null, null);
        }

        var title = NormalizeTitle(input.Title);
        if (title is null)
        {
            return (false, null, "El título es obligatorio.");
        }

        var category = await FindCategoryAsync(input.CategoryId, cancellationToken);
        if (input.CategoryId is not null && category is null)
        {
            return (false, null, "La categoría seleccionada no existe.");
        }

        var user = await FindUserAsync(input.UserId, cancellationToken);
        if (input.UserId is not null && (input.UserId <= 0 || user is null))
        {
            return (false, null, InvalidUserError);
        }

        todo.Title = title;
        todo.IsCompleted = input.IsCompleted;
        todo.CategoryId = input.CategoryId;
        todo.Category = category;
        todo.UserId = input.UserId;
        todo.User = user;
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            await _db.Entry(todo).ReloadAsync(cancellationToken);
            if (input.UserId is not int userId
                || await _db.TodoUsers.AnyAsync(item => item.Id == userId, cancellationToken))
            {
                throw;
            }

            return (false, null, InvalidUserError);
        }

        return (true, todo, null);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var todo = await _db.TodoItems.FindAsync([id], cancellationToken);
        if (todo is null)
        {
            return false;
        }

        _db.TodoItems.Remove(todo);
        await _db.SaveChangesAsync(cancellationToken);
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

    public async Task<List<TodoCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.TodoCategories.OrderBy(category => category.Name).ToListAsync(cancellationToken);
    }

    public async Task<TodoCategory?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.TodoCategories.FindAsync([id], cancellationToken);
    }

    public async Task<(bool Success, TodoCategory? Category, string? ErrorMessage)> CreateCategoryAsync(
        string? name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeCategoryName(name);
        if (normalizedName is null)
        {
            return (false, null, "El nombre de la categoría es obligatorio.");
        }

        if (await CategoryNameExistsAsync(normalizedName, cancellationToken: cancellationToken))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        var category = new TodoCategory { Name = normalizedName };
        _db.TodoCategories.Add(category);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueCategoryViolation(exception))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        return (true, category, null);
    }

    public async Task<(bool Success, TodoCategory? Category, string? ErrorMessage)> UpdateCategoryAsync(
        int id,
        string? name,
        CancellationToken cancellationToken = default)
    {
        var category = await _db.TodoCategories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return (false, null, null);
        }

        var normalizedName = NormalizeCategoryName(name);
        if (normalizedName is null)
        {
            return (false, null, "El nombre de la categoría es obligatorio.");
        }

        if (await CategoryNameExistsAsync(normalizedName, id, cancellationToken))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        category.Name = normalizedName;
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueCategoryViolation(exception))
        {
            return (false, null, DuplicateCategoryNameError);
        }

        return (true, category, null);
    }

    public async Task<(bool Exists, bool InUse)> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _db.TodoCategories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return (false, false);
        }

        if (await _db.TodoItems.AnyAsync(todo => todo.CategoryId == id, cancellationToken))
        {
            return (true, true);
        }

        _db.TodoCategories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
        return (true, false);
    }

    public async Task<List<TodoUser>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.TodoUsers
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoUser?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.TodoUsers.FindAsync([id], cancellationToken);
    }

    public async Task<(bool Success, TodoUser? User, string? ErrorMessage)> CreateUserAsync(
        string? name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeUserName(name);
        if (normalizedName is null)
        {
            return (false, null, "El nombre del usuario es obligatorio.");
        }

        var user = new TodoUser { Name = normalizedName };
        _db.TodoUsers.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return (true, user, null);
    }

    public async Task<(bool Success, TodoUser? User, string? ErrorMessage)> UpdateUserAsync(
        int id,
        string? name,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.TodoUsers.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return (false, null, null);
        }

        var normalizedName = NormalizeUserName(name);
        if (normalizedName is null)
        {
            return (false, null, "El nombre del usuario es obligatorio.");
        }

        user.Name = normalizedName;
        await _db.SaveChangesAsync(cancellationToken);
        return (true, user, null);
    }

    public async Task<(bool Exists, bool InUse)> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.TodoUsers.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return (false, false);
        }

        if (await _db.TodoItems.AnyAsync(todo => todo.UserId == id, cancellationToken))
        {
            return (true, true);
        }

        _db.TodoUsers.Remove(user);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            _db.Entry(user).State = EntityState.Detached;
            if (await _db.TodoItems.AnyAsync(todo => todo.UserId == id, cancellationToken))
            {
                return (true, true);
            }

            throw;
        }

        return (true, false);
    }

    public static string? NormalizeUserName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    public static string? NormalizeCategoryName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private async Task<TodoCategory?> FindCategoryAsync(int? categoryId, CancellationToken cancellationToken)
    {
        return categoryId is null ? null : await _db.TodoCategories.FindAsync([categoryId.Value], cancellationToken);
    }

    private async Task<TodoUser?> FindUserAsync(int? userId, CancellationToken cancellationToken)
    {
        return userId is null ? null : await _db.TodoUsers.FindAsync([userId.Value], cancellationToken);
    }

    private async Task<bool> CategoryNameExistsAsync(
        string name,
        int? exceptId = null,
        CancellationToken cancellationToken = default)
    {
        var names = await _db.TodoCategories
            .Where(category => exceptId == null || category.Id != exceptId)
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);

        return names.Any(existingName => string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUniqueCategoryViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException
            && (sqliteException.SqliteExtendedErrorCode == 787
                || sqliteException.SqliteExtendedErrorCode == 1811
                    && string.Equals(
                        sqliteException.Message,
                        "SQLite Error 19: 'FOREIGN KEY constraint failed'.",
                        StringComparison.Ordinal));
    }
}
