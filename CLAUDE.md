# ToDo API – Claude Code Guide

## Project Overview

A simple CRUD REST API for managing to-do items. Built with .NET 9 and ASP.NET Core, following clean architecture principles with a service layer and repository pattern.

## Technology Stack

| Layer     | Technology                        |
| --------- | --------------------------------- |
| Runtime   | .NET 9                            |
| Language  | C# (nullable enabled)             |
| Framework | ASP.NET Core Web API              |
| ORM       | Entity Framework Core             |
| Database  | SQLite (dev) / SQL Server (prod)  |
| Tests     | xUnit + EF Core InMemory provider |
| Docs      | OpenAPI (built-in)                |

## Project Structure

```
todo-api/
  Controllers/      # Thin HTTP handlers — validate input, delegate to services
  Services/         # Business logic — IService + implementation
  Repositories/     # Data access — IRepository + EF Core implementation
  Models/           # Domain entities (TodoItem, etc.)
  DTOs/             # Request/response shapes (CreateTodoDto, TodoResponse, etc.)
  Data/             # DbContext and EF Core configuration
  Tests/            # xUnit test project (or co-located)
```

## Architecture Rules

- **Controllers are thin.** Validate input and return HTTP responses. No business logic.
- **Services own the logic.** All domain rules and orchestration live in services.
- **Repositories own data access.** EF Core is called only through repositories — never in controllers or services directly.
- **DTOs at the boundary.** Controllers receive DTOs, services work with domain models, repositories persist entities.
- **Always async.** Every I/O operation uses `async`/`await`. No `.Result` or `.Wait()`.

## C# Conventions

- Follow Google's C# Style Guide.
- Nullable reference types are enabled — use `?` where nullable and guard against null.
- Use `record` for DTOs, `class` for entities and services.
- Prefer `var` for locals when the type is obvious from the right-hand side.
- PascalCase for types, methods, and properties. camelCase for parameters and locals.
- Async methods end in `Async` (`GetByIdAsync`, `CreateAsync`).

## Commands

```bash
# Build
dotnet build

# Run (dev)
dotnet run --project todo-api

# Run tests
dotnet test

# Run a single test class
dotnet test --filter "ClassName=TodoServiceTests"

# Add a package
dotnet add package <PackageName>

# EF Core: add migration
dotnet ef migrations add <MigrationName>

# EF Core: update database
dotnet ef database update
```

## Testing Approach

- **xUnit** for all tests.
- **EF Core InMemory provider** replaces the real database in tests — no mocking of repositories needed.
- Test the service layer with a real (in-memory) DbContext, not mocked dependencies.
- Test controllers via integration tests using `WebApplicationFactory<Program>`.
- Each test is self-contained — no shared mutable state between tests.

## Key Patterns

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
  private readonly ITodoService _service;

  public TodosController(ITodoService service) => _service = service;

  [HttpGet("{id}")]
  public async Task<ActionResult<TodoResponse>> GetById(int id)
  {
    var item = await _service.GetByIdAsync(id);
    return item is null ? NotFound() : Ok(item);
  }
}
```

### Service

```csharp
public class TodoService : ITodoService
{
  private readonly ITodoRepository _repository;

  public TodoService(ITodoRepository repository) => _repository = repository;

  public async Task<TodoResponse?> GetByIdAsync(int id)
  {
    var item = await _repository.GetByIdAsync(id);
    return item is null ? null : MapToResponse(item);
  }
}
```

### Repository

```csharp
public class TodoRepository : ITodoRepository
{
  private readonly TodoDbContext _context;

  public TodoRepository(TodoDbContext context) => _context = context;

  public async Task<TodoItem?> GetByIdAsync(int id) =>
    await _context.Todos.FindAsync(id);
}
```

### xUnit Test

```csharp
public class TodoServiceTests
{
  private static TodoDbContext CreateContext()
  {
      var options = new DbContextOptionsBuilder<TodoDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;
      return new TodoDbContext(options);
  }

  [Fact]
  public async Task GetByIdAsync_ReturnsItem_WhenExists()
  {
    // Arrange
    await using var context = CreateContext();
    context.Todos.Add(new TodoItem { Id = 1, Title = "Test" });
    await context.SaveChangesAsync();
    var service = new TodoService(new TodoRepository(context));

    // Act
    var result = await service.GetByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Title);
  }
}
```

## Definition of Done

Before committing any code:

- [ ] `dotnet build` passes with no warnings
- [ ] `dotnet test` passes
- [ ] New endpoints have xUnit tests (success + error cases)
- [ ] Nullable reference warnings resolved
- [ ] No business logic in controllers
- [ ] No direct EF Core calls outside repositories
