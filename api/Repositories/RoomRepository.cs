using MongoDB.Driver;
using PartyJukebox.Api.Infrastructure;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Repositories;

public interface IRoomRepository
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task CreateAsync(Room room, CancellationToken cancellationToken);
    Task<Room?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<Room?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(Room room, CancellationToken cancellationToken);
    Task UpdatePlaybackAsync(string roomId, PlaybackState state, CancellationToken cancellationToken);
}

public class RoomRepository : IRoomRepository
{
    private readonly MongoDbContext _context;

    public RoomRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        var count = await _context.Rooms.CountDocumentsAsync(r => r.Code == code, cancellationToken: cancellationToken);
        return count > 0;
    }

    public Task CreateAsync(Room room, CancellationToken cancellationToken)
        => _context.Rooms.InsertOneAsync(room, cancellationToken: cancellationToken);

    public async Task<Room?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => await _context.Rooms.Find(r => r.Code == code).FirstOrDefaultAsync(cancellationToken);

    public async Task<Room?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await _context.Rooms.Find(r => r.Id == id).FirstOrDefaultAsync(cancellationToken);

    public Task UpdateAsync(Room room, CancellationToken cancellationToken)
        => _context.Rooms.ReplaceOneAsync(r => r.Id == room.Id, room, cancellationToken: cancellationToken);

    public Task UpdatePlaybackAsync(string roomId, PlaybackState state, CancellationToken cancellationToken)
        => _context.Rooms.UpdateOneAsync(
            r => r.Id == roomId,
            Builders<Room>.Update
                .Set(r => r.PlaybackState.Status, state.Status)
                .Set(r => r.PlaybackState.PositionMs, state.PositionMs)
                .Set(r => r.PlaybackState.UpdatedAt, state.UpdatedAt),
            cancellationToken: cancellationToken);
}
