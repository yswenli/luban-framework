[English](README.en.md) | 中文

# LuBan.Framework

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **一站式 .NET 8 企业级 API 开发框架 —— 从 ORM 到 AI Agent，开箱即用。**

LuBan.Framework 是基于 ASP.NET Core 封装的企业级 API 框架，集成了企业开发中最常用的基础设施，让你把时间花在真正的业务上，而不是重复造轮子。

---

**Related Projects**: [LuBan.Common](LuBan.Common/README.md) | [LuBan.Threading](LuBan.Threading/README.md) | [LuBan.DI](LuBan.DI/README.md) | [LuBan.Linq](LuBan.Linq/README.md) | [LuBan.Orm](LuBan.Orm/README.md) | [LuBan.EventBus](LuBan.EventBus/README.md) | [LuBan.Service](LuBan.Service/README.md) | [LuBan.Redis](LuBan.Redis/README.md) | [LuBan.LogLib](LuBan.LogLib/README.md) | [LuBan.CloudStorage](LuBan.CloudStorage/README.md) | [LuBan.E邮件处理库](LuBan.E邮件处理库/README.md) | [LuBan.PdfKit](LuBan.PdfKit/README.md) | [LuBan.Office](LuBan.Office/README.md) | [LuBan.VideoKit](LuBan.VideoKit/README.md) | [LuBan.Lives](LuBan.Lives/README.md) | [LuBan.Speech](LuBan.Speech/README.md) | [LuBan.Wechat](LuBan.Wechat/README.md) | [LuBan.Qingflow](LuBan.Qingflow/README.md) | [LuBan.Web.Core](LuBan.Web.Core/README.md) | [LuBan.ApprovalFlow](LuBan.ApprovalFlow/README.md) | [LuBan.Reporting](LuBan.Reporting/README.md) | [LuBan.AIFlow](LuBan.AIFlow/README.md) | [LuBan.AIAgent](LuBan.AIAgent/README.md)

---

## 为什么选择 LuBan.Framework？

| 痛点 | LuBan 的解法 |
|------|-------------|
| 每个项目都要搭一遍 JWT + Swagger + 统一返回值 | `LuBan.Web.Core` 一行代码启动完整 Web 基础设施 |
| 多数据库、多租户搞到头秃 | `LuBan.Orm` 内置多租户支持，属性标注自动路由 |
| Redis 分布式锁、缓存、队列写一遍又一遍 | `LuBan.Redis` 封装 Lua 脚本可重入锁、发布订阅、Stream 队列 |
| 审批流程从零开发 | `LuBan.ApprovalFlow` 图论模型审批引擎，JSON 定义流程 |
| AI 能力接入混乱 | `LuBan.AIAgent` + `LuBan.AIFlow` 统一 Agent 框架 + 多平台 AI 接入 |
| 微信、邮件、直播、语音各自为战 | 统一工厂模式，一个接口对接所有平台 |
| Excel 导入导出、PDF 操作、Office 自动化 | `LuBan.Common` + `LuBan.PdfKit` + `LuBan.Office` 全覆盖 |

---

## 框架组件全景

按依赖层级排列，从底层工具到上层应用：

### 基础设施层

| 组件 | 说明 |
|------|------|
| **LuBan.Common** | 通用工具库：HTTP 客户端、Excel/CSV、缓存、日志、加密、图像、二维码、IP 归属地、拼音、短信等 100+ 工具类 |
| **LuBan.Threading** | 线程池、命名锁、阻塞队列、Task 超时控制 |
| **LuBan.DI** | 约定式依赖注入自动注册 + AOP 代理 |
| **LuBan.Linq** | 动态 LINQ：字符串字段名查询、表达式组合、树形转换 |

### 数据与服务层

| 组件 | 说明 |
|------|------|
| **LuBan.Orm** | 多租户 ORM：CodeFirst 建表、雪花 ID、软删除、审计字段、数据 Diff 日志、代码生成器、27 个内置系统实体 |
| **LuBan.EventBus** | 进程内事件总线：发布/订阅、一次性订阅、Channel 驱动 |
| **LuBan.Service** | 业务服务基类 + 后台任务调度（间隔/定时） |
| **LuBan.Redis** | Redis 全功能 SDK：分布式锁（Lua 可重入）、缓存、发布订阅、Stream 队列、过期监听 |
| **LuBan.LogLib** | 数据库日志：批量写入 API 日志和错误日志，自动过期清理 |

### 功能集成层

| 组件 | 说明 |
|------|------|
| **LuBan.CloudStorage** | 多云存储统一接口：云存储 / 云存储 / 对象存储 |
| **LuBan.E邮件处理库** | 邮件收发套件：SMTP 发送 / IMAP / POP3 接收 |
| **LuBan.PdfKit** | PDF 工具包：文本替换、图片替换、HTML 转 PDF、图片合并 |
| **LuBan.Office** | Office 文档自动化：PPT/Word/PDF 创建与编辑（Windows） |
| **LuBan.VideoKit** | 视频缩略图提取 |
| **LuBan.Lives** | 多平台直播 SDK：拓麦 / 微赞 / 一百 / 会畅 / 微吼 |
| **LuBan.Speech** | 语音识别集成 |
| **LuBan.Wechat** | 微信全生态：公众号 / 企业微信 / 微信支付 / 腾讯位置 |
| **LuBan.Qingflow** | 轻流 Open API 客户端：用户、应用数据、审批流、图表 |

### 应用与引擎层

| 组件 | 说明 |
|------|------|
| **LuBan.Web.Core** | 核心 Web 框架：JWT、Swagger 分组、SignalR、统一返回值、防重放、分布式锁、数据权限、在线用户、文件上传下载、健康检查、SSE 流、API 压测 |
| **LuBan.ApprovalFlow** | 审批流引擎：图论模型、条件网关、会签聚合、任务委派、HTTP 回调、事件驱动 |
| **LuBan.Reporting** | 数据导出：泛型列表导出 Excel/CSV + 动态报表 + Lua 脚本转换 |
| **LuBan.AIFlow** | AI 平台统一接入：RagFlow / Dify / Coze |
| **LuBan.AIAgent** | AI Agent 框架：工具调用、技能系统、多模型 Provider、会话管理、中间件管线 |

---

## 项目依赖关系

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
        LuBan.Web.Core ← 整合以上所有组件，一行启动
              ↑
      WebApplication1 / 你的项目
```

---

## 快速开始

### 1. 创建项目并引用

```bash
dotnet add package LuBan.Web.Core
```

### 2. 程序入口

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        WebApp.OnStarted += () => Logger.Info($"{WebApp.ServiceName} 已启动");
        WebApp.OnStopped += () => Logger.Info($"{WebApp.ServiceName} 已停止");
        WebApp.RunWebHost<Startup>(args);
    }
}
```

### 3. Startup 配置

```csharp
public class Startup : BaseStartup
{
    public Startup(IConfiguration configuration) : base(configuration) { }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        // 添加你的自定义服务
    }

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        base.Configure(app, env);
        // 添加你的自定义中间件
    }
}
```

### 4. 编写控制器

```csharp
public class UserController : BaseApiController
{
    [HttpGet, DisplayName("获取用户列表")]
    public async Task<Result> GetUserList([FromQuery] UserQueryInput input)
    {
        var users = await _userService.GetUserListAsync(input);
        return SuccessResult(users);
    }

    [AllowAnonymous, HttpPost, DisplayName("登录")]
    public async Task<Result> Login([FromBody] UserLoginInput input)
    {
        var user = await _userService.ValidateUserAsync(input.UserName, input.Password);
        if (user == null) return ErrorResult("用户名或密码错误");
        var token = await CreateToken(user);
        return SuccessResult(token);
    }
}
```

框架自动提供：
- 统一返回值格式 `{ code, type, message, result, extras, time }`
- JWT 认证 / 防重放攻击 / 全局异常处理 / API 日志
- Swagger 分组文档（default / admin / mobile / internal / open）
- 数据权限过滤 / 在线用户管理

---

## 配置示例

### 主配置 (appsettings.json)

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

### 数据库配置

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

### Redis 配置

```json
"RedisOptions": {
  "Password": "your-password",
  "Masters": "127.0.0.1:6379",
  "DefaultDatabase": 0,
  "ConnectTimeout": 10000
}
```

> 更多配置（微信、邮件、直播、语音、轻流、云存储等）请参考各子项目的 README。

---

## 统一返回值规范

所有 API 自动返回统一格式：

**成功响应** (HTTP 200)：
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

**业务异常** (HTTP 200)：
```json
{
  "code": "200",
  "type": "Fail",
  "message": "[D1002] 记录不存在",
  "result": null,
  "extras": null,
  "time": "2026-07-11 12:00:00"
}
```

**系统异常** (HTTP 500)：
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

## 版本信息

[![NuGet version](https://img.shields.io/nuget/v/LuBan.Web.Core.svg?style=flat-square)](https://www.nuget.org/packages?q=LuBan)
[![License](https://img.shields.io/badge/license-Apache%202-4EB1BA.svg)](https://www.apache.org/licenses/LICENSE-2.0.html)

- **作者**：yswenli
- **联系邮箱**：yswenli@outlook.com
- **目标框架**：.NET 8.0
- **更新频率**：持续迭代

---

## 许可证

保留所有权利
