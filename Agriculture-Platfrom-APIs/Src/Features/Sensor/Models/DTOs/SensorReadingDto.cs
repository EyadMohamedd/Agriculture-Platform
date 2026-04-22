namespace AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;

public class SensorReadingDto
{
    public string Id { get; set; } = string.Empty;
    public string SensorId { get; set; } = string.Empty;
    public string FarmId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? Temperature { get; set; }
    public double? SoilPh { get; set; }
    public double? SoilMoisture { get; set; }
    public double? NpkN { get; set; }
    public double? NpkP { get; set; }
    public double? NpkK { get; set; }
    public double? Rainfall { get; set; }
    public bool IsAnomaly { get; set; }
    public string? AnomalyReason { get; set; }
}
