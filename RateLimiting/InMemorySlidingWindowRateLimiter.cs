using Microsoft.Extensions.Options;

namespace RateLimiterPlayground.RateLimiting;

public class InMemorySlidingWindowRateLimiter : IRateLimiter
{
    private readonly Dictionary<string, Queue<DateTime>> _requests = new();
    private readonly object _lock = new();

    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    public InMemorySlidingWindowRateLimiter(IOptions<RateLimitOptions> options)
    {
        _maxRequests = options.Value.MaxRequests;
        _window = TimeSpan.FromSeconds(options.Value.WindowSeconds);
    }
    public RateLimitResult IsAllowed(string userId)
    {
        var now = DateTime.UtcNow;

        lock(_lock)
        {
            if(!_requests.TryGetValue(userId, out var timestamps))
            {
                timestamps = new Queue<DateTime>();
                _requests[userId] = timestamps;
            }

            while(timestamps.Count > 0 && now - timestamps.Peek() >= _window)
            {
                timestamps.Dequeue();
            }

            if(timestamps.Count >= _maxRequests)
            {
                return new RateLimitResult{
                    IsAllowed = false,
                    RemainingRequests = 0,
                    LimiterType = "InMemory"
                };
            }

            timestamps.Enqueue(now);
            
            return new RateLimitResult
            {
                IsAllowed = true,
                RemainingRequests = _maxRequests - timestamps.Count,
                LimiterType = "InMemory"
            };
        }
    }
}