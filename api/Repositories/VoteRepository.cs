using MongoDB.Driver;
using PartyJukebox.Api.Infrastructure;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Repositories;

public interface IVoteRepository
{
    Task<Vote?> GetVoteAsync(string roomId, string trackId, string userId, CancellationToken cancellationToken);
    Task UpsertVoteAsync(Vote vote, CancellationToken cancellationToken);
    Task RemoveVotesForTrackAsync(string trackId, CancellationToken cancellationToken);
    Task<int> CountVotesAsync(string trackId, int value, CancellationToken cancellationToken);
}

public class VoteRepository : IVoteRepository
{
    private readonly MongoDbContext _context;

    public VoteRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Vote?> GetVoteAsync(string roomId, string trackId, string userId, CancellationToken cancellationToken)
        => await _context.Votes.Find(v => v.RoomId == roomId && v.TrackId == trackId && v.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

    public Task UpsertVoteAsync(Vote vote, CancellationToken cancellationToken)
        => _context.Votes.ReplaceOneAsync(
            v => v.Id == vote.Id,
            vote,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

    public Task RemoveVotesForTrackAsync(string trackId, CancellationToken cancellationToken)
        => _context.Votes.DeleteManyAsync(v => v.TrackId == trackId, cancellationToken);

    public async Task<int> CountVotesAsync(string trackId, int value, CancellationToken cancellationToken)
    {
        var count = await _context.Votes.CountDocumentsAsync(v => v.TrackId == trackId && v.Value == value, cancellationToken: cancellationToken);
        return (int)count;
    }
}
