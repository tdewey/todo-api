using TodoApi.Models;

namespace TodoApi.Repositories;

public interface ITodoRepository
{
  Task<IEnumerable<TodoItem>> GetAllAsync(bool? isCompleted);
  Task<TodoItem?> GetByIdAsync(int id);
  Task<TodoItem> CreateAsync(TodoItem item);
  Task<TodoItem> UpdateAsync(TodoItem item);
  Task DeleteAsync(TodoItem item);
}
