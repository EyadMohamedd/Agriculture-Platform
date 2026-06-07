using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Alert.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Alert.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Alert.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    // GET /api/alerts?farmId=...&severity=...  — Farmer (own alerts only)
    [HttpGet]
    [AuthorizeRole(RoleConstants.Farmer)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] string? farmId,
        [FromQuery] string? severity,
        [FromQuery] PaginationParams pagination)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _alertService.GetAlertsAsync(userId, pagination, farmId, severity);
        return Ok(ApiResponse<PagedResult<AlertResponseDto>>.SuccessResponse(result));
    }

}
