namespace AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;

public class SensorStatisticsDto
{
    public string FarmId { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? Avg { get; set; }
    public int Count { get; set; }
}
