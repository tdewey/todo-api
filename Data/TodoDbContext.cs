using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
  public DbSet<TodoItem> Todos => Set<TodoItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<TodoItem>(entity =>
    {
      entity.HasKey(t => t.Id);
      entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
      entity.HasQueryFilter(t => t.DeletedAt == null);
    });
  }
}
