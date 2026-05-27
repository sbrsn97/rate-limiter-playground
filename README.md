# Rate Limiter Playground

A production-oriented API rate limiter project built with ASP.NET Core and Redis.

This project demonstrates both an in-memory and a distributed sliding window rate limiter implementation, along with middleware integration, Docker containerization, Redis atomic operations using Lua scripts, unit tests, and integration tests.

The goal of this project is to explore how real-world large-scale backend systems implement API rate limiting safely under concurrency and distributed environments.

---

# Features

- ASP.NET Core middleware-based rate limiting
- Exact sliding window log algorithm
- In-memory implementation
- Redis distributed implementation
- Redis Lua script atomicity
- Configurable rate limiter mode
- Dockerized API and Redis setup
- Unit tests
- Redis integration tests
- Concurrency tests
- Dependency injection and configuration-driven architecture

---

# Project Structure

```text
RateLimiterPlayground
├── Controllers
├── Middleware
├── RateLimiting
│   ├── IClock.cs
│   ├── SystemClock.cs
│   ├── IRateLimiter.cs
│   ├── RateLimitOptions.cs
│   ├── RedisOptions.cs
│   ├── RateLimitResult.cs
│   ├── RateLimiterMode.cs
│   ├── InMemorySlidingWindowRateLimiter.cs
│   └── RedisSlidingWindowRateLimiter.cs
├── docker-compose.yaml
├── Dockerfile
└── README.md
```

---

# Sliding Window Algorithm

This project uses the exact sliding window log algorithm.

For every request:

1. Remove expired requests older than the configured window
2. Count remaining requests
3. Reject request if limit exceeded
4. Otherwise record the new request

Example:

```text
Limit: 5 requests per 60 seconds

Request timestamps:
12:00:01
12:00:10
12:00:20
12:00:30
12:00:40
```

At `12:00:41`, the next request is rejected because there are already 5 requests inside the active 60-second window.

---

# In-Memory Implementation

The in-memory implementation stores request timestamps inside a dictionary:

```text
Dictionary<string, Queue<DateTime>>
```

This implementation is:

- Simple
- Accurate
- Fast for single-instance applications

However, it is not suitable for distributed systems because each API instance maintains its own local state.

---

# Distributed Redis Implementation

The Redis implementation stores request timestamps inside Redis sorted sets.

Redis key format:

```text
rate_limit:{userId}
```

Redis operations used:

```text
ZADD
ZCARD
ZREMRANGEBYSCORE
EXPIRE
```

Each request timestamp is stored as a sorted set score.

This allows efficient removal of expired requests and counting active requests inside the sliding window.

---

# Why Lua Scripts Are Required

Without Lua scripts, the Redis implementation would suffer from race conditions.

A naive implementation would execute:

```text
1. Remove expired requests
2. Count requests
3. Check limit
4. Add request
```

as separate Redis commands.

Under concurrency, multiple requests could pass the limit check simultaneously and exceed the configured rate limit.

Redis Lua scripts solve this problem by executing all operations atomically.

The entire rate limiting operation becomes a single indivisible Redis operation.

This is a common real-world approach used in distributed backend systems.

---

# Running the Project

## Prerequisites

- Docker
- .NET SDK
- Docker Compose

---

# Run with Docker Compose

```bash
docker compose up --build
```

The API will run on:

```text
http://localhost:8080
```

Redis will run on:

```text
localhost:6379
```

---

# Example Request

```bash
curl -i -H "X-User-Id: user-123" http://localhost:8080/api/test
```

Example response headers:

```text
X-RateLimit-Limiter: Redis
X-RateLimit-Remaining: 4
```

---

# Configuration

Example configuration:

```json
{
  "RateLimit": {
    "MaxRequests": 5,
    "WindowSeconds": 60,
    "Mode": "Redis"
  },
  "Redis": {
    "ConnectionString": "redis:6379"
  }
}
```

Available modes:

```text
InMemory
Redis
```

---

# Testing

Run all tests:

```bash
dotnet test
```

The project contains:

- Unit tests for the in-memory implementation
- Integration tests for the Redis implementation
- Concurrency tests validating atomicity under parallel requests

---

# Concurrency Test

One of the most important tests in this project validates that concurrent requests do not exceed the configured limit.

This test demonstrates why Redis Lua scripts are required for distributed rate limiting systems.

Without atomic operations, concurrent requests could bypass the configured rate limit.

---

# Tradeoffs

## Sliding Window Log

Advantages:

- Exact request counting
- Accurate rate limiting
- Easy to reason about

Disadvantages:

- Memory-heavy
- Stores every request timestamp
- Higher Redis memory usage under very large traffic

---

# Potential Improvements

Possible future improvements include:

- Sliding window counter implementation
- Token bucket algorithm
- Leaky bucket algorithm
- Redis script caching
- Benchmarking
- k6 load testing
- Prometheus metrics
- OpenTelemetry tracing
- Distributed cache abstraction
- Per-endpoint rate limiting
- User tier-based rate limiting
- Retry-after headers
- GitHub Actions CI pipeline

---

# Technologies Used

- ASP.NET Core
- Redis
- StackExchange.Redis
- Docker
- Docker Compose
- xUnit

---

# Purpose of This Project

This project was built to deepen understanding of:

- Distributed systems
- Concurrency
- API middleware
- Redis internals
- Atomic operations
- Rate limiting algorithms
- Backend architecture
- Production-oriented backend engineering practices