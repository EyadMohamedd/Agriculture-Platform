namespace AgriculturalMonitorSystem.Api.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Answer to: "What is your farm's registration number?"</summary>
    public string FarmRegistrationNumber { get; set; } = string.Empty;

    /// <summary>Answer to: "What is your username?"</summary>
    public string Username { get; set; } = string.Empty;
}
