namespace AgriculturalMonitorSystem.Src.Features.Sensor.Models.DTOs;

public class LatestReadingDto
{
    public string FarmId { get; set; } = string.Empty;
    public Dictionary<string, LatestSensorValue> Readings { get; set; } = new();
}

public class LatestSensorValue
{
    public string SensorId { get; set; } = string.Empty;
    public string SensorName { get; set; } = string.Empty;
    public Dictionary<string, double?> Value { get; set; } = new();
    public DateTime? Timestamp { get; set; }
    public bool IsAnomaly { get; set; }
}
