using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Mapping;

public static class DtoMapping
{
    public static RoomSummaryDto ToSummaryDto(this Room room)
        => new(
            room.Id,
            room.Code,
            room.IsActive,
            room.NowPlayingTrackId,
            room.PlaybackState.ToDto());

    public static PlaybackStateDto ToDto(this PlaybackState state)
        => new(state.Status.ToString(), state.PositionMs, state.UpdatedAt);

    public static TrackDto ToDto(this Track track)
        => new(
            track.Id,
            track.RoomId,
            track.Source,
            track.VideoId,
            track.Title,
            track.Channel,
            track.DurationMs,
            track.ThumbnailUrl,
            track.AddedBy,
            new VoteSummaryDto(track.Votes.Up, track.Votes.Down),
            track.Score,
            track.Status.ToString(),
            track.CreatedAt);
}
