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

    // GET /api/alerts  — Farmer (own) | Admin (all)
    [HttpGet]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> GetAlerts([FromQuery] PaginationParams pagination)
    {
        var userId   = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var result = await _alertService.GetAlertsAsync(userId, userRole, pagination);
        return Ok(ApiResponse<PagedResult<AlertResponseDto>>.SuccessResponse(result));
    }

}
