[中文](README.md) | English

# LuBan.Framework

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **One-stop .NET 8 Enterprise API Development Framework — From ORM to AI Agent, Ready Out of the Box.**

LuBan.Framework is an enterprise-grade API framework built on ASP.NET Core, integrating the most commonly needed infrastructure for enterprise development, so you can spend your time on real business logic instead of reinventing the wheel.

---

**Related Projects**: [LuBan.Common](LuBan.Common/README.md) | [LuBan.Threading](LuBan.Threading/README.md) | [LuBan.DI](LuBan.DI/README.md) | [LuBan.Linq](LuBan.Linq/README.md) | [LuBan.Orm](LuBan.Orm/README.md) | [LuBan.EventBus](LuBan.EventBus/README.md) | [LuBan.Service](LuBan.Service/README.md) | [LuBan.Redis](LuBan.Redis/README.md) | [LuBan.LogLib](LuBan.LogLib/README.md) | [LuBan.CloudStorage](LuBan.CloudStorage/README.md) | [LuBan.E邮件处理库](LuBan.E邮件处理库/README.md) | [LuBan.PdfKit](LuBan.PdfKit/README.md) | [LuBan.Office](LuBan.Office/README.md) | [LuBan.VideoKit](LuBan.VideoKit/README.md) | [LuBan.Lives](LuBan.Lives/README.md) | [LuBan.Speech](LuBan.Speech/README.md) | [LuBan.Wechat](LuBan.Wechat/README.md) | [LuBan.Qingflow](LuBan.Qingflow/README.md) | [LuBan.Web.Core](LuBan.Web.Core/README.md) | [LuBan.ApprovalFlow](LuBan.ApprovalFlow/README.md) | [LuBan.Reporting](LuBan.Reporting/README.md) | [LuBan.AIFlow](LuBan.AIFlow/README.md) | [LuBan.AIAgent](LuBan.AIAgent/README.md)

---

## Why Choose LuBan.Framework?

| Pain Point | LuBan's Solution |
|------|-------------|
| Every project needs JWT + Swagger + unified response setup | `LuBan.Web.Core` one-line startup for complete web infrastructure |
| Multi-database, multi-tenant headaches | `LuBan.Orm` built-in multi-tenancy with attribute-based auto-routing |
| Redis distributed locks, caching, queues rewritten every time | `LuBan.Redis` wraps Lua script reentrant locks, pub/sub, Stream queues |
| Approval workflows built from scratch | `LuBan.ApprovalFlow` graph-theory approval engine, JSON-defined workflows |
| Chaotic AI integration | `LuBan.AIAgent` + `LuBan.AIFlow` unified Agent framework + multi-platform AI |
| WeChat, email, live streaming, speech each doing their own thing | Unified factory pattern, one interface for all platforms |
| Excel import/export, PDF, Office automation | `LuBan.Common` + `LuBan.PdfKit` + `LuBan.Office` full coverage |

---

## Framework Component Overview

Arranged by dependency hierarchy, from底层 utilities to upper-layer applications:

### Infrastructure Layer

| Component | Description |
|------|------|
| **LuBan.Common** | General utility library: HTTP client, Excel/CSV, caching, logging, encryption, imaging, QR codes, IP geolocation, pinyin, SMS, and 100+ utility classes |
| **LuBan.Threading** | Thread pool, named locks, blocking queues, Task timeout control |
| **LuBan.DI** | Convention-based dependency injection auto-registration + AOP proxy |
| **LuBan.Linq** | Dynamic LINQ: string field name queries, expression composition, tree conversion |

### Data & Service Layer

| Component | Description |
|------|------|
| **LuBan.Orm** | Multi-tenant ORM: CodeFirst table creation, snowflake IDs, soft delete, audit fields, data diff logging, code generator, 27 built-in system entities |
| **LuBan.EventBus** | In-process event bus: publish/subscribe, one-time subscriptions, Channel-driven |
| **LuBan.Service** | Business service base class + background task scheduling (interval/cron) |
| **LuBan.Redis** | Full-featured Redis SDK: distributed locks (Lua reentrant), caching, pub/sub, Stream queues, expiry listening |
| **LuBan.LogLib** | Database logging: batch API log and error log writing, automatic expiry cleanup |

### Integration Layer

| Component | Description |
|------|------|
| **LuBan.CloudStorage** | Multi-cloud storage unified interface: cloud provider OSS / 云存储 / 对象存储 |
| **LuBan.E邮件处理库** | Email send/receive suite: SMTP send / IMAP / POP3 receive |
| **LuBan.PdfKit** | PDF toolkit: text replacement, image replacement, HTML to PDF, image merge |
| **LuBan.Office** | Office document automation: PPT/Word/PDF creation & editing (Windows) |
| **LuBan.VideoKit** | Video thumbnail extraction |
| **LuBan.Lives** | Multi-platform live streaming SDK: Tuomai / Weizan / Yibai / Huichang / Weihu |
| **LuBan.Speech** | Speech recognition integration |
| **LuBan.Wechat** | WeChat full ecosystem: Official Account / WeCom / WeChat Pay / Tencent Location |
| **LuBan.Qingflow** | Qingflow Open API client: users, app data, approval flows, charts |

### Application & Engine Layer

| Component | Description |
|------|------|
| **LuBan.Web.Core** | Core web framework: JWT, Swagger grouping, SignalR, unified responses, anti-replay, distributed locks, data permissions, online users, file upload/download, health checks, SSE streams, API stress testing |
| **LuBan.ApprovalFlow** | Approval workflow engine: graph-theory model, condition gateways, countersign aggregation, task delegation, HTTP callbacks, event-driven |
| **LuBan.Reporting** | Data export: generic list Excel/CSV export + dynamic reports + Lua script conversion |
| **LuBan.AIFlow** | AI platform unified access: RagFlow / Dify / Coze |
| **LuBan.AIAgent** | AI Agent framework: tool calling, skill system, multi-model providers, session management, middleware pipeline |

---

## Project Dependency Graph

```
LuBan.Threading
      ↑
LuBan.Common ──────────────────────────────────────────────┐
      ↑                                                     │
LuBan.DI ──── LuBan.Linq                                    │
      ↑           ↑                                         │
LuBan.Orm ────────┘                                         │
      ↑                                                     │
LuBan.EventBus ── LuBan.Service ── LuBan.LogLib             │
                        ↑                                   │
LuBan.Redis ── LuBan.CloudStorage ── LuBan.EMailKit        │
LuBan.PdfKit ── LuBan.Office ── LuBan.VideoKit             │
LuBan.Lives ── LuBan.Speech ── LuBan.Wechat ── LuBan.Qingflow
                        ↑                                   │
LuBan.Reporting ── LuBan.ApprovalFlow                       │
LuBan.AIFlow ── LuBan.AIAgent                               │
                        ↑                                   │
              ┌─────────────────────────────────────────────┘
              │
        LuBan.Web.Core ← Integrates all components above, one-line startup
              ↑
      WebApplication1 / Your Project
```

---

## Quick Start

### 1. Create Project and Add Reference

```bash
dotnet add package LuBan.Web.Core
```

### 2. Program Entry Point

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        WebApp.OnStarted += () => Logger.Info($"{WebApp.ServiceName} started");
        WebApp.OnStopped += () => Logger.Info($"{WebApp.ServiceName} stopped");
        WebApp.RunWebHost<Startup>(args);
    }
}
```

### 3. Startup Configuration

```csharp
public class Startup : BaseStartup
{
    public Startup(IConfiguration configuration) : base(configuration) { }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        // Add your custom services
    }

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        base.Configure(app, env);
        // Add your custom middleware
    }
}
```

### 4. Write Controllers

```csharp
public class UserController : BaseApiController
{
    [HttpGet, DisplayName("Get User List")]
    public async Task<Result> GetUserList([FromQuery] UserQueryInput input)
    {
        var users = await _userService.GetUserListAsync(input);
        return SuccessResult(users);
    }

    [AllowAnonymous, HttpPost, DisplayName("Login")]
    public async Task<Result> Login([FromBody] UserLoginInput input)
    {
        var user = await _userService.ValidateUserAsync(input.UserName, input.Password);
        if (user == null) return ErrorResult("Invalid username or password");
        var token = await CreateToken(user);
        return SuccessResult(token);
    }
}
```

The framework automatically provides:
- Unified response format `{ code, type, message, result, extras, time }`
- JWT authentication / anti-replay attacks / global exception handling / API logging
- Swagger grouped documentation (default / admin / mobile / internal / open)
- Data permission filtering / online user management

---

## Configuration Examples

### Main Configuration (appsettings.json)

```json
{
  "HostingOptions": {
    "ServiceName": "LuBan.WebApplication1",
    "Domain": "https://your-domain.com/",
    "EnableHealthCheck": true,
    "AppOptions": {
      "Urls": [ "https://127.0.0.1:39654" ],
      "JwtAuthConfig": {
        "Secret": "base64:YOUR_SECRET_KEY",
        "Issuer": "your-issuer",
        "Audience": "LuBan.Framework",
        "AccessExpiration": 315576000
      },
      "MaxRequestSize": 134217728,
      "EnableSignalR": true,
      "SignalROptions": {
        "HubUrl": "/hubs/common",
        "HandshakeTimeout": 30,
        "KeepAliveInterval": 15,
        "FreeTimeout": 60
      }
    },
    "NacosConfig": {
      "Namespace": "dev",
      "ServerAddresses": [ "http://127.0.0.1:8848" ]
    }
  }
}
```

### Database Configuration

```json
"DbConnectionOptions": {
  "EnableConsoleSql": true,
  "EnableDBLogs": true,
  "ConnectionConfigs": [
    {
      "ConfigId": "1300000000001",
      "DbType": "Sqlite",
      "ConnectionString": "Data Source=app.db",
      "DbSettings": {
        "EnableInitDb": true,
        "EnableDiffLog": true,
        "EnableUnderLine": true
      },
      "TableSettings": { "EnableInitTable": true },
      "SeedSettings": { "EnableInitSeed": true }
    }
  ]
}
```

### Redis Configuration

```json
"RedisOptions": {
  "Password": "your-password",
  "Masters": "127.0.0.1:6379",
  "DefaultDatabase": 0,
  "ConnectTimeout": 10000
}
```

> For more configurations (WeChat, email, live streaming, speech, Qingflow, cloud storage, etc.), please refer to each sub-project's README.

---

## Unified Response Specification

All APIs automatically return a unified format:

**Success Response** (HTTP 200):
```json
{
  "code": "200",
  "type": "Success",
  "message": "OK",
  "result": { ... },
  "extras": null,
  "time": "2026-07-11 12:00:00"
}
```

**Business Exception** (HTTP 200):
```json
{
  "code": "200",
  "type": "Fail",
  "message": "[D1002] Record not found",
  "result": null,
  "extras": null,
  "time": "2026-07-11 12:00:00"
}
```

**System Exception** (HTTP 500):
```json
{
  "code": 999,
  "type": "Fail",
  "message": "Server API error, please contact administrator.",
  "result": null,
  "extras": null,
  "time": "2026-07-11 12:00:00"
}
```

---

## Version Info

[![NuGet version](https://img.shields.io/nuget/v/LuBan.Web.Core.svg?style=flat-square)](https://www.nuget.org/packages?q=LuBan)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)

- **Author**: yswenli
- **Target Framework**: .NET 8.0
- **Update Frequency**: Continuous iteration

---

## License

All rights reserved
