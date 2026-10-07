using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests;

public sealed class TodoServiceSqliteTests
{
    private const string CategoriesMigration = "20261006081023_AddTodoCategories";
    private const string UsersMigration = "20261007100800_AddTodoUsers";

    [Fact]
    public async Task AddTodoUsersMigration_PreservesExistingDataAndCreatesRestrictedOptionalForeignKey()
    {
        await using var connection = await CreateConnectionAsync();
        var options = CreateOptions(connection);

        await using (var db = new TodoDbContext(options))
        {
            await db.Database.MigrateAsync(CategoriesMigration);
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO TodoCategories (Name) VALUES ('Work');");
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO TodoItems (Title, IsCompleted, CreatedAt, CategoryId) " +
                "VALUES ('Existing task', 1, '2024-01-02 03:04:05', 1);");
            await db.Database.MigrateAsync();
        }

        await using (var db = new TodoDbContext(options))
        {
            var todo = await db.TodoItems.Include(item => item.Category).SingleAsync();
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
            var foreignKey = await ReadUserForeignKeyAsync(connection);

            appliedMigrations.Should().Contain(UsersMigration);
            todo.Title.Should().Be("Existing task");
            todo.IsCompleted.Should().BeTrue();
            todo.CreatedAt.Should().Be(DateTime.Parse("2024-01-02 03:04:05"));
            todo.Category!.Name.Should().Be("Work");
            todo.UserId.Should().BeNull();
            foreignKey.Table.Should().Be("TodoUsers");
            foreignKey.OnDelete.Should().Be("RESTRICT");
        }
    }

    [Fact]
    public async Task AssignedTodoAndRenamedUser_PersistAcrossNewSqliteContexts()
    {
        await using var connection = await CreateMigratedConnectionAsync();
        var options = CreateOptions(connection);
        int userId;
        int todoId;

        await using (var db = new TodoDbContext(options))
        {
            var service = new TodoService(db);
            var user = await service.CreateUserAsync("Ana");
            var todo = await service.CreateAsync(new TodoRequest("Task", UserId: user.User!.Id));
            var renamed = await service.UpdateUserAsync(user.User.Id, "Ana García");

            userId = user.User.Id;
            todoId = todo.Todo!.Id;
            renamed.User!.Name.Should().Be("Ana García");
        }

        await using (var db = new TodoDbContext(options))
        {
            var todo = await new TodoService(db).GetByIdAsync(todoId);

            todo!.UserId.Should().Be(userId);
            todo.User!.Name.Should().Be("Ana García");
        }
    }

    [Fact]
    public async Task DeleteUserAsync_WhenForeignKeyRejectsDelete_DetachesUserAndKeepsDatabaseConsistent()
    {
        await using var connection = await CreateMigratedConnectionAsync();
        var seedOptions = CreateOptions(connection);
        int userId;

        await using (var seedDb = new TodoDbContext(seedOptions))
        {
            userId = (await new TodoService(seedDb).CreateUserAsync("Ana")).User!.Id;
        }

        var interceptor = new InsertTodoBeforeUserDeleteInterceptor(connection);
        var options = CreateOptions(connection, interceptor);
        await using (var db = new TodoDbContext(options))
        {
            var user = await db.TodoUsers.SingleAsync(item => item.Id == userId);

            var result = await new TodoService(db).DeleteUserAsync(userId);

            result.Exists.Should().BeTrue();
            result.InUse.Should().BeTrue();
            interceptor.InsertedTodo.Should().BeTrue();
            db.Entry(user).State.Should().Be(EntityState.Detached);
            (await db.TodoUsers.AnyAsync(item => item.Id == userId)).Should().BeTrue();
            (await db.TodoItems.CountAsync(item => item.UserId == userId)).Should().Be(1);
        }
    }

    [Fact]
    public async Task CreateAsync_WhenForeignKeyFailsForExistingUser_PropagatesAndDetachesNewTodo()
    {
        await using var connection = await CreateMigratedConnectionAsync();
        var seedOptions = CreateOptions(connection);
        int userId;
        int categoryId;

        await using (var seedDb = new TodoDbContext(seedOptions))
        {
            userId = (await new TodoService(seedDb).CreateUserAsync("Ana")).User!.Id;
            categoryId = (await new TodoService(seedDb).CreateCategoryAsync("Work")).Category!.Id;
        }

        var interceptor = new DeleteCategoryBeforeTodoInsertInterceptor(connection);
        var options = CreateOptions(connection, interceptor);
        await using (var db = new TodoDbContext(options))
        {
            var act = () => new TodoService(db).CreateAsync(
                new TodoRequest("Racing task", CategoryId: categoryId, UserId: userId));

            var exception = await act.Should().ThrowAsync<DbUpdateException>();
            exception.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().Be(787);
            interceptor.DeletedCategory.Should().BeTrue();
            db.ChangeTracker.Entries<TodoItem>().Should().BeEmpty();
            (await db.TodoItems.CountAsync()).Should().Be(0);
            (await db.TodoUsers.AnyAsync(item => item.Id == userId)).Should().BeTrue();
            (await db.TodoCategories.AnyAsync(item => item.Id == categoryId)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateAsync_WhenSqliteFailsForAnotherReason_PropagatesDatabaseError()
    {
        await using var connection = await CreateMigratedConnectionAsync();
        var options = CreateOptions(connection);
        await using var db = new TodoDbContext(options);
        await db.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER RejectTodoInsert BEFORE INSERT ON TodoItems " +
            "BEGIN SELECT RAISE(ABORT, 'synthetic database failure'); END;");

        var act = () => new TodoService(db).CreateAsync(new TodoRequest("Task"));

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    private static async Task<SqliteConnection> CreateConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<SqliteConnection> CreateMigratedConnectionAsync()
    {
        var connection = await CreateConnectionAsync();
        await using var db = new TodoDbContext(CreateOptions(connection));
        await db.Database.MigrateAsync();
        return connection;
    }

    private static DbContextOptions<TodoDbContext> CreateOptions(
        SqliteConnection connection,
        params IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<TodoDbContext>().UseSqlite(connection);
        if (interceptors.Length > 0)
        {
            builder.AddInterceptors(interceptors);
        }

        return builder.Options;
    }

    private static async Task<(string Table, string OnDelete)> ReadUserForeignKeyAsync(SqliteConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_list('TodoItems');";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (reader.GetString(2) == "TodoUsers")
            {
                return (reader.GetString(2), reader.GetString(6));
            }
        }

        throw new InvalidOperationException("The user foreign key was not created.");
    }

    private sealed class InsertTodoBeforeUserDeleteInterceptor(SqliteConnection connection) : SaveChangesInterceptor
    {
        public bool InsertedTodo { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!InsertedTodo && eventData.Context?.ChangeTracker.Entries<TodoUser>()
                    .Any(entry => entry.State == EntityState.Deleted) == true)
            {
                await using var command = connection.CreateCommand();
                command.CommandText =
                    "INSERT INTO TodoItems (Title, IsCompleted, CreatedAt, UserId) " +
                    "VALUES ('Concurrent task', 0, '2024-01-02 03:04:05', " +
                    "(SELECT Id FROM TodoUsers LIMIT 1));";
                await command.ExecuteNonQueryAsync(cancellationToken);
                InsertedTodo = true;
            }

            return result;
        }
    }

    private sealed class DeleteCategoryBeforeTodoInsertInterceptor(SqliteConnection connection) : SaveChangesInterceptor
    {
        public bool DeletedCategory { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!DeletedCategory && eventData.Context?.ChangeTracker.Entries<TodoItem>()
                    .Any(entry => entry.State == EntityState.Added) == true)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM TodoCategories;";
                await command.ExecuteNonQueryAsync(cancellationToken);
                DeletedCategory = true;
            }

            return result;
        }
    }
}
