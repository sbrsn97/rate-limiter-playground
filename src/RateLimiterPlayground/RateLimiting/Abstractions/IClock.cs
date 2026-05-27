namespace RateLimiterPlayground.RateLimiting.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}