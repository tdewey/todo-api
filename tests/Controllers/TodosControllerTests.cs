using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Tests.Controllers;

public class TodoApiFactory : WebApplicationFactory<Program>
{
  private readonly SqliteConnection _connection;

  public TodoApiFactory()
  {
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");
    builder.ConfigureTestServices(services =>
    {
      services.RemoveAll<DbContextOptions<TodoDbContext>>();
      services.AddDbContext<TodoDbContext>(opt => opt.UseSqlite(_connection));
    });
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (disposing) _connection.Dispose();
  }
}

public class TodosControllerTests : IClassFixture<TodoApiFactory>
{
  private readonly TodoApiFactory _factory;

  public TodosControllerTests(TodoApiFactory factory)
  {
    _factory = factory;

    // Ensure schema is created for the in-memory SQLite DB
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.EnsureCreated();
  }

  private HttpClient CreateClient() => _factory.CreateClient();

  private async Task SeedAsync(TodoItem item)
  {
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    context.Todos.Add(item);
    await context.SaveChangesAsync();
  }

  [Fact]
  public async Task GetAll_Returns200_WithItems()
  {
    await SeedAsync(new TodoItem {
      Title = "Seeded",
      CreatedAt = DateTimeOffset.UtcNow,
      UpdatedAt = DateTimeOffset.UtcNow
    });
    var client = CreateClient();

    var response = await client.GetAsync("/api/todos");
    var items = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(items);
    Assert.Contains(items, t => t.Title == "Seeded");
  }

  [Fact]
  public async Task GetById_Returns404_WhenNotFound()
  {
    var client = CreateClient();

    var response = await client.GetAsync("/api/todos/99999");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task Create_Returns400_WhenTitleMissing()
  {
    var client = CreateClient();

    var response = await client.PostAsJsonAsync("/api/todos", new { description = "No title" });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Create_Returns201_WithLocation()
  {
    var client = CreateClient();

    var response = await client.PostAsJsonAsync("/api/todos", new CreateTodoDto {
      Title = "Integration test todo"
    });
    var created = await response.Content.ReadFromJsonAsync<TodoResponse>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.NotNull(created);
    Assert.Equal("Integration test todo", created.Title);
    Assert.False(created.IsCompleted);
  }

  [Fact]
  public async Task Delete_Returns204_WhenDeleted()
  {
    await SeedAsync(new TodoItem {
      Title = "To delete",
      CreatedAt = DateTimeOffset.UtcNow,
      UpdatedAt = DateTimeOffset.UtcNow
    });
    var client = CreateClient();
    var items = await client.GetFromJsonAsync<List<TodoResponse>>("/api/todos");
    var id = items!.First(t => t.Title == "To delete").Id;

    var response = await client.DeleteAsync($"/api/todos/{id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }
}
