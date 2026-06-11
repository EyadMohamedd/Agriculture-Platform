using System.Text.Json;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Middleware;

/// <summary>
/// Layer 5 — Enforces farm ownership for endpoints decorated with [RequireFarmOwnership].
/// Admins bypass this check automatically.
/// Extracts the farm ID from route or query parameters as configured by the attribute.
/// </summary>
public class FarmOwnershipMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FarmOwnershipMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var ownershipAttr = endpoint?.Metadata.GetMetadata<RequireFarmOwnershipAttribute>();

        if (ownershipAttr != null)
        {
            var userRole = context.Items.TryGetValue("UserRole", out var r) ? r?.ToString() : null;

            if (userRole == RoleConstants.Admin)
            {
                await _next(context);
                return;
            }

            var userId  = context.Items.TryGetValue("UserId", out var u) ? u?.ToString() : null;
            var farmId  = ExtractFarmId(context, ownershipAttr);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(farmId))
            {
                await WriteForbiddenAsync(context, "Could not determine resource ownership.");
                return;
            }

            var ownershipService = context.RequestServices.GetRequiredService<IResourceOwnershipService>();
            var isOwner = await ownershipService.IsFarmOwnerAsync(userId, farmId);
            if (!isOwner)
            {
                await WriteForbiddenAsync(context, "You do not own this farm.");
                return;
            }
        }

        await _next(context);
    }

    private static string? ExtractFarmId(HttpContext context, RequireFarmOwnershipAttribute attr)
    {
        return attr.FarmIdSource.ToLowerInvariant() switch
        {
            "route" => context.Request.RouteValues.TryGetValue(attr.FarmIdPath, out var rv)
                        ? rv?.ToString()
                        : null,
            "query" => context.Request.Query.TryGetValue(attr.FarmIdPath, out var qv)
                        ? qv.ToString()
                        : null,
            _       => null
        };
    }

    private static async Task WriteForbiddenAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(ApiResponse.FailureResponse(message), JsonOptions));
    }
}
