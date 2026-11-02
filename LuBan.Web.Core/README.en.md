[中文](README.md) | English

# LuBan.Web.Core

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **Launch enterprise-grade API services with a single line of code — JWT, Swagger, multi-tenancy, approval flow, and real-time communication all built-in.**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Logging](../LuBan.Logging/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Redis](../LuBan.Redis/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md) | [LuBan.Wechat](../LuBan.Wechat/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md)

---

## Why Choose LuBan.Web.Core?

Are you tired of repeatedly configuring JWT authentication, Swagger grouping, global exception handling, unified response formats, and multi-tenant database routing for every new project?  
LuBan.Web.Core wraps all this tedious work into a single startup line, letting you focus on business logic.

---

## Quick Preview

```csharp
// Program.cs — Start a complete API service in just two lines
WebApp.RunWebHost(args);

// Or asynchronously
await WebApp.RunWebHostAsync(args);
```

That simple. The framework automatically reads the `HostingOptions` configuration from `appsettings.json`, completing DI registration, middleware pipeline setup, database initialization, background job startup, and all other tasks.

---

## Tech Stack & Dependencies

| Category | Component | Description |
|----------|-----------|-------------|
| Runtime | .NET 10.0 | ASP.NET Core minimal hosting |
| Cache | LuBan.Redis | Redis distributed cache & locks |
| System Service | Microsoft.Extensions.Hosting.Systemd / Microsoft.Extensions.Hosting.WindowsServices | Register as system service |
| Approval Flow | LuBan.ApprovalFlow | Built-in approval flow engine |
| Report Export | LuBan.Reporting | Excel/CSV export |
| Cloud Storage | LuBan.CloudStorage | Object storage integration |
| Logging | LuBan.Logging | File logging provider built on Microsoft.Extensions.Logging, 100MB size / cross-day rolling |
| Common Foundation | LuBan.Common | Unified response (`Result` / `Success` / `Fail`), paging, crypto helpers and general utilities |
| Data Access | LuBan.Orm | Entity and repository base classes that `DbRepository<TEntity>` derives from |
| Background Jobs | LuBan.Service | Job hosting, configuration and logs (server-side backing for `JobsController`) |
| AI Agent | LuBan.AIAgent | Connector abstractions and generic external MCP integration |
| Speech | LuBan.Speech | Speech recognition / synthesis |

---

## Installation

```bash
dotnet add package LuBan.Web.Core
```

---

## Configuration

Add core configuration in `appsettings.json`:

```json
{
  "HostingOptions": {
    "ServiceName": "MyAPIService",
    "Domain": "https://api.example.com",
    "EnableRedisCache": true,
    "EnableBackgroundJob": true,
    "EnableHealthCheck": true,
    "EnableVideoThumbnail": false,
    "AppOptions": {
      "Urls": [ "http://0.0.0.0:5000" ],
      "StartPath": "/swagger"
    },
    "NacosConfig": {
      "ServerAddresses": [ "http://nacos:8848" ],
      "Namespace": "production"
    }
  }
}
```

### LuBan Agent Integration

The standard startup chain (`WebApp.RunWebHost`) tries to register LuBan Agent automatically. Configuration is resolved in this order:

1. the `db_config` record with `Code = sys-luban-framework:aiagent`, whose `Value` is a JSON object shaped like the `LuBanAgent` section;
2. when that record is absent, the host configuration (`LuBanAgent` section in `appsettings.json`, environment variables);
3. neither present, or present but incomplete/invalid → **the agent stays disabled**, the web app still starts, and one yellow reason line is printed to the console.

```json
{
  "LuBanAgent": {
    // absolute path, or a path relative to the site content root (ContentRootPath)
    "WorkspaceRoot": ".",
    // optional; defaults to the first entry of Models
    "DefaultModel": "deepseek:deepseek-chat",
    "Models": [
      {
        "Provider": "deepseek",
        "Model": "deepseek-chat",
        "ApiKey": "sk-***",
        // optional, but only safe when the provider has a built-in endpoint (deepseek/openai/kimi/ollama, ...);
        // self-hosted gateways, proxies and providers outside the built-in table must set it explicitly,
        // otherwise the SDK falls back to the OpenAI default endpoint
        "BaseUrl": "https://api.deepseek.com/v1/"
      }
    ]
  }
}
```

Validation:

- `Models` is an array. Each entry needs a non-empty `Provider` (case-insensitive; providers outside the built-in table are allowed as custom providers) and non-empty `Model` / `ApiKey`; `BaseUrl`, when supplied, must be a valid http(s) URL.
- **Built-in providers may omit `BaseUrl`**; the built-in endpoint is then used (see `ProviderDefaults`). Built-in providers that have no built-in endpoint (`azure` / `claude` / `gemini`) get a null `Endpoint`, so the OpenAI SDK falls back to `https://api.openai.com/v1/` and a `ChatClientFactoryWarning` is emitted.
- **Providers outside the built-in table (self-hosted gateways, proxies, vendors not listed) must set `BaseUrl` explicitly**, for example `"Provider": "internal-llm"` with `"BaseUrl": "http://10.10.0.147:11689/v1"`. Without it the agent is reported as disabled at startup instead of silently hitting the OpenAI default endpoint.
- The same provider may appear multiple times for different models, but every entry must share the same `ApiKey` / `BaseUrl`; otherwise that provider is not registered.
- `DefaultModel` is optional (defaults to the first entry). When present it must be `provider:model` and match an entry in `Models`.
- `WorkspaceRoot` accepts absolute or relative paths. A relative value is resolved against the site `ContentRootPath`; non-web hosts (for example unit tests) fall back to the program directory.
  Note that **on Windows a value starting with `/` counts as absolute** (`/aiagent` → the current drive root, e.g. `D:\aiagent`); use `aiagent` or `./aiagent` to keep it inside the site.

> Upgrade note: the legacy flat object form (`"Models": { "Provider": ... }`) is no longer supported — convert it to an array.

To disable: remove the `LuBanAgent` section and leave no `db_config` record (there is no half-on switch). A failing agent initialization degrades to "disabled" and never prevents the web app from starting.

---

## Feature Overview

### Startup Entry Points

| API | Description |
|-----|-------------|
| `WebApp.RunWebHost(args)` | Start web service synchronously |
| `WebApp.RunWebHostAsync(args)` | Start web service asynchronously |
| `WebApp.HostingOptions` | Get global configuration |
| `WebApp.HttpContext` | Statically get current request context |
| `WebApp.User` | Statically get current JWT user |
| `WebApp.ServiceCache` | Get cache service instance |
| `WebApp.RootPath` | Site root directory path |
| `WebApp.GetPhysicalPath(...)` | Get physical file path |

### Controller Inheritance Hierarchy

The framework provides 6 base controller classes covering different access scenarios:

```
BaseApiController       — JWT auth, route api/, Swagger group default
├── BaseAdminController — Admin, route api/admin/, group admin, role authorization required
├── BaseMobileController— Mobile, route api/mobile/, group mobile
├── BaseInternalController — Internal API, route api/internal/, group internal
└── BaseOpenController  — Open API, route api/open/, group open, anonymous + anti-replay + OpenAPI token

BaseWebController       — Cookie auth, admin web pages
```

### Authentication & Authorization

- **JWT Authentication**: `JwtConfigureService` auto-configuration, `SessionUser` to get current user
- **Role Access Control**: `[ForbiddenAccess]` restricts admin, `[AllowAccess]` allows mobile/internal
- **Open API Authentication**: `[OpenApiAccess]` refresh/access token-based open API authentication
- **Internal Service API Authentication**: `[InternalApiAccess]` only requires Bearer {RefreshToken} for internal service-to-service calls, no need for full AccessToken flow
- **Cookie Web Authentication**: `[WebLoginAuth]` for admin web pages

### Unified Response Format

All APIs return a unified structure (automatically wrapped by `ApiResultConvertionAttribute`):

```json
{
  "code": 200,
  "type": "Success",
  "message": "OK",
  "result": { ... },
  "time": "2026-10-30 16:00:00"
}
```

- `type` is `Success` or `Fail`; `message` is `OK` on success.
- `time` is a **string** in `yyyy-MM-dd HH:mm:ss` format (serialized by `DateTimeJsonConverter`).
- `extras` defaults to `null` and null fields are skipped during serialization, so it usually **does not appear** and may be omitted.

### Core Middleware & Filters

| Component | Description |
|-----------|-------------|
| `ErrorHandlingMiddleware` | Global exception handling, unified error response |
| `OnlineUserMiddleware` | Online user statistics and management |
| `AraParameterFilterAttribute` | Anti-replay attack (timestamp + signature) |
| `DistributedLockAttribute` | Redis distributed lock, prevents concurrent duplicate operations; constructor `(lockName, variableType, variableName, timeout=10000ms, maxRetries=5, dbIndex=0)`, where the variable segment is appended to the lock key by `EnumVariableType` (`Query`/`Form`/`Route`/`Header`/`Cookie` read `VariableName`; `ContextUser` uses the current user ID) |
| `OutputCacheAttribute` | API output caching |
| `CacheableAttribute` | Declarative caching |
| `CacheClearAttribute` | Cache clearing |
| `IPWhiteListFilterAttribute` | IP whitelist filtering |
| `DataScopePermissionFilter` | Data permissions (self/department/organization/all); a **static helper class**, not a filter attribute, used to build and validate the data-scope predicate |
| `IgnoreDataScopePermissionAttribute` | Skips data-scope validation on a class or method |
| `InputArgsValidateActionFilterAttribute` | Automatic input validation |

### Multi-Tenant Database

`DbRepository<TEntity>` automatically parses tenant ID from JWT and routes to the corresponding database:

```csharp
// Automatically selects database connection based on current user's tenant
var repo = new DbRepository<SysUser>();
var users = await repo.GetListAsync();

// Switch entity repository
var userRepo = repo.Change<SysRole>();
```

### Swagger Grouped Documentation

Automatically generates 5 API documentation groups: `default`, `admin`, `mobile`, `internal`, `open`, with Markdown export and JS SDK generation support.

### SignalR Real-time Communication

Built-in `CommonHub`, supports `IHubClient` / `IHubServer` interfaces and `SimpleClient` for quick integration.

### Other Capabilities

- **Background Jobs**: `JobsController` + `JobServiceLoader` manage scheduled tasks, with runtime dynamic Cron expression updates
- **Health Checks**: `HealthCheckService` supports WeCom robot alerts
- **SSE Streaming**: `SseStream` server-side push; `SseEventWriter` / `SseEventPump` write named event frames (`event: <name>` + single-line JSON `data:`) with idle comment heartbeats, and do not depend on `WebApp.HttpContext`, so they are easy to unit test
- **Agent Session Ownership**: `WebAgentSessionRegistry` (`LuBan.Web.Core.AIAgent`) keeps each signed-in user's "last session" and validates ownership, so the process-wide pointer of the singleton `ISessionManager` is not overwritten across users
- **File Upload/Download**: `UploadFileUtil` / `DownloadFileUtil` / `ExtraFileController`
- **Graphic Captcha**: Built-in captcha generation
- **API Stress Testing**: `CommonController.StressTest`
- **System Service Deployment**: Supports Windows Service and systemd

### Background Job Cron Management

`JobsController` exposes the following endpoints (**all `[HttpPost]`**) for querying and dynamically updating job configuration at runtime:

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `api/admin/Jobs/GetJobCron?name=xxx` | Query a job's Cron expression |
| POST | `api/admin/Jobs/GetJobNextOccurrence?name=xxx` | Query a job's next execution time |
| POST | `api/admin/Jobs/GetJobConfigs` | Query all job configurations |
| POST | `api/admin/Jobs/UpdateJobConfig?name=xxx&cron=...&isEnabled=...&remark=...` | Update job configuration: a Cron change takes effect immediately, an `isEnabled` change starts/stops the job |

All parameters bind from query string or form (`UpdateJobConfig(name, cron, isEnabled, remark)`, the last three optional).

Cron expressions use the 6-segment seconds format (`seconds minutes hours day month weekday`), parsed by [Cronos](https://www.nuget.org/packages/Cronos/).

---

## Code Examples

### Custom Business Controller

```csharp
// Admin API
public class UserController : BaseAdminController
{
    [HttpGet]
    public async Task<ApiResult> GetList()
    {
        var repo = new DbRepository<SysUser>();
        var list = await repo.GetListAsync();
        return Success(list);
    }
}

// Mobile API
public class OrderController : BaseMobileController
{
    [HttpPost]
    // Lock key = "order" + "_" + the value of the userId query parameter, e.g. order_1001
    [DistributedLock("order", EnumVariableType.Query, "userId", timeout: 5000)]
    public async Task<ApiResult> CreateOrder([FromBody] OrderInput input)
    {
        // Business logic
        return Success();
    }
}

// Open API (anonymous access + anti-replay + OpenAPI token)
public class DataSyncController : BaseOpenController
{
    [HttpPost]
    public async Task<ApiResult> SyncData([FromBody] SyncInput input)
    {
        return Success();
    }
}

// Internal service API (only requires Bearer {RefreshToken})
public class InternalApiController : BaseInternalController
{
    [InternalApiAccess]
    [HttpPost]
    public async Task<ApiResult> GetData([FromBody] GetDataInput input)
    {
        // No AccessToken needed, directly use RefreshToken for authentication
        return Success();
    }
}
```

### Register Startup Events

```csharp
WebApp.OnStarted += () =>
{
    Console.WriteLine("Service started, executing custom initialization...");
};

WebApp.RunWebHost(args);
```

### Command Line Arguments

```bash
# Specify listening address
dotnet run --urls "http://0.0.0.0:5000;http://0.0.0.0:5001"

# Specify FFmpeg path (enable video thumbnails)
dotnet run --ffmpeg "c:/bin/ffmpeg.exe"

# Specify runtime environment
dotnet run --environment Production
```

---

## Tips

1. **Pre-launch Check**: Ensure `HostingOptions` configuration in `appsettings.json` is complete, especially `ServiceName`, `Domain`, `AppOptions.Urls`
2. **Multi-tenancy**: JWT must contain `TenantId` Claim, otherwise the primary database is used by default
3. **Open API**: `BaseOpenController` requires all three: `[AllowAnonymous]` + `[OpenApiAccess]` + `[AraParameterFilter]`
4. **Internal Service API**: Use `[InternalApiAccess]` attribute, only requires `Bearer {RefreshToken}` without obtaining AccessToken
5. **Background Jobs**: Use `BackgroundJobNames` to precisely control which background jobs are enabled; an empty list enables all
6. **Approval Flow Integration**: When `ApprovalFlowOptions.AutoApproval = true`, `FlowEngine` starts and stops with the service automatically

---

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
