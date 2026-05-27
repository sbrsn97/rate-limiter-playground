namespace RateLimiterPlayground.RateLimiting;

public class RateLimitOptions
{
    public int MaxRequests {get; set;} = 10000;
    public int WindowSeconds {get; set;} = 60;
}