using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

public class TodoRepository(TodoDbContext context) : ITodoRepository
{
  public async Task<IEnumerable<TodoItem>> GetAllAsync(bool? isCompleted)
  {
    var query = context.Todos.AsNoTracking();

    if (isCompleted.HasValue)
      query = query.Where(t => t.IsCompleted == isCompleted.Value);

    return await query.ToListAsync();
  }

  public async Task<TodoItem?> GetByIdAsync(int id)
  {
    return await context.Todos.FindAsync(id);
  }

  public async Task<TodoItem> CreateAsync(TodoItem item)
  {
    context.Todos.Add(item);
    await context.SaveChangesAsync();
    return item;
  }

  public async Task<TodoItem> UpdateAsync(TodoItem item)
  {
    await context.SaveChangesAsync();
    return item;
  }

  public async Task DeleteAsync(TodoItem item)
  {
    item.DeletedAt = DateTimeOffset.UtcNow;
    await context.SaveChangesAsync();
  }
}
