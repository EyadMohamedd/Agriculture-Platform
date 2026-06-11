namespace AgriculturalMonitorSystem.Api.DTOs;

public class FarmValidationRangeDto
{
    public string? Id { get; set; }
    public string? FarmId { get; set; }

    /// <summary>temperature | ph | moisture | npk_n | npk_p | npk_k | rainfall</summary>
    public string SensorType { get; set; } = string.Empty;

    public double MinNormal { get; set; }
    public double MaxNormal { get; set; }
    public double WarningLow { get; set; }
    public double WarningHigh { get; set; }
    public double CriticalLow { get; set; }
    public double CriticalHigh { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
