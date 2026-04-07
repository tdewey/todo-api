using System.ComponentModel.DataAnnotations;

namespace todo_api.DTOs;

public record CreateTodoDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;
}
