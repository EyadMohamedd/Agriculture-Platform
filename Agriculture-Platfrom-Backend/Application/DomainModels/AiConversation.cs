using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using AgriculturalMonitorSystem.Api.DTOs; // Temporary until Step 6

namespace AgriculturalMonitorSystem.Application.DomainModels;

public class AiConversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("farm_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FarmId { get; set; } = string.Empty;

    [BsonElement("question")]
    public string Question { get; set; } = string.Empty;

    [BsonElement("answer")]
    public string Answer { get; set; } = string.Empty;

    [BsonElement("context_snapshot")]
    public AiSensorDataDto ContextSnapshot { get; set; } = new();

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
