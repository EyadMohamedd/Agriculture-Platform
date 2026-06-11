using AgriculturalMonitorSystem.Shared.Models;

namespace AgriculturalMonitorSystem.Api.DTOs;

public class UpdateFarmDto
{
    public string? Name { get; set; }
    public Location? Location { get; set; }
    public string? CropType { get; set; }
}
