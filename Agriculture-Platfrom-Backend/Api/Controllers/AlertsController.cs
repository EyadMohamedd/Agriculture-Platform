using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

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
