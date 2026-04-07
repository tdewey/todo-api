using todo_api.DTOs;
using todo_api.Models;
using todo_api.Repositories;

namespace todo_api.Services;

public class TodoService(ITodoRepository repository) : ITodoService
{
    public async Task<IEnumerable<TodoResponse>> GetAllAsync(bool? isCompleted)
    {
        var items = await repository.GetAllAsync(isCompleted);
        return items.Select(MapToResponse);
    }

    public async Task<TodoResponse?> GetByIdAsync(int id)
    {
        var item = await repository.GetByIdAsync(id);
        return item is null ? null : MapToResponse(item);
    }

    public async Task<TodoResponse> CreateAsync(CreateTodoDto dto)
    {
        var now = DateTimeOffset.UtcNow;
        var item = new TodoItem
        {
            Title = dto.Title,
            IsCompleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await repository.CreateAsync(item);
        return MapToResponse(created);
    }

    public async Task<TodoResponse?> UpdateAsync(int id, UpdateTodoDto dto)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null) return null;

        item.Title = dto.Title;
        item.IsCompleted = dto.IsCompleted;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await repository.UpdateAsync(item);
        return MapToResponse(updated);
    }

    public async Task<TodoResponse?> CompleteAsync(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null) return null;

        item.IsCompleted = true;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await repository.UpdateAsync(item);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null) return false;

        await repository.DeleteAsync(item);
        return true;
    }

    private static TodoResponse MapToResponse(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        IsCompleted = item.IsCompleted,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
