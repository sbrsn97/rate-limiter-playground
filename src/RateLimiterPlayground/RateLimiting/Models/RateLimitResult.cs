namespace RateLimiterPlayground.RateLimiting.Models;

public class RateLimitResult
{
    public bool IsAllowed {get; set;}
    public int RemainingRequests {get; set;}
    public string LimiterType {get; set;} = string.Empty;
}