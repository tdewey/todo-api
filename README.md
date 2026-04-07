# Todo API

A production-quality REST API for managing to-do items, built with .NET 9 and ASP.NET Core. Submitted as a take-home assessment.

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

# 2. Run the API (migrations are applied automatically on first start)
dotnet run --project todo-api --launch-profile http
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

Expected output:
```
Passed! - Failed: 0, Passed: 15, Skipped: 0, Total: 15
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
| `PATCH` | `/api/todos/{id}/complete` | — | `200 OK` — completed todo, or `404` |
| `DELETE` | `/api/todos/{id}` | — | `204 No Content`, or `404` |

### Request Shapes

**`CreateTodoDto`**
```json
{
  "title": "Buy groceries",      // required, max 200 chars
  "description": "Milk, eggs"    // optional, max 1000 chars
}
```

**`UpdateTodoDto`**
```json
{
  "title": "Buy groceries",      // required, max 200 chars
  "description": "Milk, eggs",   // optional, max 1000 chars
  "isCompleted": false           // required bool
}
```

**`TodoResponse`** (all endpoints return this shape)
```json
{
  "id": 1,
  "title": "Buy groceries",
  "description": "Milk, eggs",
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

**Repository + Service pattern**
Controllers are thin HTTP handlers. Services own all business logic. Repositories own all data access. This layering keeps each concern isolated and testable independently — services are unit-tested with an in-memory DB, controllers are integration-tested via `WebApplicationFactory`.

**Soft deletes**
`DELETE /api/todos/{id}` sets a `DeletedAt` timestamp rather than removing the row. A global EF Core query filter (`t => t.DeletedAt == null`) automatically excludes soft-deleted items from all queries. This preserves history without any additional application logic.

**SQLite with EF Core migrations**
SQLite was chosen to make the project zero-configuration — no database server to install. Migrations are applied automatically on startup (`db.Database.Migrate()`), so the reviewer can clone and run immediately. The migration to SQL Server would require only a connection string change.

**DataAnnotations over FluentValidation**
DataAnnotations on DTOs is sufficient for the validation needs here (required fields, max lengths). FluentValidation would add expressiveness for complex rules but introduces a dependency and ceremony not warranted at this scale.

**Swagger available in all environments**
`UseSwaggerUI()` is intentionally not gated to `IsDevelopment()`. For a take-home assessment, a reviewer shouldn't need to set environment variables to explore the API.

---

## Trade-offs

| Area | Decision | Reasoning |
|------|----------|-----------|
| Auth | None | Out of scope for this assessment; would add JWT Bearer in a production app |
| Pagination | None | `GET /api/todos` returns all items; acceptable for a demo dataset |
| Purge | No purge endpoint | Soft-deleted rows accumulate indefinitely; a `DELETE /api/todos/{id}/purge` (admin-only) would address this |
| SQLite | Dev + test only | Not suitable for high-concurrency production; swap connection string for SQL Server |
| Sorting | None | Items returned in insertion order; query params for `sortBy`/`sortDir` would be straightforward to add |

---

## What I'd Do Next

1. **Authentication** — JWT Bearer tokens with role-based access (user can only see their own todos)
2. **Pagination** — cursor-based or offset pagination on `GET /api/todos`
3. **Purge endpoint** — `DELETE /api/todos/{id}/purge` for permanent removal of soft-deleted items
4. **Containerization** — `Dockerfile` + `docker-compose.yml` so the reviewer can run with a single command
5. **Structured logging** — Serilog with request/response logging middleware
6. **CI** — GitHub Actions workflow running `dotnet build` + `dotnet test` on every push
