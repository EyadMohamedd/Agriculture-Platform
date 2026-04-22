namespace AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
