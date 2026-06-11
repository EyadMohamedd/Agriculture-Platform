using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Application.DomainModels;

/// <summary>
/// Farm-specific threshold override. When present for a farm+sensorType pair,
/// AlertService uses this instead of the global ValidationRange default.
/// </summary>
public class FarmValidationRange
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("farm_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FarmId { get; set; } = string.Empty;

    /// <summary>temperature | ph | moisture | npk_n | npk_p | npk_k | rainfall</summary>
    [BsonElement("sensor_type")]
    public string SensorType { get; set; } = string.Empty;

    [BsonElement("min_normal")]
    public double MinNormal { get; set; }

    [BsonElement("max_normal")]
    public double MaxNormal { get; set; }

    [BsonElement("warning_low")]
    public double WarningLow { get; set; }

    [BsonElement("warning_high")]
    public double WarningHigh { get; set; }

    [BsonElement("critical_low")]
    public double CriticalLow { get; set; }

    [BsonElement("critical_high")]
    public double CriticalHigh { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
