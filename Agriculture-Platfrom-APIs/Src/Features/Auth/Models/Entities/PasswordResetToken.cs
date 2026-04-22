using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Models.Entities;

public class PasswordResetToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("expiry")]
    public DateTime Expiry { get; set; }

    [BsonElement("is_used")]
    public bool IsUsed { get; set; } = false;
}
