using System.Text.Json;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Middleware;

/// <summary>
/// Layer 4 — Enforces role requirements declared by [AuthorizeRole].
/// Runs after AuthMiddleware so HttpContext.Items["UserRole"] is already set.
/// Returns 403 if the authenticated user's role is not in the allowed list.
/// </summary>
public class RoleMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RoleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var authorizeAttr = endpoint?.Metadata.GetMetadata<AuthorizeRoleAttribute>();

        if (authorizeAttr != null)
        {
            var userRole = context.Items.TryGetValue("UserRole", out var r) ? r?.ToString() : null;

            if (string.IsNullOrEmpty(userRole) || !authorizeAttr.Roles.Contains(userRole))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        ApiResponse.FailureResponse("You do not have permission to access this resource."),
                        JsonOptions));
                return;
            }
        }

        await _next(context);
    }
}
