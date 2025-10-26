using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Mapping;
using PartyJukebox.Api.Services;

namespace PartyJukebox.Api.SignalR;

public interface IRoomClient
{
    Task RoomSync(RoomSummaryDto room, IEnumerable<TrackDto> queue);
    Task QueueUpdated(string trackId, int score, string status);
    Task TrackAdded(TrackDto track);
    Task TrackRemoved(string trackId);
    Task PlaybackState(PlaybackStateDto state);
    Task RoomHeartbeat(string userId);
}

public class RoomHub : Hub<IRoomClient>
{
    public const string HubPath = "/hub/rooms";

    private readonly IRoomService _roomService;
    private readonly ITrackService _trackService;
    private readonly ILogger<RoomHub> _logger;

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> ConnectionRoom = new();

    public RoomHub(IRoomService roomService, ITrackService trackService, ILogger<RoomHub> logger)
    {
        _roomService = roomService;
        _trackService = trackService;
        _logger = logger;
    }

    public static string RoomGroup(string code) => $"room:{code}";

    public async Task JoinRoom(string code)
    {
        var room = await _roomService.GetByCodeAsync(code, Context.ConnectionAborted);
        if (room is null)
        {
            throw new HubException("Room not found");
        }

        ConnectionRoom[Context.ConnectionId] = room.Code;
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(room.Code));

        var queue = await _trackService.GetOrderedQueueAsync(room, Context.ConnectionAborted);
        await Clients.Caller.RoomSync(room.ToSummaryDto(), queue.Select(q => q.ToDto()));
    }

    public async Task Heartbeat(string userId)
    {
        var code = ResolveConnectionRoom();
        await Clients.Group(RoomGroup(code)).RoomHeartbeat(userId);
    }

    public async Task HostPlay(string trackId, string hostSecret)
    {
        var code = ResolveConnectionRoom();
        var room = await _roomService.GetByCodeAsync(code, Context.ConnectionAborted)
                   ?? throw new HubException("Room not found");

        if (!await _roomService.EnsureHostAsync(room, hostSecret))
        {
            throw new HubException("Forbidden");
        }

        var track = await _trackService.GetByIdAsync(trackId, Context.ConnectionAborted)
                    ?? throw new HubException("Track not found");

        await _trackService.PromoteNextTrackAsync(room, Context.ConnectionAborted);
        await _roomService.UpdateNowPlayingAsync(room, track.Id, Context.ConnectionAborted);
        await _roomService.UpdatePlaybackStateAsync(room, Models.PlaybackStatus.Playing, 0, Context.ConnectionAborted);
    }

    public async Task HostPause(string hostSecret, int positionMs)
    {
        var code = ResolveConnectionRoom();
        var room = await _roomService.GetByCodeAsync(code, Context.ConnectionAborted)
                   ?? throw new HubException("Room not found");

        if (!await _roomService.EnsureHostAsync(room, hostSecret))
        {
            throw new HubException("Forbidden");
        }

        await _roomService.UpdatePlaybackStateAsync(room, Models.PlaybackStatus.Paused, positionMs, Context.ConnectionAborted);
    }

    public async Task HostSeek(string hostSecret, int positionMs)
    {
        var code = ResolveConnectionRoom();
        var room = await _roomService.GetByCodeAsync(code, Context.ConnectionAborted)
                   ?? throw new HubException("Room not found");

        if (!await _roomService.EnsureHostAsync(room, hostSecret))
        {
            throw new HubException("Forbidden");
        }

        await _roomService.UpdatePlaybackStateAsync(room, Models.PlaybackStatus.Playing, positionMs, Context.ConnectionAborted);
    }

    public async Task HostSkip(string hostSecret)
    {
        var code = ResolveConnectionRoom();
        var room = await _roomService.GetByCodeAsync(code, Context.ConnectionAborted)
                   ?? throw new HubException("Room not found");

        if (!await _roomService.EnsureHostAsync(room, hostSecret))
        {
            throw new HubException("Forbidden");
        }

        var next = await _trackService.PromoteNextTrackAsync(room, Context.ConnectionAborted);
        if (next is null)
        {
            await _roomService.UpdateNowPlayingAsync(room, null, Context.ConnectionAborted);
            await _roomService.UpdatePlaybackStateAsync(room, Models.PlaybackStatus.Idle, 0, Context.ConnectionAborted);
            return;
        }

        await _roomService.UpdateNowPlayingAsync(room, next.Id, Context.ConnectionAborted);
        await _roomService.UpdatePlaybackStateAsync(room, Models.PlaybackStatus.Buffering, 0, Context.ConnectionAborted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConnectionRoom.TryRemove(Context.ConnectionId, out _);
        await base.OnDisconnectedAsync(exception);
    }

    private string ResolveConnectionRoom()
    {
        if (!ConnectionRoom.TryGetValue(Context.ConnectionId, out var code))
        {
            _logger.LogWarning("Connection {ConnectionId} has no room association", Context.ConnectionId);
            throw new HubException("Not joined");
        }

        return code;
    }
}
