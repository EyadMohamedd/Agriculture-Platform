using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.DTOs;

public class FarmResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Location Location { get; set; } = new();
    public string? CropType { get; set; }
    public int SensorCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
