using RateLimiterPlayground.RateLimiting;

namespace RateLimiterPlayground.Tests.RateLimiting;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; private set; } = DateTime.UtcNow;

    public void Advance(TimeSpan time)
    {
        UtcNow = UtcNow.Add(time);
    }
}