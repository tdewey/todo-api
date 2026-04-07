using todo_api.DTOs;

namespace todo_api.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoResponse>> GetAllAsync(bool? isCompleted);
    Task<TodoResponse?> GetByIdAsync(int id);
    Task<TodoResponse> CreateAsync(CreateTodoDto dto);
    Task<TodoResponse?> UpdateAsync(int id, UpdateTodoDto dto);
    Task<TodoResponse?> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}
