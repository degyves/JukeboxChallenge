using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using PartyJukebox.Api.Infrastructure;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Repositories;

public interface ITrackRepository
{
    Task CreateAsync(Track track, CancellationToken cancellationToken);
    Task<List<Track>> GetQueueAsync(string roomId, CancellationToken cancellationToken);
    Task<Track?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Track?> GetTopReadyAsync(string roomId, CancellationToken cancellationToken);
    Task UpdateAsync(Track track, CancellationToken cancellationToken);
    Task DeleteAsync(string trackId, CancellationToken cancellationToken);
    Task<bool> ExistsByVideoAsync(string roomId, string videoId, CancellationToken cancellationToken);
}

public class TrackRepository : ITrackRepository
{
    private readonly MongoDbContext _context;

    public TrackRepository(MongoDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(Track track, CancellationToken cancellationToken)
        => _context.Tracks.InsertOneAsync(track, cancellationToken: cancellationToken);

    public Task<List<Track>> GetQueueAsync(string roomId, CancellationToken cancellationToken)
        => _context.Tracks.Find(t => t.RoomId == roomId && t.Status != TrackStatus.Played)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Track?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await _context.Tracks.Find(t => t.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<Track?> GetTopReadyAsync(string roomId, CancellationToken cancellationToken)
    {
        var candidates = await _context.Tracks
            .Find(t => t.RoomId == roomId && (t.Status == TrackStatus.Ready || t.Status == TrackStatus.Preparing || t.Status == TrackStatus.Queued))
            .ToListAsync(cancellationToken);

        return candidates
            .OrderByDescending(t => GetStatusPriority(t.Status))
            .ThenByDescending(t => t.Score)
            .ThenBy(t => t.CreatedAt)
            .FirstOrDefault();
    }

    public Task UpdateAsync(Track track, CancellationToken cancellationToken)
        => _context.Tracks.ReplaceOneAsync(t => t.Id == track.Id, track, cancellationToken: cancellationToken);

    public Task DeleteAsync(string trackId, CancellationToken cancellationToken)
        => _context.Tracks.DeleteOneAsync(t => t.Id == trackId, cancellationToken);

    public async Task<bool> ExistsByVideoAsync(string roomId, string videoId, CancellationToken cancellationToken)
    {
        var filter = Builders<Track>.Filter.And(
            Builders<Track>.Filter.Eq(t => t.RoomId, roomId),
            Builders<Track>.Filter.Eq(t => t.VideoId, videoId),
            Builders<Track>.Filter.Ne(t => t.Status, TrackStatus.Played));
        return await _context.Tracks.Find(filter).AnyAsync(cancellationToken);
    }

    private static int GetStatusPriority(TrackStatus status) => status switch
    {
        TrackStatus.Ready => 3,
        TrackStatus.Preparing => 2,
        TrackStatus.Queued => 1,
        _ => 0
    };
}
