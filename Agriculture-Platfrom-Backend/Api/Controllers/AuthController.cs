using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.Services.Interfaces;
using AgriculturalMonitorSystem.Api.Attributes;
using AgriculturalMonitorSystem.Application.Constants;
using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ChangePasswordDto> changePasswordValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpPost("api/auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var user = await _authService.RegisterAsync(dto);
        return StatusCode(201, ApiResponse<object>.SuccessResponse(
            new { user.Id, user.Name, user.Email, user.Role },
            "Registration successful."));
    }

    [HttpPost("api/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var response = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful."));
    }

    [HttpPost("api/auth/logout")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public IActionResult Logout()
    {
        return Ok(ApiResponse.SuccessResponse("Logged out successfully."));
    }

    [HttpDelete("api/auth/account")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> DeleteOwnAccount()
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var role = HttpContext.Items["UserRole"]!.ToString()!;
        await _authService.DeleteAccountAsync(userId, userId, role);
        return Ok(ApiResponse.SuccessResponse("Account deleted successfully."));
    }

    [HttpPost("api/auth/forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var token = await _authService.ForgotPasswordAsync(dto);
        return Ok(ApiResponse<object>.SuccessResponse(
            new { resetToken = token },
            "Security answers verified. Use the reset token to set a new password."));
    }

    [HttpPost("api/auth/reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(ApiResponse.SuccessResponse("Password reset successfully."));
    }

    // ── User Profile Endpoints ───────────────────────────────────────────────

    [HttpGet("api/users/profile")]
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

    [HttpPut("api/users/profile")]
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

    [HttpPut("api/users/password")]
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
