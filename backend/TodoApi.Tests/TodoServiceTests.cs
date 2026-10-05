using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests;

public class TodoServiceTests
{
    private static TodoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TodoDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTodosOrderedById()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        db.TodoItems.AddRange(
            new TodoItem { Id = 2, Title = "Second", IsCompleted = true, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new TodoItem { Id = 1, Title = "First", IsCompleted = false, CreatedAt = DateTime.UtcNow.AddMinutes(-1) });
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Select(todo => todo.Id).Should().Equal(1, 2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTodoExists_ReturnsTodo()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var expected = new TodoItem { Title = "Existing", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        db.TodoItems.Add(expected);
        await db.SaveChangesAsync();

        var result = await service.GetByIdAsync(expected.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Existing");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTodoDoesNotExist_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsEmpty_ReturnsValidationErrorAndDoesNotPersist()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.CreateAsync(new TodoItem { Title = "   " });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Title is required.");
        result.Todo.Should().BeNull();
        db.TodoItems.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WhenTitleHasWhitespace_TrimsAndPersists()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var createdAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var result = await service.CreateAsync(new TodoItem
        {
            Title = "  Buy milk  ",
            IsCompleted = true,
            CreatedAt = createdAt
        });

        result.Success.Should().BeTrue();
        result.Todo.Should().NotBeNull();
        result.Todo!.Title.Should().Be("Buy milk");
        result.Todo.IsCompleted.Should().BeTrue();
        result.Todo.CreatedAt.Should().Be(createdAt);
        db.TodoItems.Should().ContainSingle(todo => todo.Title == "Buy milk");
    }

    [Fact]
    public async Task CreateAsync_WhenCreatedAtIsDefault_UsesUtcNow()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var before = DateTime.UtcNow;
        var result = await service.CreateAsync(new TodoItem { Title = "Write tests" });
        var after = DateTime.UtcNow;

        result.Success.Should().BeTrue();
        result.Todo!.CreatedAt.Should().BeOnOrAfter(before);
        result.Todo.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoExists_ChangesTitleAndCompletionState()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var todo = new TodoItem { Title = "Old title", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(todo.Id, new TodoItem { Title = "  New title  ", IsCompleted = true });

        result.Success.Should().BeTrue();
        result.Todo.Should().NotBeNull();
        result.Todo!.Title.Should().Be("New title");
        result.Todo.IsCompleted.Should().BeTrue();
        db.TodoItems.Single(item => item.Id == todo.Id).Title.Should().Be("New title");
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoDoesNotExist_ReturnsFailure()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.UpdateAsync(123, new TodoItem { Title = "Updated" });

        result.Success.Should().BeFalse();
        result.Todo.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenTitleIsBlank_ReturnsValidationErrorAndKeepsExistingTodo()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var todo = new TodoItem { Title = "Original", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(todo.Id, new TodoItem { Title = "   ", IsCompleted = true });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Title is required.");
        db.TodoItems.Single(item => item.Id == todo.Id).Title.Should().Be("Original");
        db.TodoItems.Single(item => item.Id == todo.Id).IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoExists_RemovesIt()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var todo = new TodoItem { Title = "Delete me", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var deleted = await service.DeleteAsync(todo.Id);

        deleted.Should().BeTrue();
        db.TodoItems.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoDoesNotExist_ReturnsFalse()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var deleted = await service.DeleteAsync(1000);

        deleted.Should().BeFalse();
    }

    [Fact]
    public void NormalizeTitle_WhenInputIsNullOrWhiteSpace_ReturnsNull()
    {
        TodoService.NormalizeTitle(null).Should().BeNull();
        TodoService.NormalizeTitle(" ").Should().BeNull();
        TodoService.NormalizeTitle("\t\n").Should().BeNull();
    }

    [Fact]
    public void NormalizeTitle_WhenTitleHasWhitespace_TrimsIt()
    {
        TodoService.NormalizeTitle("  Finish sprint  ").Should().Be("Finish sprint");
    }
}
