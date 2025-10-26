using System.ComponentModel.DataAnnotations;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Dtos;

public record CreateRoomRequest([Required] string DisplayName);

public record CreateRoomResponse(string Code, string HostSecret, string UserId);

public record JoinRoomRequest([Required] string DisplayName, string? HostSecret);

public record JoinRoomResponse(string UserId, UserRole Role);

public record RoomSummaryDto(string Id, string Code, bool IsActive, string? NowPlayingTrackId, PlaybackStateDto PlaybackState);

public record PlaybackStateDto(string Status, int PositionMs, DateTime UpdatedAt);
