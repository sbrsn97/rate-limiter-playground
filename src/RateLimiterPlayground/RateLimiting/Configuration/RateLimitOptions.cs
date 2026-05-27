using System.Threading.RateLimiting;

namespace RateLimiterPlayground.RateLimiting.Configuration;

public class RateLimitOptions
{
    public int MaxRequests {get; set;} = 10000;
    public int WindowSeconds {get; set;} = 60;
    public RateLimiterMode Mode {get; set;} = RateLimiterMode.InMemory;
}