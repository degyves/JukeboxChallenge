using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using PartyJukebox.Api.Configuration;
using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Mapping;
using PartyJukebox.Api.Models;
using PartyJukebox.Api.Repositories;
using PartyJukebox.Api.SignalR;

namespace PartyJukebox.Api.Services;

public interface ITrackService
{
    Task<Track> EnqueueAsync(Room room, EnqueueTrackRequest request, CancellationToken cancellationToken);
    Task<List<Track>> GetOrderedQueueAsync(Room room, CancellationToken cancellationToken);
    Task<Track> RecordVoteAsync(Room room, Track track, int value, string userId, CancellationToken cancellationToken);
    Task RemoveTrackAsync(Room room, Track track, CancellationToken cancellationToken);
    Task<Track?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Track?> PromoteNextTrackAsync(Room room, CancellationToken cancellationToken);
}

public class TrackService : ITrackService
{
    private readonly ITrackRepository _trackRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IQueueOrderingService _orderingService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IStreamClient _streamClient;
    private readonly IOptions<RateLimitOptions> _rateLimitOptions;
    private readonly ILogger<TrackService> _logger;
    private readonly IHubContext<RoomHub, IRoomClient> _hubContext;
    private readonly IRoomRepository _roomRepository;

    public TrackService(
        ITrackRepository trackRepository,
        IVoteRepository voteRepository,
        IQueueOrderingService orderingService,
        IRateLimitService rateLimitService,
        IStreamClient streamClient,
        IOptions<RateLimitOptions> rateLimitOptions,
        ILogger<TrackService> logger,
        IHubContext<RoomHub, IRoomClient> hubContext,
        IRoomRepository roomRepository)
    {
        _trackRepository = trackRepository;
        _voteRepository = voteRepository;
        _orderingService = orderingService;
        _rateLimitService = rateLimitService;
        _streamClient = streamClient;
        _rateLimitOptions = rateLimitOptions;
        _logger = logger;
        _hubContext = hubContext;
        _roomRepository = roomRepository;
    }

    public async Task<Track> EnqueueAsync(Room room, EnqueueTrackRequest request, CancellationToken cancellationToken)
    {
        await EnsureRateLimitAsync($"add:{room.Code}:{request.AddedByUserId}", _rateLimitOptions.Value.TrackAddsPerMinute, cancellationToken);

        StreamMetadata metadata;

        // If the client supplied metadata, prefer that to avoid server-side YouTube calls.
        // Duration is optional (clients may only supply title/channel/videoId/thumbnail).
        if (!string.IsNullOrWhiteSpace(request.VideoId) &&
            !string.IsNullOrWhiteSpace(request.Title) &&
            !string.IsNullOrWhiteSpace(request.Channel))
        {
            var duration = request.DurationMs ?? 0;
            metadata = new StreamMetadata(request.VideoId!, request.Title!, request.Channel!, duration, request.ThumbnailUrl ?? string.Empty);
        }
        else if (!string.IsNullOrWhiteSpace(request.YoutubeUrl))
        {
            // Backwards compatible: resolve via stream service. Log warning so callers without metadata can be identified.
            _logger.LogWarning("Client enqueued track with youtubeUrl but no metadata. Calling stream resolve for input={Url}", request.YoutubeUrl);
            metadata = await _streamClient.ResolveAsync(request.YoutubeUrl!, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.Query))
        {
            metadata = await _streamClient.SearchAsync(request.Query!, cancellationToken)
                        ?? throw new InvalidOperationException("No results for query");
        }
        else
        {
            throw new InvalidOperationException("Either youtubeUrl, query, or client metadata is required.");
        }

        if (await _trackRepository.ExistsByVideoAsync(room.Id, metadata.VideoId, cancellationToken))
        {
            throw new InvalidOperationException("Track already in queue.");
        }

        var track = new Track
        {
            Id = ObjectId.GenerateNewId().ToString(),
            RoomId = room.Id,
            VideoId = metadata.VideoId,
            Title = metadata.Title,
            Channel = metadata.Channel,
            DurationMs = metadata.DurationMs,
            ThumbnailUrl = metadata.ThumbnailUrl,
            AddedBy = request.AddedByUserId,
            Status = TrackStatus.Ready
        };

        await _trackRepository.CreateAsync(track, cancellationToken);

        await _hubContext.Clients.Group(RoomHub.RoomGroup(room.Code))
            .TrackAdded(track.ToDto());

        return track;
    }

    public async Task<List<Track>> GetOrderedQueueAsync(Room room, CancellationToken cancellationToken)
    {
        var tracks = await _trackRepository.GetQueueAsync(room.Id, cancellationToken);
        return _orderingService.OrderQueue(tracks).ToList();
    }

    public async Task<Track> RecordVoteAsync(Room room, Track track, int value, string userId, CancellationToken cancellationToken)
    {
        if (value is not (1 or -1))
        {
            throw new InvalidOperationException("Invalid vote value");
        }

        await EnsureRateLimitAsync($"vote:{room.Code}:{userId}", _rateLimitOptions.Value.VotesPerMinute, cancellationToken);

        var existing = await _voteRepository.GetVoteAsync(room.Id, track.Id, userId, cancellationToken);
        if (existing is null)
        {
            existing = new Vote
            {
                Id = ObjectId.GenerateNewId().ToString(),
                RoomId = room.Id,
                TrackId = track.Id,
                UserId = userId,
                Value = value
            };
        }
        else
        {
            existing.Value = value;
        }

        await _voteRepository.UpsertVoteAsync(existing, cancellationToken);

        track.Votes.Up = await _voteRepository.CountVotesAsync(track.Id, 1, cancellationToken);
        track.Votes.Down = await _voteRepository.CountVotesAsync(track.Id, -1, cancellationToken);
        track.Score = track.Votes.Up - track.Votes.Down;

        await _trackRepository.UpdateAsync(track, cancellationToken);

        await BroadcastQueueUpdate(room, track);

        return track;
    }

    public async Task RemoveTrackAsync(Room room, Track track, CancellationToken cancellationToken)
    {
        await _trackRepository.DeleteAsync(track.Id, cancellationToken);
        await _voteRepository.RemoveVotesForTrackAsync(track.Id, cancellationToken);
        await _hubContext.Clients.Group(RoomHub.RoomGroup(room.Code))
            .TrackRemoved(track.Id);
    }

    public Task<Track?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => _trackRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Track?> PromoteNextTrackAsync(Room room, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(room.NowPlayingTrackId))
        {
            var current = await _trackRepository.GetByIdAsync(room.NowPlayingTrackId, cancellationToken);
            if (current is not null && current.Status != TrackStatus.Played)
            {
                current.Status = TrackStatus.Played;
                await _trackRepository.UpdateAsync(current, cancellationToken);
                await BroadcastQueueUpdate(room, current);
            }
        }

        var next = await _trackRepository.GetTopReadyAsync(room.Id, cancellationToken);
        if (next is null)
        {
            return null;
        }

        next.Status = TrackStatus.Playing;
        await _trackRepository.UpdateAsync(next, cancellationToken);
        await BroadcastQueueUpdate(room, next);
        return next;
    }

    private async Task EnsureRateLimitAsync(string key, int limit, CancellationToken cancellationToken)
    {
        if (!await _rateLimitService.TryConsumeAsync(key, limit, cancellationToken))
        {
            throw new InvalidOperationException("Rate limit exceeded");
        }
    }

    private async Task BroadcastQueueUpdate(Room room, Track track)
    {
        await _hubContext.Clients.Group(RoomHub.RoomGroup(room.Code))
            .QueueUpdated(track.Id, track.Score, track.Status.ToString());
    }
}
