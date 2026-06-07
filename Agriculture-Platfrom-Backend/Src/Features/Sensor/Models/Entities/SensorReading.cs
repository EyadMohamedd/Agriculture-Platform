using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Src.Features.Sensor.Models.Entities;

/// <summary>
/// Stores one reading from a single sensor. Only the field matching the sensor_type is populated.
/// NPK sensor populates all three npk_n, npk_p, npk_k.
/// </summary>
public class SensorReading
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("sensor_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SensorId { get; set; } = string.Empty;

    [BsonElement("farm_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FarmId { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("temperature")]
    [BsonIgnoreIfNull]
    public double? Temperature { get; set; }

    [BsonElement("soil_ph")]
    [BsonIgnoreIfNull]
    public double? SoilPh { get; set; }

    [BsonElement("soil_moisture")]
    [BsonIgnoreIfNull]
    public double? SoilMoisture { get; set; }

    [BsonElement("npk_n")]
    [BsonIgnoreIfNull]
    public double? NpkN { get; set; }

    [BsonElement("npk_p")]
    [BsonIgnoreIfNull]
    public double? NpkP { get; set; }

    [BsonElement("npk_k")]
    [BsonIgnoreIfNull]
    public double? NpkK { get; set; }

    [BsonElement("rainfall")]
    [BsonIgnoreIfNull]
    public double? Rainfall { get; set; }

    [BsonElement("is_anomaly")]
    public bool IsAnomaly { get; set; } = false;

    [BsonElement("anomaly_reason")]
    [BsonIgnoreIfNull]
    public string? AnomalyReason { get; set; }
}
