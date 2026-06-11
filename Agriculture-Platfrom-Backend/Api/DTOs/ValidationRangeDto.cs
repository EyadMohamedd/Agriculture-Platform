namespace AgriculturalMonitorSystem.Api.DTOs;

public class ValidationRangeDto
{
    public string? Id { get; set; }
    public string SensorType { get; set; } = string.Empty;
    public double MinNormal { get; set; }
    public double MaxNormal { get; set; }
    public double WarningLow { get; set; }
    public double WarningHigh { get; set; }
    public double CriticalLow { get; set; }
    public double CriticalHigh { get; set; }
}
