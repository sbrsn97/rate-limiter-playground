using RateLimiterPlayground.RateLimiting.Models;

namespace RateLimiterPlayground.RateLimiting.Abstractions;
public interface IRateLimiter
{
    RateLimitResult IsAllowed(string userId);
}