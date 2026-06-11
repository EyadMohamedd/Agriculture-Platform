using AgriculturalMonitorSystem.Api.DTOs;
using AgriculturalMonitorSystem.Application.DomainModels;

namespace AgriculturalMonitorSystem.Application.Services.Interfaces;

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
