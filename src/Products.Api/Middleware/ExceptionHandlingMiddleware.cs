using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Products.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed.",
                validationEx.Errors.Select(e => e.ErrorMessage).ToList() as IReadOnlyList<string>
            ),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "Invalid credentials.",
                null as IReadOnlyList<string>
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                null as IReadOnlyList<string>
            )
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception. TraceId: {TraceId}", traceId);
        }

        var response = new ErrorResponse(traceId, statusCode, message, errors);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
