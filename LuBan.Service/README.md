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

// 后台任务 — 声明即用
[JobInfo("数据清理任务", "每日凌晨清理过期数据")]
public class DataCleanupJob : BaseJobService
{
    // 构造参数为毫秒间隔；也可用 base(2, 0, 0) 指定时间点，或 base("0 0 2 * * *") 指定 cron
    public DataCleanupJob() : base(24 * 60 * 60 * 1000) { }

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
| 缓存访问 | 静态属性 `ServiceCache` 从 `DiContainer` 解析 `IServiceCache`，开箱即用 |
| 单例实例 | `BaseService<T> : BaseSingleInstance<T>`，通过 `Instance` 惰性获取单例（**基类不内置仓储属性**，仓储请自行 new 或构造注入） |

### 后台任务框架

| 功能 | 说明 |
|------|------|
| 任务接口 | `IJob` 定义标准任务契约（IsRunning、Run、RunAsync、Start、Stop） |
| 调度引擎 | `BaseBackgroundService` 核心调度逻辑，支持间隔调度、时间点调度与 Cron 表达式调度 |
| 任务基类 | `BaseJobService` 抽象基类，构造时传入调度参数（`intervalTime` 毫秒 / `(hour, minute, second)` / cron 字符串）；运行中可用 `SetCron` 改调度 |
| 子任务基类 | `BaseSubJobService` 定义每隔 `IntervalTime`（默认 100ms）执行一次的子任务，由父任务统一驱动 |
| 父任务 | `ParentJobService` 框架内置，反射加载全部 `BaseSubJobService` 并按各自间隔轮询执行 |
| 配置服务 | `JobConfigService` 作业配置（cron / 是否启用 / 备注）的初始化、查询与更新 |
| 日志服务 | `JobLogService` 记录每次执行的开始/成功/失败，提供分页查询、状态查询与日志清理 |
| 自动发现 | `JobServiceLoader` 自动扫描所有 `IJob` 实现，无需手动注册 |
| 任务标记 | `JobInfoAttribute` 声明任务元数据（名称、描述等） |

## 使用指南

### 1. 业务服务

```csharp
// 无泛型基类 — 适合不绑定特定实体的服务
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
        return SuccessResult("系统运行正常");
    }
}

// 泛型基类 — 提供 Instance 单例，仓储需自行持有
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

### 2. 后台任务

```csharp
// 定义间隔任务
[JobInfo("缓存刷新", "每 30 分钟刷新系统缓存")]
public class CacheRefreshJob : BaseJobService
{
    public CacheRefreshJob() : base(30 * 60 * 1000) { }  // 每 30 分钟

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
    void Stop(CancellationToken cancellationToken);  // 支持取消/超时的停止
}
```

### 3. Cron 表达式调度

支持三种构造方式设置调度策略，底层统一基于 Cron 引擎（Cronos）调度：

```csharp
// 方式一：间隔调度（自动映射为 cron，仅能被 60/60/24 整除的秒/分/时及每天可精确映射）
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
| `0 0 0 * * *` | 每天 00:00:00 |
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

通过 HTTP 接口动态管理（内置 `JobsController`，端点全部为 `[HttpPost]`）：

- `POST api/admin/Jobs/GetJobCron?name=xxx` — 查询 cron 表达式
- `POST api/admin/Jobs/GetJobNextOccurrence?name=xxx` — 查询下一次执行时间
- `POST api/admin/Jobs/UpdateJobConfig?name=xxx&cron=...&isEnabled=...&remark=...` — 更新作业配置（cron 变更即时生效，`isEnabled` 变更会启停作业）

### 4. 任务自动发现

```csharp
// JobServiceLoader 自动扫描所有实现了 IJob 的类
// 配合 JobInfoAttribute 获取任务元数据
// 无需手动注册，新增任务类即可被自动发现与调度
```

## 小贴士

- 始终使用 `GetResultAsync()` 包装业务逻辑，自动处理异常并返回统一格式
- `BaseService<T>` 只提供 `Instance` 单例与响应封装，**没有内置仓储**；仓储请自行 `new BaseRepository<T>()` 或依赖注入
- 后台任务继承 `BaseJobService` 时通过构造参数（毫秒间隔 / 时间点 / cron）设置调度，运行中可 `SetCron` 动态调整
- 使用 `JobInfo("名称", "描述")` 为任务添加名称与描述，便于管理与监控
- `IServiceCache` 通过静态属性 `ServiceCache` 从 DI 容器解析，可在任何 `BaseService` 子类中使用

## 许可证

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
