using System.Text.Json;
using AgriculturalMonitorSystem.Shared.Helpers;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Middleware;

/// <summary>
/// Layer 3 — Validates JWT and populates HttpContext.Items with UserId and UserRole.
/// Only enforces 401 when the endpoint is decorated with [AuthorizeRole].
/// Public endpoints (register, login, etc.) pass through without a token.
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtHelper _jwtHelper;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthMiddleware(RequestDelegate next, JwtHelper jwtHelper)
    {
        _next = next;
        _jwtHelper = jwtHelper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var requiresAuth = endpoint?.Metadata.GetMetadata<AuthorizeRoleAttribute>() != null;

        var token = ExtractBearerToken(context);
        if (token != null)
        {
            var principal = _jwtHelper.ValidateToken(token);
            if (principal != null)
            {
                context.Items["UserId"]   = principal.FindFirst("userId")?.Value
                                         ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                context.Items["UserRole"] = principal.FindFirst("role")?.Value
                                         ?? principal.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            }
            else if (requiresAuth)
            {
                await WriteUnauthorizedAsync(context, "Token is invalid or expired.");
                return;
            }
        }
        else if (requiresAuth)
        {
            await WriteUnauthorizedAsync(context, "Authentication token is required.");
            return;
        }

        await _next(context);
    }

    private static string? ExtractBearerToken(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return header["Bearer ".Length..].Trim();
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(ApiResponse.FailureResponse(message), JsonOptions));
    }
}
