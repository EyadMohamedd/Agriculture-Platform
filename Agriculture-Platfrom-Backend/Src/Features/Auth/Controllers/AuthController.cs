using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Auth.Services;
using AgriculturalMonitorSystem.Src.Shared.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Constants;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    // POST /api/auth/register  — Public
    [HttpPost("register")]
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

    // POST /api/auth/login  — Public
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage)));

        var response = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful."));
    }

    // POST /api/auth/logout  — Farmer | Admin
    [HttpPost("logout")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public IActionResult Logout()
    {
        // JWT is stateless — no server-side action. Frontend deletes token.
        return Ok(ApiResponse.SuccessResponse("Logged out successfully."));
    }

    // DELETE /api/auth/account  — deletes own account (Farmer/Admin)
    [HttpDelete("account")]
    [AuthorizeRole(RoleConstants.Farmer, RoleConstants.Admin)]
    public async Task<IActionResult> DeleteOwnAccount()
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var role = HttpContext.Items["UserRole"]!.ToString()!;
        await _authService.DeleteAccountAsync(userId, userId, role);
        return Ok(ApiResponse.SuccessResponse("Account deleted successfully."));
    }

    // POST /api/auth/forgot-password  — Public
    // Body: { email, farmRegistrationNumber, username }
    // Returns a reset token if both security answers are correct.
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var token = await _authService.ForgotPasswordAsync(dto);
        return Ok(ApiResponse<object>.SuccessResponse(
            new { resetToken = token },
            "Security answers verified. Use the reset token to set a new password."));
    }

    // POST /api/auth/reset-password  — Public
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(ApiResponse.SuccessResponse("Password reset successfully."));
    }
}
