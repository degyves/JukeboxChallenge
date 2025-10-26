using Microsoft.Extensions.Options;
using PartyJukebox.Api.Configuration;
using PartyJukebox.Api.Infrastructure;
using StackExchange.Redis;

namespace PartyJukebox.Api.Services;

public interface IRateLimitService
{
    Task<bool> TryConsumeAsync(string key, int limitPerMinute, CancellationToken cancellationToken = default);
}

public class RateLimitService : IRateLimitService
{
    private readonly IRedisConnectionFactory _redisFactory;

    public RateLimitService(IRedisConnectionFactory redisFactory)
    {
        _redisFactory = redisFactory;
    }

    public async Task<bool> TryConsumeAsync(string key, int limitPerMinute, CancellationToken cancellationToken = default)
    {
        var db = await _redisFactory.GetDatabaseAsync();
        var redisKey = new RedisKey($"rl:{DateTime.UtcNow:yyyyMMddHHmm}:{key}");
        var newValue = await db.StringIncrementAsync(redisKey);

        if (newValue == 1)
        {
            await db.KeyExpireAsync(redisKey, TimeSpan.FromMinutes(1));
        }

        return newValue <= limitPerMinute;
    }
}
