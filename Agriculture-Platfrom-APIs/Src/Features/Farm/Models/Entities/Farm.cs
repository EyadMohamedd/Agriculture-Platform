using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using AgriculturalMonitorSystem.Src.Shared.Models;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Models.Entities;

public class Farm
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("location")]
    [BsonIgnoreIfNull]
    public Location? Location { get; set; }

    /// <summary>Optional crop type growing on this farm (e.g. "Wheat", "Rice", "Maize")</summary>
    [BsonElement("crop_type")]
    [BsonIgnoreIfNull]
    public string? CropType { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
