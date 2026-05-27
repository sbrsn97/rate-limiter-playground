namespace RateLimiterPlayground.RateLimiting;
public interface IRateLimiter
{
    RateLimitResult IsAllowed(string userId);
}