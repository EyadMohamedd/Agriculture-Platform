using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;

public class UpdateFarmDto
{
    public string? Name { get; set; }
    public Location? Location { get; set; }
    public string? CropType { get; set; }
}
