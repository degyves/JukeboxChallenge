using MongoDB.Driver;
using PartyJukebox.Api.Infrastructure;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Repositories;

public interface IUserRepository
{
    Task CreateAsync(UserProfile user, CancellationToken cancellationToken);
    Task<UserProfile?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<UserProfile?> GetRoomHostAsync(string roomId, CancellationToken cancellationToken);
    Task UpdateLastSeenAsync(string userId, CancellationToken cancellationToken);
}

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(UserProfile user, CancellationToken cancellationToken)
        => _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);

    public async Task<UserProfile?> GetByIdAsync(string id, CancellationToken cancellationToken)
        => await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task<UserProfile?> GetRoomHostAsync(string roomId, CancellationToken cancellationToken)
        => await _context.Users.Find(u => u.RoomId == roomId && u.Role == UserRole.Host).FirstOrDefaultAsync(cancellationToken);

    public Task UpdateLastSeenAsync(string userId, CancellationToken cancellationToken)
        => _context.Users.UpdateOneAsync(
            u => u.Id == userId,
            Builders<UserProfile>.Update.Set(u => u.LastSeenAt, DateTime.UtcNow),
            cancellationToken: cancellationToken);
}
