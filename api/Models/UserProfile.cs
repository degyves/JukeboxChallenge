using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartyJukebox.Api.Models;

public class UserProfile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("roomId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string RoomId { get; set; } = default!;

    [BsonElement("role")]
    public UserRole Role { get; set; }

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = default!;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastSeenAt")]
    public DateTime? LastSeenAt { get; set; } = DateTime.UtcNow;
}

public enum UserRole
{
    Host,
    Guest
}
