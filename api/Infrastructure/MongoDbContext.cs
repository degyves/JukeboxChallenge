using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PartyJukebox.Api.Configuration;
using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Infrastructure;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.Database);

        Rooms = _database.GetCollection<Room>("rooms");
        Tracks = _database.GetCollection<Track>("tracks");
        Votes = _database.GetCollection<Vote>("votes");
        Users = _database.GetCollection<UserProfile>("users");
    }

    public IMongoCollection<Room> Rooms { get; }

    public IMongoCollection<Track> Tracks { get; }

    public IMongoCollection<Vote> Votes { get; }

    public IMongoCollection<UserProfile> Users { get; }
}
