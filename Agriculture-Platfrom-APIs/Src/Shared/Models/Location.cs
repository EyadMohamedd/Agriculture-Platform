using MongoDB.Bson.Serialization.Attributes;

namespace AgriculturalMonitorSystem.Src.Shared.Models;

public class Location
{
    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }

    [BsonElement("formatted_address")]
    public string FormattedAddress { get; set; } = string.Empty;
}
