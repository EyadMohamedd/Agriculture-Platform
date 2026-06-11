using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Application.DomainModels;

public class Alert
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("farm_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FarmId { get; set; } = string.Empty;

    [BsonElement("sensor_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SensorId { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>e.g. temperature_critical_high, moisture_warning_low</summary>
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Low | Medium | High | Critical</summary>
    [BsonElement("severity")]
    public string Severity { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("is_resolved")]
    public bool IsResolved { get; set; } = false;

    [BsonElement("resolved_at")]
    [BsonIgnoreIfNull]
    public DateTime? ResolvedAt { get; set; }
}
