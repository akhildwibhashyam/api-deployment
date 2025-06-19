using System.Net;
using System.Text.Json;

namespace ProductManagementSystem.Presentation.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public RequestDelegate Next { get; } = next;
    public ILogger<ErrorHandlingMiddleware> Logger { get; } = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await Next(context);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Error processing request {Method} {Path}. TraceId: {TraceId}", 
                context.Request.Method, 
                context.Request.Path, 
                context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            ArgumentException argumentEx => (HttpStatusCode.BadRequest, argumentEx.Message),
            InvalidOperationException invalidOpEx => (HttpStatusCode.BadRequest, invalidOpEx.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized to access this resource."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.ToString()
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
