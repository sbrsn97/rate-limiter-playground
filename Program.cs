using RateLimiterPlayground.Middleware;
using RateLimiterPlayground.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimit")
);
builder.Services.AddSingleton<IRateLimiter, SlidingWindowRateLimiter>();

var app = builder.Build();

app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
