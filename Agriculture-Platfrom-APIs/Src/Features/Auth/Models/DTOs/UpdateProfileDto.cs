using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;

public class UpdateProfileDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public Location? FarmLocation { get; set; }
}
