namespace AgriculturalMonitorSystem.Src.Features.Alert.Models.DTOs;

public class AlertResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FarmId { get; set; } = string.Empty;
    public string SensorId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
