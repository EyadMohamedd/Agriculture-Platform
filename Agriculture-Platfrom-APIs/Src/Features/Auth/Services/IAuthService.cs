using AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<User> GetProfileAsync(string userId);
    Task<User> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<string> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task DeleteAccountAsync(string userId, string requestingUserId, string requestingUserRole);
}
