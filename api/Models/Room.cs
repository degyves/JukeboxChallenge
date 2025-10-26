using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartyJukebox.Api.Models;

public class Room
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("code")]
    public string Code { get; set; } = default!;

    [BsonElement("hostSecret")]
    public string HostSecret { get; set; } = default!;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("nowPlayingTrackId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? NowPlayingTrackId { get; set; }

    [BsonElement("playbackState")]
    public PlaybackState PlaybackState { get; set; } = new();
}

public class PlaybackState
{
    [BsonElement("status")]
    public PlaybackStatus Status { get; set; } = PlaybackStatus.Idle;

    [BsonElement("positionMs")]
    public int PositionMs { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum PlaybackStatus
{
    Idle,
    Buffering,
    Playing,
    Paused,
    Ended
}
