[中文](README.md) | English

# LuBan.Service

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> The unified foundation for business services and background tasks — let every service stand on the shoulders of giants.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Speech](../LuBan.Speech/README.md) | [LuBan.Wechat](../LuBan.Wechat/README.md)
---

## Why Do You Need It?

- Every service repeating the same try/catch and unified response wrapping?
- Background task scheduling logic scattered everywhere with no unified management?
- Scheduling logic for cron jobs, interval tasks, and time-point tasks re-implemented every time?
- Task discovery and loading relies on manual registration — new tasks easily overlooked?

LuBan.Service provides standardized business service base classes and a background task framework, with unified response wrapping, exception handling, cache access, and a built-in configurable task scheduling engine with auto-discovery mechanism.

## Quick Preview

```csharp
// Business service — inherit and use
public class OrderService : BaseService<Order>
{
    public async Task<Result<OrderDto>> GetOrderAsync(long id)
    {
        return await GetResultAsync(async () =>
        {
            var order = await Repository.GetByIdAsync(id);
            return order.ConvertTo<OrderDto>();
        });
    }
}

// Background task — declare and use
[JobInfo(Name = "Data Cleanup Task", Description = "Clean up expired data daily at midnight")]
public class DataCleanupJob : BaseJobService
{
    protected override TimeSpan Interval => TimeSpan.FromHours(24);

    public override async Task RunAsync()
    {
        await CleanupExpiredRecordsAsync();
    }
}
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.Common | Base interfaces and common models |
| LuBan.DI | Dependency injection extensions |
| LuBan.Orm | Data access layer (repositories, entities) |

## Installation

```bash
dotnet add package LuBan.Service
```

## Feature Overview

### Business Service Base Classes

| Feature | Description |
|---------|-------------|
| Unified Response | `SuccessResult()` / `ErrorResult()` standardized response format |
| Exception Wrapping | `GetResult()` / `GetResultAsync()` built-in try/catch, exceptions auto-wrapped as error results |
| Cache Access | `IServiceCache` injected via DI, ready to use out of the box |
| Repository Integration | `BaseService<T>` auto-links to `BaseRepository<T>` |

### Background Task Framework

| Feature | Description |
|---------|-------------|
| Task Interface | `IJob` defines standard task contract (IsRunning, Run, RunAsync, Start, Stop) |
| Scheduling Engine | `BaseBackgroundService` core scheduling logic, supports interval, time-point and Cron expression scheduling |
| Task Base Class | `BaseJobService` abstract base class — configure scheduling parameters and it runs |
| Auto Discovery | `JobServiceLoader` auto-scans all `IJob` implementations, no manual registration needed |
| Task Annotation | `JobInfoAttribute` declares task metadata (name, description, etc.) |

## Usage Guide

### 1. Business Services

```csharp
// Non-generic base class — suitable for services not bound to a specific entity
public class ReportService : BaseService
{
    public async Task<Result<ReportDto>> GenerateReportAsync(string type)
    {
        return await GetResultAsync(async () =>
        {
            var data = await CollectDataAsync(type);
            return new ReportDto { Type = type, Data = data };
        });
    }

    public Result<string> GetStatus()
    {
        return SuccessResult("System running normally");
    }
}

// Generic base class — auto-links repository
public class UserService : BaseService<DbUser>
{
    public async Task<Result<List<UserDto>>> GetAllAsync()
    {
        return await GetResultAsync(async () =>
        {
            var users = await Repository.AsQueryable()
                .ToList();
            return users.ConvertTo<List<UserDto>>();
        });
    }
}
```

### 2. Background Tasks

```csharp
// Define interval task
[JobInfo(Name = "Cache Refresh", Description = "Refresh system cache every 30 minutes")]
public class CacheRefreshJob : BaseJobService
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(30);

    public override async Task RunAsync()
    {
        await RefreshSystemCacheAsync();
    }
}

// Task lifecycle
public interface IJob
{
    bool IsRunning { get; }
    void Run();
    Task RunAsync();
    void Start();
    void Stop();
}
```

### 3. Cron Expression Scheduling

Three constructor styles for scheduling, all unified on the Cronos engine:

```csharp
// Style 1: Interval scheduling (auto-mapped to cron, only evenly-divisible seconds/minutes/hours and daily)
public class IntervalJob : BaseJobService
{
    public IntervalJob() : base(5 * 60 * 1000) { }  // Every 5 minutes => "0 */5 * * * *"

    public override async Task RunAsync() { /* ... */ }
}

// Style 2: Time-point scheduling (HH:mm:ss, mapped to cron)
public class TimePointJob : BaseJobService
{
    public TimePointJob() : base(2, 30, 0) { }  // 02:30:00 daily => "0 30 2 * * *"

    public override async Task RunAsync() { /* ... */ }
}

// Style 3: Direct 6-segment seconds-level cron expression
public class CronJob : BaseJobService
{
    public CronJob() : base("0 0 8 * * 1") { }  // Every Monday 08:00:00

    public override async Task RunAsync() { /* ... */ }
}
```

Cron format is **6-segment seconds-level**: `second minute hour day month dow`, e.g.:

| Expression | Meaning |
|------------|---------|
| `*/10 * * * * *` | Every 10 seconds |
| `0 */5 * * * *` | Every 5 minutes |
| `0 30 2 * * *` | Daily at 02:30:00 |
| `0 0 0 * * *` | Every day at 00:00:00 |
| `0 0 8 * * 1` | Every Monday at 08:00:00 |

Dynamic operations:

```csharp
// Read current cron expression
var cron = job.Cron;

// Query next execution time (local timezone)
var next = job.GetNextOccurrence();

// Dynamic update (takes effect immediately if running, deferred to Start if not)
job.SetCron("0 15 3 * * *");
job.Cron = "0 0 12 * * *";  // Property assignment equivalent to SetCron
```

HTTP API for dynamic management (built-in `JobsController`):

- `GET api/admin/Jobs/GetJobCron?name=xxx` — Query cron expression
- `GET api/admin/Jobs/GetJobNextOccurrence?name=xxx` — Query next execution time
- `PUT api/admin/Jobs/UpdateJobCron?name=xxx` — Update cron for running job (body: `{ "cron": "0 0 8 * * *" }`)

### 4. Task Auto-Discovery

```csharp
// JobServiceLoader auto-scans all classes implementing IJob
// Combined with JobInfoAttribute to retrieve task metadata
// No manual registration needed — new task classes are automatically discovered and scheduled
```

## Tips

- Always use `GetResultAsync()` to wrap business logic for automatic exception handling and unified response format
- The `Repository` property in `BaseService<T>` is directly available — no additional injection needed
- Background tasks only need to inherit `BaseJobService` and set `Interval`; the scheduling engine handles the rest
- Use `JobInfoAttribute` to add names and descriptions to tasks for easier management and monitoring
- `IServiceCache` is injected via DI and can be used in any `BaseService` subclass

## License

MIT
