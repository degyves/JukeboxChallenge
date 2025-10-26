using System.ComponentModel.DataAnnotations;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Dtos;

public record EnqueueTrackRequest(
    string? YoutubeUrl,
    string? Query,
    // Optional client-provided metadata to avoid server-side YouTube fetches
    string? VideoId,
    string? Title,
    string? Channel,
    int? DurationMs,
    string? ThumbnailUrl,
    [Required] string AddedByUserId);

public record TrackDto(
    string Id,
    string RoomId,
    string Source,
    string VideoId,
    string Title,
    string Channel,
    int DurationMs,
    string ThumbnailUrl,
    string AddedBy,
    VoteSummaryDto Votes,
    int Score,
    string Status,
    DateTime CreatedAt);

public record VoteSummaryDto(int Up, int Down);

public record VoteRequest([Required] int Value, [Required] string UserId);

public record QueueResponse(IEnumerable<TrackDto> Tracks);
