namespace TodoApi.DTOs;

public record TodoResponse
{
  public int Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public bool IsCompleted { get; init; }
  public DateTimeOffset CreatedAt { get; init; }
  public DateTimeOffset UpdatedAt { get; init; }
}
