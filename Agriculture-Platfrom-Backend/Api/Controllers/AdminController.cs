using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Controllers;

[ApiController]
[Route("api/admin")]
[AuthorizeRole(RoleConstants.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IValidator<ValidationRangeDto> _rangeValidator;

    public AdminController(IAdminService adminService, IValidator<ValidationRangeDto> rangeValidator)
    {
        _adminService = adminService;
        _rangeValidator = rangeValidator;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetAllUsersAsync(pagination);
        return Ok(ApiResponse<PagedResult<UserManagementDto>>.SuccessResponse(result));
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("User deleted successfully."));
    }

    [HttpGet("validation-ranges")]
    public async Task<IActionResult> GetValidationRanges()
    {
        var ranges = await _adminService.GetValidationRangesAsync();
        return Ok(ApiResponse<List<ValidationRangeDto>>.SuccessResponse(ranges));
    }

    [HttpPut("validation-ranges/{id}")]
    public async Task<IActionResult> UpdateValidationRange(string id, [FromBody] ValidationRangeDto dto)
    {
        var validation = await _rangeValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var result = await _adminService.UpdateValidationRangeAsync(id, dto);
        return Ok(ApiResponse<ValidationRangeDto>.SuccessResponse(result, "Validation range updated successfully."));
    }
}
