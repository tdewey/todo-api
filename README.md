# Todo API

A REST API for managing to-do items, built with .NET 9 and ASP.NET Core.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 9 |
| Language | C# (nullable enabled) |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| Database | SQLite |
| Docs | Swagger UI (Swashbuckle) |
| Tests | xUnit + EF Core InMemory / SQLite in-memory |

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- `dotnet-ef` CLI tool (for running migrations manually):

```bash
dotnet tool install --global dotnet-ef --version 9.0.10
```

> **Note:** After installing, add `~/.dotnet/tools` to your PATH if it isn't already:
> ```bash
> export PATH="$PATH:/Users/$USER/.dotnet/tools"
> ```

---

## Setup & Run

```bash
# 1. Clone and enter the repo
git clone <repo-url>
cd todo-api

# 2. Run the API
# * running in the 'seed' profile seeds the database with
#   in-memory data that goes away on shutdown
dotnet run --project todo-api --launch-profile http|https|seed
```

The API starts at `http://localhost:5243`. Swagger UI is available at:

```
http://localhost:5243/swagger
```

No manual database setup is required. The SQLite file (`todos.db`) is created and migrated automatically on startup.

---

## Running Tests

```bash
dotnet test
```

To run a specific test class:
```bash
dotnet test --filter "ClassName=TodoServiceTests"
dotnet test --filter "ClassName=TodosControllerTests"
```

---

## API Reference

All endpoints are also available via the included `todo-api.http` file (compatible with VS Code REST Client and JetBrains HTTP Client) and Swagger UI at `/swagger`.

| Method | Path | Request Body | Success Response |
|--------|------|-------------|-----------------|
| `GET` | `/api/todos` | — | `200 OK` — array of todos |
| `GET` | `/api/todos?isCompleted={bool}` | — | `200 OK` — filtered array |
| `GET` | `/api/todos/{id}` | — | `200 OK` — single todo, or `404` |
| `POST` | `/api/todos` | `CreateTodoDto` | `201 Created` with `Location` header |
| `PUT` | `/api/todos/{id}` | `UpdateTodoDto` | `200 OK` — updated todo, or `404` |
| `DELETE` | `/api/todos/{id}` | — | `204 No Content`, or `404` |

### Request Shapes

**`CreateTodoDto`**
```json
{
  "title": "Buy groceries",      // required, max 200 chars
}
```

**`UpdateTodoDto`**
```json
{
  "title": "Buy groceries",      // required, max 200 chars
  "isCompleted": false           // required bool
}
```

**`TodoResponse`** (all endpoints return this shape)
```json
{
  "id": 1,
  "title": "Buy groceries",
  "isCompleted": false,
  "createdAt": "2026-04-07T18:00:00+00:00",
  "updatedAt": "2026-04-07T18:00:00+00:00"
}
```

### Error Responses

All errors follow [RFC 7807 Problem Details](https://www.rfc-editor.org/rfc/rfc7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["The Title field is required."]
  }
}
```

---

## Design Decisions

#### Repository + Service pattern
Controllers, services, and repositories are separate layers. Controllers handle HTTP and nothing else — they call a service method and return the result. Services own the business rules. Repositories talk to the database. This separation makes each layer testable in isolation. Service tests use EF Core InMemory, controller tests go through `WebApplicationFactory` against a real SQLite connection.

#### Soft deletes
`DELETE /api/todos/{id}` sets a `DeletedAt` timestamp rather than removing the row. A global EF Core query filter (`t => t.DeletedAt == null`) automatically excludes soft-deleted items from all queries. This preserves history without any additional application logic. I went with this partly because it's a sensible default for any API where an audit trail might matter.

#### SQLite with EF Core migrations
SQLite was chosen to make the project low/no-configuration. Migrations are applied automatically on startup (`db.Database.Migrate()`), devs can clone and run immediately. The migration to SQL Server would require only a connection string change.

#### DataAnnotations over FluentValidation
DataAnnotations on the DTOs cover required fields and max lengths. For an MVP the tradeoff is straightforward — FluentValidation is genuinely better for complex validation logic, but there isn't any here.

---

## Trade-offs

| Area | Decision | Reasoning |
|------|----------|-----------|
| Id | Int | Instead of a UUID, this MVP implementation increments an `int` for its Primary Key identifier  |
| Auth | None | Out of scope for this iteration; would add JWT Bearer in future iterations |
| Pagination | None | `GET /api/todos` returns all items; acceptable for an MVP |
| Purge | No purge endpoint | Soft-deleted rows accumulate indefinitely; a `DELETE /api/todos/{id}/purge` (admin-only) would address this |
| SQLite | Dev + test only | Not suitable for high-concurrency production; swap connection string for SQL Server or other RDb |
| Sorting | None | Items returned in insertion order; query params for `sortBy`/`sortDir` would be straightforward to add |

---

## What I'd Do Next

1. **Authentication**: JWT Bearer tokens with role-based access (user can only see their own todos)
2. **Pagination**: cursor-based or offset pagination on `GET /api/todos`
3. **Purge endpoint**: `DELETE /api/todos/{id}/purge` for permanent removal of soft-deleted items
4. **Containerization**: `Dockerfile` + `docker-compose.yml` for single-command startup
5. **Structured logging**: Serilog with request/response logging middleware
6. **CI**: GitHub Actions workflow running `dotnet build` + `dotnet test` on every push
