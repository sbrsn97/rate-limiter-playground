using RateLimiterPlayground.RateLimiting.Abstractions;

namespace RateLimiterPlayground.RateLimiting.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}