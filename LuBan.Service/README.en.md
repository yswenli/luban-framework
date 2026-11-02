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
    readonly BaseRepository<Order> _repository = new();

    public async Task<Result> GetOrderAsync(long id)
    {
        return await GetResultAsync(async () =>
        {
            var order = await _repository.GetFirstAsync(x => x.Id == id);
            return order.ConvertTo<OrderDto>();
        });
    }
}

// Background task — declare and use
[JobInfo("Data Cleanup Task", "Clean up expired data daily at midnight")]
public class DataCleanupJob : BaseJobService
{
    // Constructor argument is a millisecond interval; base(2, 0, 0) sets a time point, base("0 0 2 * * *") sets cron
    public DataCleanupJob() : base(24 * 60 * 60 * 1000) { }

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
| Cache Access | Static property `ServiceCache` resolves `IServiceCache` from `DiContainer`, ready to use out of the box |
| Singleton Instance | `BaseService<T> : BaseSingleInstance<T>` exposes a lazy singleton via `Instance` (**no built-in repository property** — new one up or inject one yourself) |

### Background Task Framework

| Feature | Description |
|---------|-------------|
| Task Interface | `IJob` defines standard task contract (IsRunning, Run, RunAsync, Start, Stop) |
| Scheduling Engine | `BaseBackgroundService` core scheduling logic, supports interval, time-point and Cron expression scheduling |
| Task Base Class | `BaseJobService` abstract base class — pass scheduling parameters to the constructor (`intervalTime` in ms / `(hour, minute, second)` / cron string); `SetCron` reschedules at runtime |
| Sub-task Base Class | `BaseSubJobService` defines sub-tasks that run once every `IntervalTime` (default 100 ms), driven by a parent task |
| Parent Task | `ParentJobService` built into the framework, reflectively loads every `BaseSubJobService` and polls them at their own intervals |
| Configuration Service | `JobConfigService` initializes, queries and updates job configuration (cron / enabled flag / remark) |
| Log Service | `JobLogService` records start/success/failure of every run, with paged queries, status lookup and log cleanup |
| Auto Discovery | `JobServiceLoader` auto-scans all `IJob` implementations, no manual registration needed |
| Task Annotation | `JobInfoAttribute` declares task metadata (name, description, etc.) |

## Usage Guide

### 1. Business Services

```csharp
// Non-generic base class — suitable for services not bound to a specific entity
public class ReportService : BaseService
{
    public async Task<Result> GenerateReportAsync(string type)
    {
        return await GetResultAsync(async () =>
        {
            var data = await CollectDataAsync(type);
            return new ReportDto { Type = type, Data = data };
        });
    }

    public Result GetStatus()
    {
        return SuccessResult("System running normally");
    }
}

// Generic base class — provides the Instance singleton; hold the repository yourself
public class UserService : BaseService<DbUser>
{
    readonly BaseRepository<DbUser> _repository = new();

    public async Task<Result> GetAllAsync()
    {
        return await GetResultAsync(async () =>
        {
            var users = await _repository.AsQueryable().ToListAsync();
            return users.ConvertTo<List<UserDto>>();
        });
    }
}
```

### 2. Background Tasks

```csharp
// Define interval task
[JobInfo("Cache Refresh", "Refresh system cache every 30 minutes")]
public class CacheRefreshJob : BaseJobService
{
    public CacheRefreshJob() : base(30 * 60 * 1000) { }  // Every 30 minutes

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
    void Stop(CancellationToken cancellationToken);  // stop with cancellation/timeout support
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

HTTP API for dynamic management (built-in `JobsController`, all endpoints are `[HttpPost]`):

- `POST api/admin/Jobs/GetJobCron?name=xxx` — Query cron expression
- `POST api/admin/Jobs/GetJobNextOccurrence?name=xxx` — Query next execution time
- `POST api/admin/Jobs/UpdateJobConfig?name=xxx&cron=...&isEnabled=...&remark=...` — Update job configuration (a cron change takes effect immediately, an `isEnabled` change starts/stops the job)

### 4. Task Auto-Discovery

```csharp
// JobServiceLoader auto-scans all classes implementing IJob
// Combined with JobInfoAttribute to retrieve task metadata
// No manual registration needed — new task classes are automatically discovered and scheduled
```

## Tips

- Always use `GetResultAsync()` to wrap business logic for automatic exception handling and unified response format
- `BaseService<T>` only provides the `Instance` singleton and response helpers — **there is no built-in repository**; new up `BaseRepository<T>()` or inject one
- Derive background tasks from `BaseJobService` and pass scheduling parameters to the constructor (millisecond interval / time point / cron); use `SetCron` to reschedule at runtime
- Use `JobInfo("Name", "Description")` to add names and descriptions to tasks for easier management and monitoring
- `IServiceCache` is resolved from the DI container through the static `ServiceCache` property and can be used in any `BaseService` subclass

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
