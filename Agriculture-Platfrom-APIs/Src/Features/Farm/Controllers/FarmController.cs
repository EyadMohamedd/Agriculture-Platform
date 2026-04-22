using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Farm.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Controllers;

[ApiController]
[Route("api/farms")]
public class FarmController : ControllerBase
{
    private readonly IFarmService _farmService;
    private readonly IValidator<CreateFarmDto> _createValidator;
    private readonly IValidator<UpdateFarmDto> _updateValidator;
    private readonly IValidator<FarmValidationRangeDto> _rangeValidator;

    public FarmController(
        IFarmService farmService,
        IValidator<CreateFarmDto> createValidator,
        IValidator<UpdateFarmDto> updateValidator,
        IValidator<FarmValidationRangeDto> rangeValidator)
    {
        _farmService = farmService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _rangeValidator = rangeValidator;
    }

    // GET /api/farms  — Farmer (own) | Admin (all)
    [HttpGet]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> GetFarms([FromQuery] PaginationParams pagination)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var result = await _farmService.GetFarmsAsync(userId, userRole, pagination);
        return Ok(ApiResponse<PagedResult<FarmResponseDto>>.SuccessResponse(result));
    }

    // POST /api/farms  — Farmer | Admin (auto-creates 5 sensors)
    [HttpPost]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> CreateFarm([FromBody] CreateFarmDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var result = await _farmService.CreateFarmAsync(dto, userId);
        return StatusCode(201, ApiResponse<FarmResponseDto>.SuccessResponse(result,
            "Farm created successfully. 5 sensors have been automatically created."));
    }

    // PUT /api/farms/{id}  — Farmer (own) | Admin (any)
    [HttpPut("{id}")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    [RequireFarmOwnership("route", "id")]
    public async Task<IActionResult> UpdateFarm(string id, [FromBody] UpdateFarmDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var result = await _farmService.UpdateFarmAsync(id, dto, userId, userRole);
        return Ok(ApiResponse<FarmResponseDto>.SuccessResponse(result, "Farm updated successfully."));
    }

    // DELETE /api/farms/{id}  — Farmer (own) | Admin (any). Cascades to sensors/readings/alerts.
    [HttpDelete("{id}")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    [RequireFarmOwnership("route", "id")]
    public async Task<IActionResult> DeleteFarm(string id)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        await _farmService.DeleteFarmAsync(id, userId, userRole);
        return Ok(ApiResponse.SuccessResponse("Farm and all associated data deleted successfully."));
    }

    // ── Farm-specific validation range overrides ──────────────────────────────
    // Farmers can customise thresholds for their own farm.
    // If no override exists, the system falls back to global defaults managed by Admin.

    // GET /api/farms/{farmId}/validation-ranges  — Farmer (own) | Admin (any)
    [HttpGet("{farmId}/validation-ranges")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    [RequireFarmOwnership("route", "farmId")]
    public async Task<IActionResult> GetFarmValidationRanges(string farmId)
    {
        var userId   = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var ranges = await _farmService.GetFarmValidationRangesAsync(farmId, userId, userRole);
        return Ok(ApiResponse<List<FarmValidationRangeDto>>.SuccessResponse(ranges));
    }

    // POST /api/farms/{farmId}/validation-ranges  — Farmer (own) | Admin (any)
    // Upserts (creates or updates) the override for a sensor type.
    [HttpPost("{farmId}/validation-ranges")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    [RequireFarmOwnership("route", "farmId")]
    public async Task<IActionResult> UpsertFarmValidationRange(string farmId, [FromBody] FarmValidationRangeDto dto)
    {
        var validation = await _rangeValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var userId   = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var result = await _farmService.UpsertFarmValidationRangeAsync(farmId, dto, userId, userRole);
        return Ok(ApiResponse<FarmValidationRangeDto>.SuccessResponse(result, "Validation range saved successfully."));
    }

    // DELETE /api/farms/{farmId}/validation-ranges/{rangeId}  — Farmer (own) | Admin (any)
    // Removes the override so readings fall back to global defaults.
    [HttpDelete("{farmId}/validation-ranges/{rangeId}")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    [RequireFarmOwnership("route", "farmId")]
    public async Task<IActionResult> DeleteFarmValidationRange(string farmId, string rangeId)
    {
        var userId   = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        await _farmService.DeleteFarmValidationRangeAsync(farmId, rangeId, userId, userRole);
        return Ok(ApiResponse.SuccessResponse("Validation range override removed. System default will now apply."));
    }
}
