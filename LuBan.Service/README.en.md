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

```xml
<PackageReference Include="LuBan.Service" Version="*" />
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
| Scheduling Engine | `BaseBackgroundService` core scheduling logic, supports interval and time-point scheduling |
| Task Base Class | `BaseJobService` abstract base class — configure `Interval` and it runs |
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

### 3. Task Auto-Discovery

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
