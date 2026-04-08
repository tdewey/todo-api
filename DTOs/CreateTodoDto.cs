using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public record CreateTodoDto
{
  [Required]
  [MaxLength(200)]
  public string Title { get; init; } = string.Empty;
}
