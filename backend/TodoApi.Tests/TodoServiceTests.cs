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

    [Fact]
    public async Task UserMethods_CreateTrimmedHomonymsAndListByNameThenId()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var second = await service.CreateUserAsync("Ana");
        var first = await service.CreateUserAsync("  Ana  ");
        var another = await service.CreateUserAsync("Bea");
        var users = await service.GetUsersAsync();

        second.Success.Should().BeTrue();
        first.Success.Should().BeTrue();
        another.Success.Should().BeTrue();
        first.User!.Name.Should().Be("Ana");
        users.Select(user => user.Id).Should().Equal(second.User!.Id, first.User.Id, another.User!.Id);
        (await service.GetUserByIdAsync(first.User.Id)).Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateUserAsync_WhenNameIsBlank_ReturnsValidationError(string? name)
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.CreateUserAsync(name);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("El nombre del usuario es obligatorio.");
        db.TodoUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUserAsync_WhenNameIsBlank_KeepsExistingName()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateUserAsync("Original");

        var result = await service.UpdateUserAsync(created.User!.Id, "  ");

        result.Success.Should().BeFalse();
        (await service.GetUserByIdAsync(created.User.Id))!.Name.Should().Be("Original");
    }

    [Fact]
    public async Task DeleteUserAsync_WhenAssignedToTodo_ReturnsInUseAndKeepsBoth()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var user = await service.CreateUserAsync("Ana");
        var createdTodo = await service.CreateAsync(new TodoRequest("Task", UserId: user.User!.Id));

        var result = await service.DeleteUserAsync(user.User.Id);

        result.Exists.Should().BeTrue();
        result.InUse.Should().BeTrue();
        (await service.GetUserByIdAsync(user.User.Id)).Should().NotBeNull();
        (await service.GetByIdAsync(createdTodo.Todo!.Id))!.UserId.Should().Be(user.User.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUnused_RemovesUser()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var user = await service.CreateUserAsync("Ana");

        var result = await service.DeleteUserAsync(user.User!.Id);

        result.Exists.Should().BeTrue();
        result.InUse.Should().BeFalse();
        (await service.GetUserByIdAsync(user.User.Id)).Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_RenamingUserIsReflectedInAssignedTodos()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var user = await service.CreateUserAsync("Ana");
        var todo = await service.CreateAsync(new TodoRequest("Task", UserId: user.User!.Id));

        var updated = await service.UpdateUserAsync(user.User.Id, "  Ana García  ");
        var reloadedTodo = await service.GetByIdAsync(todo.Todo!.Id);

        updated.Success.Should().BeTrue();
        updated.User!.Name.Should().Be("Ana García");
        reloadedTodo!.User!.Name.Should().Be("Ana García");
    }

    [Fact]
    public async Task TodoMethods_AssignChangeAndClearUser()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var first = await service.CreateUserAsync("Ana");
        var second = await service.CreateUserAsync("Bea");
        var created = await service.CreateAsync(new TodoRequest("Task", UserId: first.User!.Id));

        created.Todo!.UserId.Should().Be(first.User.Id);
        created.Todo.User!.Name.Should().Be("Ana");

        var changed = await service.UpdateAsync(
            created.Todo.Id,
            new TodoRequest("Updated", UserId: second.User!.Id));
        changed.Todo!.UserId.Should().Be(second.User.Id);
        changed.Todo.User!.Name.Should().Be("Bea");

        var cleared = await service.UpdateAsync(created.Todo.Id, new TodoRequest("Updated"));
        cleared.Todo!.UserId.Should().BeNull();
        cleared.Todo.User.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task CreateAsync_WhenUserDoesNotExist_ReturnsValidationError(int userId)
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.CreateAsync(new TodoRequest("Task", UserId: userId));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(TodoService.InvalidUserError);
        db.TodoItems.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task UpdateAsync_WhenUserDoesNotExist_DoesNotMutateTodo(int userId)
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var todo = new TodoItem { Title = "Original", IsCompleted = false };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(
            todo.Id,
            new TodoRequest("Changed", true, UserId: userId));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(TodoService.InvalidUserError);
        var persisted = await service.GetByIdAsync(todo.Id);
        persisted!.Title.Should().Be("Original");
        persisted.IsCompleted.Should().BeFalse();
        persisted.UserId.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        (await service.GetUserByIdAsync(123)).Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ReturnsNotFoundResult()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.UpdateUserAsync(123, "Valid name");

        result.Success.Should().BeFalse();
        result.User.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ReturnsNotFoundResult()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.DeleteUserAsync(123);

        result.Exists.Should().BeFalse();
        result.InUse.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenTodoDoesNotExistAndInputIsInvalid_PreservesNotFoundPriority()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.UpdateAsync(
            123,
            new TodoRequest(" ", CategoryId: 999, UserId: 999));

        result.Success.Should().BeFalse();
        result.Todo.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_PreservesEveryTodoField()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var category = await service.CreateCategoryAsync("Work");
        var user = await service.CreateUserAsync("Ana");
        var createdAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var todo = new TodoItem
        {
            Title = "Original",
            IsCompleted = true,
            CreatedAt = createdAt,
            CategoryId = category.Category!.Id,
            UserId = user.User!.Id
        };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(
            todo.Id,
            new TodoRequest("Changed", false, CategoryId: 999, UserId: user.User.Id));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("La categoría seleccionada no existe.");
        var persisted = await service.GetByIdAsync(todo.Id);
        persisted!.Title.Should().Be("Original");
        persisted.IsCompleted.Should().BeTrue();
        persisted.CreatedAt.Should().Be(createdAt);
        persisted.CategoryId.Should().Be(category.Category.Id);
        persisted.UserId.Should().Be(user.User.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_PreservesCategoryAndEveryTodoField()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var category = await service.CreateCategoryAsync("Work");
        var user = await service.CreateUserAsync("Ana");
        var createdAt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var todo = new TodoItem
        {
            Title = "Original",
            IsCompleted = true,
            CreatedAt = createdAt,
            CategoryId = category.Category!.Id,
            UserId = user.User!.Id
        };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(
            todo.Id,
            new TodoRequest("Changed", false, CategoryId: category.Category.Id, UserId: 999));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(TodoService.InvalidUserError);
        var persisted = await service.GetByIdAsync(todo.Id);
        persisted!.Title.Should().Be("Original");
        persisted.IsCompleted.Should().BeTrue();
        persisted.CreatedAt.Should().Be(createdAt);
        persisted.CategoryId.Should().Be(category.Category.Id);
        persisted.UserId.Should().Be(user.User.Id);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenAssignedToCompletedTodo_ReturnsInUseAndKeepsBoth()
    {
        await using var db = CreateDbContext();
        var service = new TodoService(db);
        var user = await service.CreateUserAsync("Ana");
        var createdTodo = await service.CreateAsync(
            new TodoRequest("Completed", true, UserId: user.User!.Id));

        var result = await service.DeleteUserAsync(user.User.Id);

        result.Exists.Should().BeTrue();
        result.InUse.Should().BeTrue();
        (await service.GetUserByIdAsync(user.User.Id)).Should().NotBeNull();
        var persistedTodo = await service.GetByIdAsync(createdTodo.Todo!.Id);
        persistedTodo!.IsCompleted.Should().BeTrue();
        persistedTodo.UserId.Should().Be(user.User.Id);
    }
}
