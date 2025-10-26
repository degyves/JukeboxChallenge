using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartyJukebox.Api.Models;

public class Vote
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("roomId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string RoomId { get; set; } = default!;

    [BsonElement("trackId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string TrackId { get; set; } = default!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = default!;

    [BsonElement("value")]
    public int Value { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
