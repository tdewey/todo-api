using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

public class TodoItem
{
  public int Id { get; set; }

  [Required]
  [MaxLength(200)]
  public string Title { get; set; } = string.Empty;

  public bool IsCompleted { get; set; }

  public DateTimeOffset CreatedAt { get; set; }

  public DateTimeOffset UpdatedAt { get; set; }

  public DateTimeOffset? DeletedAt { get; set; }
}
