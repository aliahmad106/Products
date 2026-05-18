namespace Products.Api.Middleware;

public record ErrorResponse(
    string TraceId,
    int StatusCode,
    string Message,
    IReadOnlyList<string>? Errors = null
);
