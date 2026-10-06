using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
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

        var result = await service.CreateAsync(new TodoRequest("   "));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("El título es obligatorio.");
        result.Todo.Should().BeNull();
        db.TodoItems.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WhenTitleHasWhitespace_TrimsAndPersists()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var createdAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var result = await service.CreateAsync(new TodoRequest("  Buy milk  ", true, CreatedAt: createdAt));

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
        var result = await service.CreateAsync(new TodoRequest("Write tests"));
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

        var result = await service.UpdateAsync(todo.Id, new TodoRequest("  New title  ", true));

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

        var result = await service.UpdateAsync(123, new TodoRequest("Updated"));

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

        var result = await service.UpdateAsync(todo.Id, new TodoRequest("   ", true));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("El título es obligatorio.");
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
    public async Task CreateCategoryAsync_TrimsNameAndRejectsDuplicatesIgnoringCase()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var created = await service.CreateCategoryAsync("  Work  ");
        var duplicate = await service.CreateCategoryAsync("work");

        created.Success.Should().BeTrue();
        created.Category!.Name.Should().Be("Work");
        duplicate.Success.Should().BeFalse();
        duplicate.ErrorMessage.Should().Be(TodoService.DuplicateCategoryNameError);
        db.TodoCategories.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryDoesNotExist_ReturnsValidationError()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.CreateAsync(new TodoRequest("Task", CategoryId: 999));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        db.TodoItems.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_AssignsCategoryAndDeleteCategoryIsBlockedWhileInUse()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var categoryResult = await service.CreateCategoryAsync("Work");
        var todo = new TodoItem { Title = "Task" };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var updated = await service.UpdateAsync(todo.Id, new TodoRequest("Task", CategoryId: categoryResult.Category!.Id));
        var deleted = await service.DeleteCategoryAsync(categoryResult.Category.Id);

        updated.Success.Should().BeTrue();
        updated.Todo!.CategoryId.Should().Be(categoryResult.Category.Id);
        updated.Todo.Category!.Name.Should().Be("Work");
        deleted.Exists.Should().BeTrue();
        deleted.InUse.Should().BeTrue();
        db.TodoCategories.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenUnused_RemovesCategory()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var category = await service.CreateCategoryAsync("Work");

        var result = await service.DeleteCategoryAsync(category.Category!.Id);

        result.Exists.Should().BeTrue();
        result.InUse.Should().BeFalse();
        db.TodoCategories.Should().BeEmpty();
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

    [Fact]
    public void NormalizeCategoryName_WhenNameIsBlank_ReturnsNull()
    {
        TodoService.NormalizeCategoryName("  ").Should().BeNull();
        TodoService.NormalizeCategoryName(" Work ").Should().Be("Work");
    }
}
