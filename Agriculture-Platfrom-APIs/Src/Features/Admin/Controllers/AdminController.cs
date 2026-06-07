using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Admin.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Admin.Controllers;

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

    // ── User management ───────────────────────────────────────────────────────

    // GET /api/admin/users?page=1&pageSize=20
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetAllUsersAsync(pagination);
        return Ok(ApiResponse<PagedResult<UserManagementDto>>.SuccessResponse(result));
    }

    // DELETE /api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("User deleted successfully."));
    }

    // ── System-default validation ranges ─────────────────────────────────────

    // GET /api/admin/validation-ranges
    [HttpGet("validation-ranges")]
    public async Task<IActionResult> GetValidationRanges()
    {
        var ranges = await _adminService.GetValidationRangesAsync();
        return Ok(ApiResponse<List<ValidationRangeDto>>.SuccessResponse(ranges));
    }

    // PUT /api/admin/validation-ranges/{id}
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
