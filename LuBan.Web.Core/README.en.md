[中文](README.md) | English

# LuBan.Web.Core

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **Launch enterprise-grade API services with a single line of code — JWT, Swagger, multi-tenancy, approval flow, and real-time communication all built-in.**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Redis](../LuBan.Redis/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md) | [LuBan.Wechat](../LuBan.Wechat/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md)

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
| Runtime | .NET 8.0 | ASP.NET Core minimal hosting |
| Cache | LuBan.Redis | Redis distributed cache & locks |
| System Service | Hosting.Systemd / WindowsServices | Register as system service |
| Approval Flow | LuBan.ApprovalFlow | Built-in approval flow engine |
| Report Export | LuBan.Reporting | Excel/CSV export |
| Cloud Storage | LuBan.CloudStorage | Object storage integration |
| Logging | LuBan.LogLib | Unified logging library |

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
- **Cookie Web Authentication**: `[WebLoginAuth]` for admin web pages

### Unified Response Format

All APIs return a unified structure (automatically wrapped by `ApiResultConvertionAttribute`):

```json
{
  "code": 200,
  "type": "success",
  "message": "Operation successful",
  "result": { ... },
  "extras": {},
  "time": 1720000000
}
```

### Core Middleware & Filters

| Component | Description |
|-----------|-------------|
| `ErrorHandlingMiddleware` | Global exception handling, unified error response |
| `OnlineUserMiddleware` | Online user statistics and management |
| `AraParameterFilterAttribute` | Anti-replay attack (timestamp + signature) |
| `DistributedLockAttribute` | Redis distributed lock, prevents concurrent duplicate operations |
| `OutputCacheAttribute` | API output caching |
| `CacheableAttribute` | Declarative caching |
| `CacheClearAttribute` | Cache clearing |
| `IPWhiteListFilterAttribute` | IP whitelist filtering |
| `DataScopePermissionFilter` | Data permissions (self/department/organization/all) |
| `InputArgsValidateActionFilter` | Automatic input validation |

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

- **Background Jobs**: `JobsController` + `JobServiceLoader` manage scheduled tasks
- **Health Checks**: `HealthCheckService` supports WeCom robot alerts
- **SSE Streaming**: `SseStream` server-side push
- **File Upload/Download**: `UploadFileUtil` / `DownloadFileUtil` / `ExtraFileController`
- **Graphic Captcha**: Built-in captcha generation
- **API Stress Testing**: `CommonController.StressTest`
- **System Service Deployment**: Supports Windows Service and systemd

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
    [DistributedLock("order_{userId}", Seconds = 5)]
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
4. **Background Jobs**: Use `BackgroundJobNames` to precisely control which background jobs are enabled; an empty list enables all
5. **Approval Flow Integration**: When `ApprovalFlowOptions.AutoApproval = true`, `FlowEngine` starts and stops with the service automatically

---

## License

Copyright (c) yswenli. All Rights Reserved.
