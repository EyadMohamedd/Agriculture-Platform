using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Api.DTOs;

public class AiRequestDto
{
    [JsonPropertyName("Question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("sensor_data")]
    public AiSensorDataDto SensorData { get; set; } = new();
}

public class AiSensorDataDto
{
    [BsonElement("temperature")]   [BsonIgnoreIfNull] [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [BsonElement("soil_ph")]       [BsonIgnoreIfNull] [JsonPropertyName("soil_ph")]
    public double? SoilPh { get; set; }

    [BsonElement("soil_moisture")] [BsonIgnoreIfNull] [JsonPropertyName("soil_moisture")]
    public double? SoilMoisture { get; set; }

    [BsonElement("nitrogen")]      [BsonIgnoreIfNull] [JsonPropertyName("nitrogen")]
    public double? Nitrogen { get; set; }

    [BsonElement("phosphorus")]    [BsonIgnoreIfNull] [JsonPropertyName("phosphorus")]
    public double? Phosphorus { get; set; }

    [BsonElement("potassium")]     [BsonIgnoreIfNull] [JsonPropertyName("potassium")]
    public double? Potassium { get; set; }

    [BsonElement("rainfall_mm")]   [BsonIgnoreIfNull] [JsonPropertyName("rainfall_mm")]
    public double? RainfallMm { get; set; }

    [BsonElement("farm_location")] [JsonPropertyName("farm_location")]
    public string FarmLocation { get; set; } = string.Empty;

    [BsonElement("crop_type")]     [JsonPropertyName("crop_type")]
    public string CropType { get; set; } = string.Empty;

    [BsonElement("alerts")]        [JsonPropertyName("alerts")]
    public List<AlertContextDto> Alerts { get; set; } = [];
}

public class AlertContextDto
{
    [BsonElement("severity")] [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [BsonElement("sensor")]   [JsonPropertyName("sensor")]
    public string Sensor { get; set; } = string.Empty;

    [BsonElement("message")]  [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
