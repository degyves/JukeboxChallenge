using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Infrastructure;

public class MongoInitializer : IHostedService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoInitializer> _logger;

    public MongoInitializer(MongoDbContext context, ILogger<MongoInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var roomIndex = new CreateIndexModel<Room>(
            Builders<Room>.IndexKeys.Ascending(r => r.Code),
            new CreateIndexOptions { Unique = true });
        await _context.Rooms.Indexes.CreateOneAsync(roomIndex, cancellationToken: cancellationToken);

        _logger.LogInformation("Mongo indexes ensured");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
