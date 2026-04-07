using System.ComponentModel.DataAnnotations;

namespace todo_api.DTOs;

public record UpdateTodoDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    public bool IsCompleted { get; init; }
}
