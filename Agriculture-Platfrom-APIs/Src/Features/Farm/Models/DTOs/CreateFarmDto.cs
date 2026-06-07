using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;

public class CreateFarmDto
{
    public string Name { get; set; } = string.Empty;
    public Location Location { get; set; } = new();
    public string? CropType { get; set; }
}
