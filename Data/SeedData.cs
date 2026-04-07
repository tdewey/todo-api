using todo_api.Models;

namespace todo_api.Data;

public static class SeedData
{
    public static void Apply(TodoDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        db.Todos.AddRange(
            new TodoItem { Title = "Buy groceries", IsCompleted = false, CreatedAt = now, UpdatedAt = now },
            new TodoItem { Title = "Write unit tests", IsCompleted = true, CreatedAt = now, UpdatedAt = now },
            new TodoItem { Title = "Review pull request", IsCompleted = false, CreatedAt = now, UpdatedAt = now },
            new TodoItem { Title = "Update README", IsCompleted = true, CreatedAt = now, UpdatedAt = now },
            new TodoItem { Title = "Fix login bug", IsCompleted = false, CreatedAt = now, UpdatedAt = now }
        );
        db.SaveChanges();
    }
}
