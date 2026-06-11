using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public SensorsController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

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

    [HttpGet("latest/{farmId}")]
    [AuthorizeRole(RoleConstants.Farmer)]
    [RequireFarmOwnership("route", "farmId")]
    public async Task<IActionResult> GetLatestReadings(string farmId)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _sensorService.GetLatestReadingsByFarmAsync(farmId, userId);
        return Ok(ApiResponse<LatestReadingDto>.SuccessResponse(result));
    }

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
