using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.DTOs;

public class CreateFarmDto
{
    public string Name { get; set; } = string.Empty;
    public Location Location { get; set; } = new();
    public string? CropType { get; set; }
}
