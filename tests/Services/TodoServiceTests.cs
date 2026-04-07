using Microsoft.EntityFrameworkCore;
using todo_api.Data;
using todo_api.DTOs;
using todo_api.Models;
using todo_api.Repositories;
using todo_api.Services;

namespace todo_api.Tests.Services;

public class TodoServiceTests
{
    private static TodoDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static TodoService CreateService(TodoDbContext context) =>
        new(new TodoRepository(context));

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems_WhenNoFilter()
    {
        await using var context = CreateContext();
        context.Todos.AddRange(
            new TodoItem { Title = "A", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new TodoItem { Title = "B", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new TodoItem { Title = "C", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetAllAsync(null);

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCompleted_WhenFilterTrue()
    {
        await using var context = CreateContext();
        context.Todos.AddRange(
            new TodoItem { Title = "Done", IsCompleted = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new TodoItem { Title = "Not done", IsCompleted = false, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetAllAsync(true);

        var list = result.ToList();
        Assert.Single(list);
        Assert.Equal("Done", list[0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenExists()
    {
        await using var context = CreateContext();
        context.Todos.Add(new TodoItem { Title = "Test", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();
        var id = context.Todos.First().Id;
        var service = CreateService(context);

        var result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesItem_WithCorrectDefaults()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var before = DateTimeOffset.UtcNow;

        var result = await service.CreateAsync(new CreateTodoDto { Title = "New" });

        Assert.Equal("New", result.Title);
        Assert.False(result.IsCompleted);
        Assert.True(result.CreatedAt >= before);
        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesItem_WhenExists()
    {
        await using var context = CreateContext();
        context.Todos.Add(new TodoItem { Title = "Old", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();
        var id = context.Todos.First().Id;
        var service = CreateService(context);
        var before = DateTimeOffset.UtcNow;

        var result = await service.UpdateAsync(id, new UpdateTodoDto { Title = "New", IsCompleted = true });

        Assert.NotNull(result);
        Assert.Equal("New", result.Title);
        Assert.True(result.IsCompleted);
        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.UpdateAsync(999, new UpdateTodoDto { Title = "X" });

        Assert.Null(result);
    }

    [Fact]
    public async Task CompleteAsync_SetsIsCompleted_WhenExists()
    {
        await using var context = CreateContext();
        context.Todos.Add(new TodoItem { Title = "Task", IsCompleted = false, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();
        var id = context.Todos.First().Id;
        var service = CreateService(context);
        var before = DateTimeOffset.UtcNow;

        var result = await service.CompleteAsync(id);

        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesItem_WhenExists()
    {
        await using var context = CreateContext();
        context.Todos.Add(new TodoItem { Title = "To delete", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();
        var id = context.Todos.First().Id;
        var service = CreateService(context);

        var result = await service.DeleteAsync(id);

        Assert.True(result);

        // Item excluded from normal queries (global query filter)
        var all = await service.GetAllAsync(null);
        Assert.Empty(all);

        // But DeletedAt is set on the underlying entity (bypass filter)
        var raw = await context.Todos.IgnoreQueryFilters().FirstAsync(t => t.Id == id);
        Assert.NotNull(raw.DeletedAt);
    }
}
