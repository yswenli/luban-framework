[English](README.en.md) | 中文

# LuBan.Service

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 业务服务与后台任务的统一基座，让每个服务都站在巨人的肩膀上。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Speech](../LuBan.Speech/README.md) | [LuBan.Wechat](../LuBan.Wechat/README.md)
---

## 为什么需要它？

- 每个 Service 都在重复写 try/catch、统一返回值封装？
- 后台任务调度逻辑散落各处，没有统一管理？
- 定时任务、间隔任务、时间点任务的调度逻辑每次都要重新实现？
- 任务发现与加载靠手动注册，新增任务容易遗漏？

LuBan.Service 提供标准化的业务服务基类与后台任务框架，统一返回值封装、异常处理、缓存访问，并内置可配置的任务调度引擎与自动发现机制。

## 快速预览

```csharp
// 业务服务 — 继承即用
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

// 后台任务 — 声明即用
[JobInfo(Name = "数据清理任务", Description = "每日凌晨清理过期数据")]
public class DataCleanupJob : BaseJobService
{
    protected override TimeSpan Interval => TimeSpan.FromHours(24);

    public override async Task RunAsync()
    {
        await CleanupExpiredRecordsAsync();
    }
}
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.Common | 基础接口与通用模型 |
| LuBan.DI | 依赖注入扩展 |
| LuBan.Orm | 数据访问层（仓储、实体） |

## 安装

```bash
dotnet add package LuBan.Service
```

## 功能总览

### 业务服务基类

| 功能 | 说明 |
|------|------|
| 统一返回值 | `SuccessResult()` / `ErrorResult()` 标准化响应格式 |
| 异常封装 | `GetResult()` / `GetResultAsync()` 内置 try/catch，异常自动包装为错误结果 |
| 缓存访问 | 通过 DI 注入 `IServiceCache`，开箱即用 |
| 仓储集成 | `BaseService<T>` 自动关联 `BaseRepository<T>` |

### 后台任务框架

| 功能 | 说明 |
|------|------|
| 任务接口 | `IJob` 定义标准任务契约（IsRunning、Run、RunAsync、Start、Stop） |
| 调度引擎 | `BaseBackgroundService` 核心调度逻辑，支持间隔调度、时间点调度与 Cron 表达式调度 |
| 任务基类 | `BaseJobService` 抽象基类，配置调度参数即可运行 |
| 自动发现 | `JobServiceLoader` 自动扫描所有 `IJob` 实现，无需手动注册 |
| 任务标记 | `JobInfoAttribute` 声明任务元数据（名称、描述等） |

## 使用指南

### 1. 业务服务

```csharp
// 无泛型基类 — 适合不绑定特定实体的服务
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
        return SuccessResult("系统运行正常");
    }
}

// 泛型基类 — 自动绑定仓储
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

### 2. 后台任务

```csharp
// 定义间隔任务
[JobInfo(Name = "缓存刷新", Description = "每 30 分钟刷新系统缓存")]
public class CacheRefreshJob : BaseJobService
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(30);

    public override async Task RunAsync()
    {
        await RefreshSystemCacheAsync();
    }
}

// 任务生命周期
public interface IJob
{
    bool IsRunning { get; }
    void Run();
    Task RunAsync();
    void Start();
    void Stop();
}
```

### 3. Cron 表达式调度

支持三种构造方式设置调度策略，底层统一基于 Cron 引擎（Cronos）调度：

```csharp
// 方式一：间隔调度（自动映射为 cron，仅整秒/整分/整时/整天可精确映射）
public class IntervalJob : BaseJobService
{
    public IntervalJob() : base(5 * 60 * 1000) { }  // 每 5 分钟 => "0 */5 * * * *"

    public override async Task RunAsync() { /* ... */ }
}

// 方式二：时间点调度（HH:mm:ss，底层映射为 cron）
public class TimePointJob : BaseJobService
{
    public TimePointJob() : base(2, 30, 0) { }  // 每天 02:30:00 => "0 30 2 * * *"

    public override async Task RunAsync() { /* ... */ }
}

// 方式三：直接使用 6 段秒级 cron 表达式
public class CronJob : BaseJobService
{
    public CronJob() : base("0 0 8 * * 1") { }  // 每周一 08:00:00

    public override async Task RunAsync() { /* ... */ }
}
```

Cron 格式为 **6 段秒级**：`秒 分 时 日 月 周`，例如：

| 表达式 | 含义 |
|--------|------|
| `*/10 * * * * *` | 每 10 秒 |
| `0 */5 * * * *` | 每 5 分钟 |
| `0 30 2 * * *` | 每天 02:30:00 |
| `0 0 0 */2 * *` | 每 2 天的 00:00:00 |
| `0 0 8 * * 1` | 每周一 08:00:00 |

动态操作：

```csharp
// 读取当前 cron 表达式
var cron = job.Cron;

// 查询下一次执行时间（本地时区）
var next = job.GetNextOccurrence();

// 动态更新 cron（运行中立即生效，未运行时待 Start 生效）
job.SetCron("0 15 3 * * *");
job.Cron = "0 0 12 * * *";  // 属性赋值等价于 SetCron
```

通过 HTTP 接口动态管理（内置 `JobsController`）：

- `GET api/admin/Jobs/GetJobCron?name=xxx` — 查询 cron 表达式
- `GET api/admin/Jobs/GetJobNextOccurrence?name=xxx` — 查询下一次执行时间
- `PUT api/admin/Jobs/UpdateJobCron?name=xxx` — 更新运行中任务的 cron（body: `{ "cron": "0 0 8 * * *" }`）

### 4. 任务自动发现

```csharp
// JobServiceLoader 自动扫描所有实现了 IJob 的类
// 配合 JobInfoAttribute 获取任务元数据
// 无需手动注册，新增任务类即可被自动发现与调度
```

## 小贴士

- 始终使用 `GetResultAsync()` 包装业务逻辑，自动处理异常并返回统一格式
- `BaseService<T>` 中的 `Repository` 属性直接可用，无需额外注入
- 后台任务只需继承 `BaseJobService` 并设置 `Interval`，调度引擎自动处理
- 使用 `JobInfoAttribute` 为任务添加名称与描述，便于管理与监控
- `IServiceCache` 通过 DI 注入，可在任何 `BaseService` 子类中使用

## 许可证

MIT
