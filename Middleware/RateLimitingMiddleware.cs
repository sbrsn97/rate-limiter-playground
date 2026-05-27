using RateLimiterPlayground.RateLimiting;

namespace RateLimiterPlayground.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter)
    {
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();

        if(string.IsNullOrWhiteSpace(userId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing X-User-Id header.");
            return;
        }

        var result = rateLimiter.IsAllowed(userId);

        context.Response.Headers["X-RateLimit-Remaining"] = result.RemainingRequests.ToString();

        if(!result.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate Limit Exceeded.");
            return;
        }

        await _next(context);
    }
}