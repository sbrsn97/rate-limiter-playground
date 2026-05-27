namespace RateLimiterPlayground.RateLimiting;

public interface IClock
{
    DateTime UtcNow { get; }
}