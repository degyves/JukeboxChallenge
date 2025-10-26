using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Mapping;
using PartyJukebox.Api.Models;
using PartyJukebox.Api.Repositories;
using PartyJukebox.Api.SignalR;

namespace PartyJukebox.Api.Services;

public interface IRoomService
{
    Task<(Room room, UserProfile host)> CreateRoomAsync(string displayName, CancellationToken cancellationToken);
    Task<(Room room, UserProfile user)> JoinRoomAsync(string code, string displayName, string? hostSecret, CancellationToken cancellationToken);
    Task<Room?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> EnsureHostAsync(Room room, string hostSecret);
    Task UpdatePlaybackStateAsync(Room room, PlaybackStatus status, int positionMs, CancellationToken cancellationToken);
    Task UpdateNowPlayingAsync(Room room, string? trackId, CancellationToken cancellationToken);
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomCodeGenerator _roomCodeGenerator;
    private readonly ITrackRepository _trackRepository;
    private readonly ILogger<RoomService> _logger;
    private readonly IHubContext<RoomHub, IRoomClient> _hubContext;

    public RoomService(
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IRoomCodeGenerator roomCodeGenerator,
        ILogger<RoomService> logger,
        IHubContext<RoomHub, IRoomClient> hubContext,
        ITrackRepository trackRepository)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _roomCodeGenerator = roomCodeGenerator;
        _logger = logger;
        _hubContext = hubContext;
        _trackRepository = trackRepository;
    }

    public async Task<(Room room, UserProfile host)> CreateRoomAsync(string displayName, CancellationToken cancellationToken)
    {
        string code;
        do
        {
            code = _roomCodeGenerator.GenerateCode();
        } while (await _roomRepository.CodeExistsAsync(code, cancellationToken));

        var room = new Room
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Code = code,
            HostSecret = _roomCodeGenerator.GenerateSecret()
        };
        await _roomRepository.CreateAsync(room, cancellationToken);

        var host = new UserProfile
        {
            Id = ObjectId.GenerateNewId().ToString(),
            RoomId = room.Id,
            Role = UserRole.Host,
            DisplayName = displayName,
            LastSeenAt = DateTime.UtcNow
        };
        await _userRepository.CreateAsync(host, cancellationToken);

        _logger.LogInformation("Created room {Code}", code);

        await _hubContext.Clients.Group(RoomHub.RoomGroup(code))
            .RoomSync(new RoomSummaryDto(room.Id, room.Code, room.IsActive, room.NowPlayingTrackId, room.PlaybackState.ToDto()), Array.Empty<TrackDto>());

        return (room, host);
    }

    public async Task<(Room room, UserProfile user)> JoinRoomAsync(string code, string displayName, string? hostSecret, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByCodeAsync(code, cancellationToken)
                   ?? throw new InvalidOperationException("Room not found");

        var role = !string.IsNullOrWhiteSpace(hostSecret) && hostSecret == room.HostSecret
            ? UserRole.Host
            : UserRole.Guest;

        var user = new UserProfile
        {
            RoomId = room.Id,
            Role = role,
            DisplayName = displayName,
            LastSeenAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} joined room {Code}", user.Id, code);

        await _hubContext.Clients.Group(RoomHub.RoomGroup(code))
            .RoomHeartbeat(user.Id);

        return (room, user);
    }

    public Task<Room?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => _roomRepository.GetByCodeAsync(code, cancellationToken);

    public Task<bool> EnsureHostAsync(Room room, string hostSecret)
        => Task.FromResult(room.HostSecret == hostSecret);

    public async Task UpdatePlaybackStateAsync(Room room, PlaybackStatus status, int positionMs, CancellationToken cancellationToken)
    {
        room.PlaybackState.Status = status;
        room.PlaybackState.PositionMs = positionMs;
        room.PlaybackState.UpdatedAt = DateTime.UtcNow;

        await _roomRepository.UpdatePlaybackAsync(room.Id, room.PlaybackState, cancellationToken);

        await _hubContext.Clients.Group(RoomHub.RoomGroup(room.Code))
            .PlaybackState(new PlaybackStateDto(status.ToString(), positionMs, room.PlaybackState.UpdatedAt));
    }

    public async Task UpdateNowPlayingAsync(Room room, string? trackId, CancellationToken cancellationToken)
    {
        room.NowPlayingTrackId = trackId;
        await _roomRepository.UpdateAsync(room, cancellationToken);

        var queue = await _trackRepository.GetQueueAsync(room.Id, cancellationToken);

        await _hubContext.Clients.Group(RoomHub.RoomGroup(room.Code))
            .RoomSync(room.ToSummaryDto(), queue.Select(t => t.ToDto()).ToArray());
    }
}
