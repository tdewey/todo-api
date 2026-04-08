using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unhandled exception");
      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      context.Response.ContentType = "application/problem+json";
      var problem = new ProblemDetails
      {
        Status = StatusCodes.Status500InternalServerError,
        Title = "An unexpected error occurred."
      };
      await context.Response.WriteAsJsonAsync(problem);
    }
  }
}
