using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using todo_api.Data;
using todo_api.Middleware;
using todo_api.Repositories;
using todo_api.Services;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" }));

// In the Seed environment we use a named shared-cache SQLite in-memory database.
// SQLite destroys such a database the moment all connections to it are closed.
// We open one persistent connection here and pass it to EF Core so the database
// stays alive for the entire lifetime of the process.
SqliteConnection? seedConnection = null;
if (builder.Environment.IsEnvironment("Seed"))
{
    seedConnection = new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    seedConnection.Open();
    builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlite(seedConnection));
}
else
{
    builder.Services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

if (app.Environment.IsEnvironment("Seed"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.EnsureCreated();
    SeedData.Apply(db);
}
else if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

seedConnection?.Dispose();

public partial class Program { }
