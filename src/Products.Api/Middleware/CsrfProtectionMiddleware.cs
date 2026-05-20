namespace Products.Api.Middleware;

/// <summary>
/// CSRF protection for cookie-based authentication.
/// Requires a custom header (X-Requested-With) on state-changing requests.
/// Browsers won't send custom headers cross-origin without CORS preflight approval.
/// </summary>
public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    public CsrfProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!SafeMethods.Contains(context.Request.Method) &&
            context.Request.Cookies.ContainsKey("access_token"))
        {
            // For cookie-authenticated state-changing requests, require custom header
            if (!context.Request.Headers.ContainsKey("X-Requested-With"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Missing CSRF header." });
                return;
            }
        }

        await _next(context);
    }
}
