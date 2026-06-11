namespace AgriculturalMonitorSystem.Api.DTOs;

public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public int FarmCount { get; set; }
    public List<FarmSummaryDto> Farms { get; set; } = [];
}

public class FarmSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? CropType { get; set; }
}
