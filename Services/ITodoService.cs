using TodoApi.DTOs;

namespace TodoApi.Services;

public interface ITodoService
{
  Task<IEnumerable<TodoResponse>> GetAllAsync(bool? isCompleted);
  Task<TodoResponse?> GetByIdAsync(int id);
  Task<TodoResponse> CreateAsync(CreateTodoDto dto);
  Task<TodoResponse?> UpdateAsync(int id, UpdateTodoDto dto);

  Task<bool> DeleteAsync(int id);
}
