using Microsoft.Extensions.Options;
using RateLimiterPlayground.RateLimiting;
using StackExchange.Redis;

namespace RateLimiterPlayground.Tests.RateLimiting;

public class RedisSlidingWindowRateLimiterTests : IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisSlidingWindowRateLimiterTests()
    {
        _redis = ConnectionMultiplexer.Connect("localhost:6379");
        _database = _redis.GetDatabase();
    }

    [Fact]
    public void IsAllowed_ShouldRejectRequest_WhenLimitIsExceeded()
    {
        var userId = $"test-user:{Guid.NewGuid()}";
        var limiter = CreateLimiter(maxRequests: 2, windowSeconds: 60);

        Assert.True(limiter.IsAllowed(userId).IsAllowed);
        Assert.True(limiter.IsAllowed(userId).IsAllowed);

        var rejected = limiter.IsAllowed(userId);

        Assert.False(rejected.IsAllowed);
        Assert.Equal(0, rejected.RemainingRequests);
        Assert.Equal("Redis", rejected.LimiterType);
    }

    [Fact]
    public void IsAllowed_ShouldTrackUsersIndependently()
    {
        var limiter = CreateLimiter(maxRequests: 1, windowSeconds: 60);

        var userOne = $"test-user:{Guid.NewGuid()}";
        var userTwo = $"test-user:{Guid.NewGuid()}";

        Assert.True(limiter.IsAllowed(userOne).IsAllowed);
        Assert.False(limiter.IsAllowed(userOne).IsAllowed);

        Assert.True(limiter.IsAllowed(userTwo).IsAllowed);
    }

    private RedisSlidingWindowRateLimiter CreateLimiter(
        int maxRequests,
        int windowSeconds)
    {
        var options = Options.Create(new RateLimitOptions
        {
            MaxRequests = maxRequests,
            WindowSeconds = windowSeconds,
            Mode = RateLimiterMode.Redis
        });

        return new RedisSlidingWindowRateLimiter(_redis, options);
    }

    public void Dispose()
    {
        _redis.Dispose();
    }
}