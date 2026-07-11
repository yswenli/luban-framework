[English](README.en.md) | 中文

# LuBan.Web.Core

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **一行代码启动企业级 API 服务——JWT、Swagger、多租户、审批流、实时通信全内置。**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Redis](../LuBan.Redis/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md) | [LuBan.Wechat](../LuBan.Wechat/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md)

---

## 为什么选择 LuBan.Web.Core？

你是否厌倦了每个新项目都要重复配置 JWT 认证、Swagger 分组、全局异常处理、统一响应格式、多租户数据库路由？  
LuBan.Web.Core 把这些"脏活累活"全部封装为一行启动代码，让你专注于业务逻辑。

---

## 快速预览

```csharp
// Program.cs — 只需两行即可启动完整 API 服务
WebApp.RunWebHost(args);

// 或异步方式
await WebApp.RunWebHostAsync(args);
```

就这么简单。框架会自动读取 `appsettings.json` 中的 `HostingOptions` 配置，完成 DI 注册、中间件管道搭建、数据库初始化、后台任务启动等全部工作。

---

## 技术栈 & 依赖

| 类别 | 组件 | 说明 |
|------|------|------|
| 运行时 | .NET 8.0 | ASP.NET Core 最小宿主 |
| 缓存 | LuBan.Redis | Redis 分布式缓存 & 锁 |
| 系统服务 | Hosting.Systemd / WindowsServices | 支持注册为系统服务 |
| 审批流 | LuBan.ApprovalFlow | 内置审批流引擎 |
| 报表导出 | LuBan.Reporting | Excel/CSV 导出 |
| 云存储 | LuBan.CloudStorage | 对象存储集成 |
| 日志 | LuBan.LogLib | 统一日志库 |

---

## 安装

```bash
dotnet add package LuBan.Web.Core
```

---

## 配置

在 `appsettings.json` 中添加核心配置：

```json
{
  "HostingOptions": {
    "ServiceName": "我的API服务",
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

## 功能全景

### 启动入口

| API | 说明 |
|-----|------|
| `WebApp.RunWebHost(args)` | 同步启动 Web 服务 |
| `WebApp.RunWebHostAsync(args)` | 异步启动 Web 服务 |
| `WebApp.HostingOptions` | 获取全局配置 |
| `WebApp.HttpContext` | 静态获取当前请求上下文 |
| `WebApp.User` | 静态获取当前 JWT 用户 |
| `WebApp.ServiceCache` | 获取缓存服务实例 |
| `WebApp.RootPath` | 站点根目录路径 |
| `WebApp.GetPhysicalPath(...)` | 获取物理文件路径 |

### 控制器继承体系

框架提供 6 种基类控制器，覆盖不同接入场景：

```
BaseApiController       — JWT 认证，路由 api/，Swagger 分组 default
├── BaseAdminController — 管理端，路由 api/admin/，分组 admin，需角色授权
├── BaseMobileController— 移动端，路由 api/mobile/，分组 mobile
├── BaseInternalController — 内部接口，路由 api/internal/，分组 internal
└── BaseOpenController  — 开放接口，路由 api/open/，分组 open，匿名 + 防重放 + OpenAPI 令牌

BaseWebController       — Cookie 认证，管理后台 Web 页面
```

### 认证 & 授权

- **JWT 认证**：`JwtConfigureService` 自动配置，`SessionUser` 获取当前用户
- **角色访问控制**：`[ForbiddenAccess]` 限制管理端、`[AllowAccess]` 放行移动端/内部接口
- **Open API 认证**：`[OpenApiAccess]` 基于 refresh/access token 的开放接口鉴权
- **Cookie Web 认证**：`[WebLoginAuth]` 用于管理后台页面

### 统一响应格式

所有 API 返回统一结构（由 `ApiResultConvertionAttribute` 自动包装）：

```json
{
  "code": 200,
  "type": "success",
  "message": "操作成功",
  "result": { ... },
  "extras": {},
  "time": 1720000000
}
```

### 核心中间件 & 过滤器

| 组件 | 说明 |
|------|------|
| `ErrorHandlingMiddleware` | 全局异常捕获，统一错误响应 |
| `OnlineUserMiddleware` | 在线用户统计与管理 |
| `AraParameterFilterAttribute` | 防重放攻击（时间戳 + 签名） |
| `DistributedLockAttribute` | Redis 分布式锁，防止并发重复操作 |
| `OutputCacheAttribute` | 接口输出缓存 |
| `CacheableAttribute` | 声明式缓存 |
| `CacheClearAttribute` | 缓存清除 |
| `IPWhiteListFilterAttribute` | IP 白名单过滤 |
| `DataScopePermissionFilter` | 数据权限（本人/部门/组织/全部） |
| `InputArgsValidateActionFilter` | 入参自动校验 |

### 多租户数据库

`DbRepository<TEntity>` 自动从 JWT 中解析租户 ID，路由到对应数据库：

```csharp
// 自动根据当前用户租户选择数据库连接
var repo = new DbRepository<SysUser>();
var users = await repo.GetListAsync();

// 切换实体仓储
var userRepo = repo.Change<SysRole>();
```

### Swagger 分组文档

自动生成 5 组 API 文档：`default`、`admin`、`mobile`、`internal`、`open`，支持 Markdown 导出和 JS SDK 生成。

### SignalR 实时通信

内置 `CommonHub`，支持 `IHubClient` / `IHubServer` 接口和 `SimpleClient` 快速接入。

### 其他能力

- **后台任务**：`JobsController` + `JobServiceLoader` 管理定时任务
- **健康检查**：`HealthCheckService` 支持企业微信机器人告警
- **SSE 流式输出**：`SseStream` 服务端推送
- **文件上传下载**：`UploadFileUtil` / `DownloadFileUtil` / `ExtraFileController`
- **图形验证码**：内置验证码生成
- **API 压测**：`CommonController.StressTest`
- **系统服务部署**：支持 Windows Service 和 systemd

---

## 代码示例

### 自定义业务控制器

```csharp
// 管理端接口
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

// 移动端接口
public class OrderController : BaseMobileController
{
    [HttpPost]
    [DistributedLock("order_{userId}", Seconds = 5)]
    public async Task<ApiResult> CreateOrder([FromBody] OrderInput input)
    {
        // 业务逻辑
        return Success();
    }
}

// 开放接口（匿名访问 + 防重放 + OpenAPI 令牌）
public class DataSyncController : BaseOpenController
{
    [HttpPost]
    public async Task<ApiResult> SyncData([FromBody] SyncInput input)
    {
        return Success();
    }
}
```

### 注册启动事件

```csharp
WebApp.OnStarted += () =>
{
    Console.WriteLine("服务已启动，执行自定义初始化...");
};

WebApp.RunWebHost(args);
```

### 命令行参数

```bash
# 指定监听地址
dotnet run --urls "http://0.0.0.0:5000;http://0.0.0.0:5001"

# 指定 FFmpeg 路径（启用视频缩略图）
dotnet run --ffmpeg "c:/bin/ffmpeg.exe"

# 指定运行环境
dotnet run --environment Production
```

---

## 小贴士

1. **启动前检查**：确保 `appsettings.json` 中 `HostingOptions` 配置完整，特别是 `ServiceName`、`Domain`、`AppOptions.Urls`
2. **多租户**：JWT 中需包含 `TenantId` Claim，否则默认使用主库
3. **开放接口**：`BaseOpenController` 同时要求 `[AllowAnonymous]` + `[OpenApiAccess]` + `[AraParameterFilter]`，缺一不可
4. **后台任务**：通过 `BackgroundJobNames` 可精确控制启用哪些后台任务，空列表则全部启用
5. **审批流联动**：当 `ApprovalFlowOptions.AutoApproval = true` 时，`FlowEngine` 会随服务自动启停

---

## 许可证

Copyright (c) yswenli. All Rights Reserved.
