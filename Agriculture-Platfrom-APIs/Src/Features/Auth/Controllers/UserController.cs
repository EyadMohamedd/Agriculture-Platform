using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Auth.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

    public UserController(IAuthService authService, IValidator<ChangePasswordDto> changePasswordValidator)
    {
        _authService = authService;
        _changePasswordValidator = changePasswordValidator;
    }

    // GET /api/users/profile  — Farmer | Admin
    [HttpGet("profile")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var user = await _authService.GetProfileAsync(userId);
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.Role,
            user.CreatedAt,
            user.UpdatedAt
        }));
    }

    // PUT /api/users/profile  — Farmer | Admin
    [HttpPut("profile")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var user = await _authService.UpdateProfileAsync(userId, dto);
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.Role,
            user.CreatedAt,
            user.UpdatedAt
        }, "Profile updated successfully."));
    }

    // PUT /api/users/password  — Farmer | Admin
    [HttpPut("password")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var validation = await _changePasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var userId = HttpContext.Items["UserId"]!.ToString()!;
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse.SuccessResponse("Password changed successfully."));
    }
}
