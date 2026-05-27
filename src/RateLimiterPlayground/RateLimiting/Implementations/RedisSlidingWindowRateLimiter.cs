using Microsoft.Extensions.Options;
using RateLimiterPlayground.RateLimiting.Abstractions;
using RateLimiterPlayground.RateLimiting.Configuration;
using RateLimiterPlayground.RateLimiting.Models;
using StackExchange.Redis;

namespace RateLimiterPlayground.RateLimiting.Implementations;

public class RedisSlidingWindowRateLimiter : IRateLimiter
{
    private readonly IDatabase _database;
    private readonly int _maxRequests;
    private readonly int _windowSeconds;

    private const string LuaScript = """
        local key = KEYS[1]
        local now = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])
        local limit = tonumber(ARGV[3])
        local member = ARGV[4]

        local windowStart = now - window

        redis.call('ZREMRANGEBYSCORE', key, 0, windowStart)

        local currentCount = redis.call('ZCARD', key)

        if currentCount >= limit then
            return {0, 0}
        end

        redis.call('ZADD', key, now, member)
        redis.call('EXPIRE', key, math.ceil(window / 1000))

        local remaining = limit - currentCount - 1

        return {1, remaining}
        """;
        
    public RedisSlidingWindowRateLimiter(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RateLimitOptions> options)
    {
        _database = connectionMultiplexer.GetDatabase();
        _maxRequests = options.Value.MaxRequests;
        _windowSeconds = options.Value.WindowSeconds;
    }

    public RateLimitResult IsAllowed(string userId)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowMilliseconds = _windowSeconds * 1000;
        var key = $"rate_limit:{userId}";
        var member = $"{now}:{Guid.NewGuid()}";

        var result = (RedisResult[])_database.ScriptEvaluate(
            LuaScript,
            new RedisKey[] {key},
            new RedisValue[] {now, windowMilliseconds, _maxRequests, member}
        )!;

        return new RateLimitResult
        {
            IsAllowed = (int)result[0] == 1,
            RemainingRequests = (int)result[1],
            LimiterType = "Redis"
        };
    }
}