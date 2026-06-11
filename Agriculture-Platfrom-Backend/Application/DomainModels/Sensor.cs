using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Application.DomainModels;

public class Sensor
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("farm_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FarmId { get; set; } = string.Empty;

    [BsonElement("sensor_name")]
    public string SensorName { get; set; } = string.Empty;

    /// <summary>temperature | ph | moisture | npk | rainfall</summary>
    [BsonElement("sensor_type")]
    public string SensorType { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "active";

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
