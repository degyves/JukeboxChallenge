using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartyJukebox.Api.Models;

public class Track
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("roomId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string RoomId { get; set; } = default!;

    [BsonElement("source")]
    public string Source { get; set; } = "youtube";

    [BsonElement("videoId")]
    public string VideoId { get; set; } = default!;

    [BsonElement("title")]
    public string Title { get; set; } = default!;

    [BsonElement("channel")]
    public string Channel { get; set; } = default!;

    [BsonElement("durationMs")]
    public int DurationMs { get; set; }

    [BsonElement("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = default!;

    [BsonElement("addedBy")]
    public string AddedBy { get; set; } = default!;

    [BsonElement("votes")]
    public VoteSummary Votes { get; set; } = new();

    [BsonElement("score")]
    public int Score { get; set; }

    [BsonElement("status")]
    public TrackStatus Status { get; set; } = TrackStatus.Queued;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VoteSummary
{
    [BsonElement("up")]
    public int Up { get; set; }

    [BsonElement("down")]
    public int Down { get; set; }
}

public enum TrackStatus
{
    Queued,
    Preparing,
    Ready,
    Error,
    Playing,
    Played
}
