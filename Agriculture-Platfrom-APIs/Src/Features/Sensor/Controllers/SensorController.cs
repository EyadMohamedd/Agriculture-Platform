using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Sensor.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public SensorController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

    // GET /api/sensors/readings?farmId=...&sensorType=...&startDate=...&endDate=...
    [HttpGet("readings")]
    [AuthorizeRole(RoleConstants.Farmer)]
    public async Task<IActionResult> GetReadings(
        [FromQuery] string? farmId,
        [FromQuery] string? sensorType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] PaginationParams pagination)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _sensorService.GetReadingsAsync(farmId, userId, pagination, sensorType, startDate, endDate);
        return Ok(ApiResponse<PagedResult<SensorReadingDto>>.SuccessResponse(result));
    }

    // GET /api/sensors/latest/{farmId}  — Farmer (own farm only)
    [HttpGet("latest/{farmId}")]
    [AuthorizeRole(RoleConstants.Farmer)]
    [RequireFarmOwnership("route", "farmId")]
    public async Task<IActionResult> GetLatestReadings(string farmId)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _sensorService.GetLatestReadingsByFarmAsync(farmId, userId);
        return Ok(ApiResponse<LatestReadingDto>.SuccessResponse(result));
    }

    // GET /api/sensors/statistics/{farmId}?sensorType=temperature&from=...&to=...
    [HttpGet("statistics/{farmId}")]
    [AuthorizeRole(RoleConstants.Farmer)]
    [RequireFarmOwnership("route", "farmId")]
    public async Task<IActionResult> GetStatistics(
        string farmId,
        [FromQuery] string sensorType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var userId   = HttpContext.Items["UserId"]!.ToString()!;
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate   = to   ?? DateTime.UtcNow;

        var result = await _sensorService.GetStatisticsAsync(farmId, sensorType, fromDate, toDate, userId);
        return Ok(ApiResponse<SensorStatisticsDto>.SuccessResponse(result));
    }
}
