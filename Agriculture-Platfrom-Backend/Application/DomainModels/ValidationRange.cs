using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Application.DomainModels;

public class ValidationRange
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

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
}
