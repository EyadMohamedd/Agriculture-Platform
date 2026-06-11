using System.Diagnostics;

namespace AgriculturalMonitorSystem.Api.Middleware;

/// <summary>
/// Layer 2 — Logs every request: method, path, status code, and execution time.
/// Runs inside ErrorHandlingMiddleware so exceptions don't stop logging.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var userId = context.Items.TryGetValue("UserId", out var uid) ? uid?.ToString() : "anonymous";

        _logger.LogInformation(
            "{Method} {Path} → {StatusCode} in {ElapsedMs}ms (user={UserId})",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            userId);
    }
}
