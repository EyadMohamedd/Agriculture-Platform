using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Controllers;

[ApiController]
[Route("api/farms")]
public class FarmsController : ControllerBase
{
    private readonly IFarmService _farmService;
    private readonly IValidator<CreateFarmDto> _createValidator;
    private readonly IValidator<UpdateFarmDto> _updateValidator;
    private readonly IValidator<FarmValidationRangeDto> _rangeValidator;

    public FarmsController(
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

    [HttpGet]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> GetFarms([FromQuery] PaginationParams pagination)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var userRole = HttpContext.Items["UserRole"]!.ToString()!;
        var result = await _farmService.GetFarmsAsync(userId, userRole, pagination);
        return Ok(ApiResponse<PagedResult<FarmResponseDto>>.SuccessResponse(result));
    }

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
