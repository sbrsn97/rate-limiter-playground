using RateLimiterPlayground.Middleware;
using RateLimiterPlayground.RateLimiting.Abstractions;
using RateLimiterPlayground.RateLimiting.Configuration;
using RateLimiterPlayground.RateLimiting.Implementations;
using RateLimiterPlayground.RateLimiting.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimit")
);
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("Redis")
);

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
   var configuration = builder.Configuration.GetSection("Redis").Get<RedisOptions>()!; 

   return ConnectionMultiplexer.Connect(configuration.ConnectionString);
});

var rateLimitOptions = builder.Configuration
    .GetSection("RateLimit")
    .Get<RateLimitOptions>() ?? new RateLimitOptions();

builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddSingleton<IRateLimiter>(ServiceProvider =>
{
   return rateLimitOptions.Mode switch
   {
       RateLimiterMode.InMemory => 
        ActivatorUtilities.CreateInstance<InMemorySlidingWindowRateLimiter>(ServiceProvider),

       RateLimiterMode.Redis =>
        ActivatorUtilities.CreateInstance<RedisSlidingWindowRateLimiter>(ServiceProvider),

       _ => throw new InvalidOperationException(
            $"Unsupported rate limiter mode: {rateLimitOptions.Mode}"
       )
   };
});

var app = builder.Build();

app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
