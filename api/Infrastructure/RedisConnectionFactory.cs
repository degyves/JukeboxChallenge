using Microsoft.Extensions.Options;
using PartyJukebox.Api.Configuration;
using StackExchange.Redis;

namespace PartyJukebox.Api.Infrastructure;

public interface IRedisConnectionFactory
{
    Task<IDatabase> GetDatabaseAsync();
    Task<ISubscriber> GetSubscriberAsync();
}

public class RedisConnectionFactory : IRedisConnectionFactory, IAsyncDisposable
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _multiplexer;

    public RedisConnectionFactory(IOptions<RedisOptions> options)
    {
        _multiplexer = new Lazy<Task<ConnectionMultiplexer>>(() => ConnectionMultiplexer.ConnectAsync(options.Value.ConnectionString));
    }

    public async Task<IDatabase> GetDatabaseAsync()
    {
        var mux = await _multiplexer.Value;
        return mux.GetDatabase();
    }

    public async Task<ISubscriber> GetSubscriberAsync()
    {
        var mux = await _multiplexer.Value;
        return mux.GetSubscriber();
    }

    public async ValueTask DisposeAsync()
    {
        if (_multiplexer.IsValueCreated)
        {
            await (await _multiplexer.Value).CloseAsync();
        }
    }
}
