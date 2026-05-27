using Microsoft.Extensions.Options;
using RateLimiterPlayground.RateLimiting;

namespace RateLimiterPlayground.Tests.RateLimiting;

public class InMemorySlidingWindowRateLimiterTests
{
    private static InMemorySlidingWindowRateLimiter CreateLimiter(
        int maxRequests = 3,
        int windowSeconds = 60)
    {
        var options = Options.Create(new RateLimitOptions
        {
            MaxRequests = maxRequests,
            WindowSeconds = windowSeconds,
            Mode = RateLimiterMode.InMemory
        });

        return new InMemorySlidingWindowRateLimiter(options);
    }

    [Fact]
    public void IsAllowed_ShouldAllowRequests_WhenLimitIsNotExceeded()
    {
        var limiter = CreateLimiter(maxRequests: 3);

        var first = limiter.IsAllowed("user-1");
        var second = limiter.IsAllowed("user-1");
        var third = limiter.IsAllowed("user-1");

        Assert.True(first.IsAllowed);
        Assert.True(second.IsAllowed);
        Assert.True(third.IsAllowed);
        Assert.Equal(2, first.RemainingRequests);
        Assert.Equal(1, second.RemainingRequests);
        Assert.Equal(0, third.RemainingRequests);
    }

    [Fact]
    public void IsAllowed_ShouldRejectRequest_WhenLimitIsExceeded()
    {
        var limiter = CreateLimiter(maxRequests: 2);

        Assert.True(limiter.IsAllowed("user-1").IsAllowed);
        Assert.True(limiter.IsAllowed("user-1").IsAllowed);

        var rejected = limiter.IsAllowed("user-1");

        Assert.False(rejected.IsAllowed);
        Assert.Equal(0, rejected.RemainingRequests);
    }

    [Fact]
    public void IsAllowed_ShouldTrackUsersIndependently()
    {
        var limiter = CreateLimiter(maxRequests: 1);

        var userOneFirstRequest = limiter.IsAllowed("user-1");
        var userOneSecondRequest = limiter.IsAllowed("user-1");

        var userTwoFirstRequest = limiter.IsAllowed("user-2");

        Assert.True(userOneFirstRequest.IsAllowed);
        Assert.False(userOneSecondRequest.IsAllowed);
        Assert.True(userTwoFirstRequest.IsAllowed);
    }

    [Fact]
    public async Task IsAllowed_ShouldAllowAgain_AfterWindowExpires()
    {
        var limiter = CreateLimiter(maxRequests: 1, windowSeconds: 1);

        var first = limiter.IsAllowed("user-1");
        var second = limiter.IsAllowed("user-1");

        await Task.Delay(1100);

        var third = limiter.IsAllowed("user-1");

        Assert.True(first.IsAllowed);
        Assert.False(second.IsAllowed);
        Assert.True(third.IsAllowed);
    }
}