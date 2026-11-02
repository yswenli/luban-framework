[English](README.en.md) | 中文

# LuBan.Logging

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 基于 Microsoft.Extensions.Logging 的文件日志 Provider，开箱即用，100MB/跨天自动滚动。

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## 为什么需要它？

- 不想依赖 log4net 的 XML 配置？
- 想要 Microsoft.Extensions.Logging 的标准 ILogger 抽象？
- 需要日志文件自动按大小或日期滚动，不用手动清理？
- 希望通过 DI 单例方式使用日志，而不是静态调用？

LuBan.Logging 提供自研文件日志 Provider，深度集成 Microsoft.Extensions.Logging，与 LuBan.Common.Logger 静态门面无缝衔接。

---

## 技术栈

| 组件 | 说明 |
|------|------|
| .NET 10.0 | 目标框架 |
| Microsoft.Extensions.Logging | 日志抽象与 Provider 基础 |
| System.Text.Json | 日志序列化（camelCase、忽略默认值、自定义转换器） |
| LuBan.DI | ISingleton 单例注入 |

---

## 安装

```bash
dotnet add package LuBan.Logging
```

---

## 快速预览

### ASP.NET Core Web 项目

```csharp
// Program.cs 或 ServiceHost 中
// 注册文件日志 Provider——内部会先调用 builder.ClearProviders()，
// 清掉 WebApplication.CreateBuilder 默认注册的 Console/Debug/EventSource，
// 框架自身日志（Microsoft.Hosting.Lifetime 等）不再输出到控制台
builder.Logging.AddLuBanFileLogger();

// 在 Build 之后注入到 static Logger
Logger.SetLogger(DiContainer.Instance.GetRequiredService<ILoggerFactory>());
Logger.SetSerializer(LuBanLoggingServiceExtensions.CreateLuBanSerializer());
```

### 控制台/Agent 项目

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddLuBanFileLogger());

var sp = services.BuildServiceProvider();
Logger.SetLogger(sp.GetRequiredService<ILoggerFactory>());
Logger.SetSerializer(LuBanLoggingServiceExtensions.CreateLuBanSerializer());
```

### 通过 DI 单例使用

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    // ILuBanLogger 需先显式注册（见下方「DI 集成」），类型扫描不会注册该接口
    public MyService(ILuBanLogger luBanLogger)
    {
        _logger = luBanLogger.CreateLogger<MyService>();
    }

    public void DoWork()
    {
        _logger.LogInformation("工作开始");
        // ...
        _logger.LogInformation("工作完成");
    }
}
```

---

## 功能特性

### 文件日志 Provider

| 特性 | 说明 |
|------|------|
| 按 category 路由 | `loginfo`→info.txt、`logdebug`→debug.txt、`logwarn`→warn.txt、`logerror`→error.txt、`logcall`→calllog.txt |
| 大小滚动 | 单个文件超过 100MB 自动滚动（可配置） |
| 日期滚动 | 跨天自动滚动，备份文件名含日期 |
| 备份保留 | 最多保留 5 个备份文件（可配置） |
| 编码 | UTF-8 |
| 文件锁 | FileShare.ReadWrite，多进程可读 |

### STJ 序列化器

日志内容使用 System.Text.Json 序列化，`CreateLuBanSerializer()` 直接复用 LuBan.Common 的 `SerializeUtil.Serialize(indented:false, defalutVal:false, nullValue:true, camelCase:true)`，因此输出为：

- `WriteIndented = false` — 单行 JSON，不缩进
- `PropertyNamingPolicy = CamelCase` — 驼峰命名
- `DefaultIgnoreCondition = WhenWritingDefault` — 忽略默认值（`defalutVal:false` 直接取该策略，null 与默认值都不写出，此组合下 `nullValue:true` 不起作用）
- 自定义转换器：
  - `DateTimeJsonConverter` — 日期格式 `yyyy-MM-dd HH:mm:ss.fff`
  - `ExceptionJsonConverter` — 异常对象（`ExceptionType`/`Message`/`StackTrace`/`InnerException`/`Source`，属性名由转换器写死，不随 camelCase 策略）
  - `AssemblyJsonConverter` — 程序集信息
  - `MemberInfoJsonConverter` — 成员信息

### DI 集成

`LuBanLogger` 是 `ILuBanLogger` 的实现，并实现 `ISingleton` 标记接口。注意 LuBan.DI 的 `AutoInjectAllCustomerServices()` 只按**实现类**注册（`RegisterService` 不注册接口），因此扫描后可解析 `LuBanLogger`，但解析 `ILuBanLogger` 需要显式注册一次：

```csharp
public interface ILuBanLogger
{
    ILogger CreateLogger(string categoryName);
    ILogger<T> CreateLogger<T>();
}

// 显式注册接口（类型扫描不会做这一步）
services.AddSingleton<ILuBanLogger, LuBanLogger>();
```

> 注意 Provider 只认 5 个固定 category（`loginfo`/`logdebug`/`logwarn`/`logerror`/`logcall`）：`CreateLogger<T>()` 得到的是类名 category，`FileLoggerProvider.Write` 找不到文件映射会直接丢弃，不会落盘。需要写文件请走 `Logger.Info()` 等静态门面。

---

## 配置选项

所有配置项均有默认值，无需配置即可使用。Provider 注册时会自动绑定配置节 `LuBanLoggingOptions`（`IConfiguration` 已注册时），`configure` 回调在其后执行、可覆盖配置节的值。

```csharp
builder.Logging.AddLuBanFileLogger(options =>
{
    options.Enabled = true;                 // 总开关（默认 true），false 时不写任何文件
    options.Directory = "logs";             // 日志目录（默认 logs）
    options.MaxFileSizeMB = 100;            // 单文件最大大小 MB（默认 100）
    options.MaxRollBackups = 5;             // 最大备份数（默认 5）
    options.EnableConsole = false;          // Logger 是否同时输出到控制台（默认 false）
    options.EnableDb = false;               // 是否写入数据库（默认 false，供 LuBan.Orm 的日志收集使用）
    options.Categories["logdebug"] = false; // 按 category 单独开关（默认 loginfo/logdebug/logwarn/logerror/logcall 全开）
    options.LogLimit = new LogLimitConfig   // 数据库日志容量与过期（EnableDb 时生效）
    {
        ApiLogMaxSize = 10240,              // API 日志最大条数（默认 10240）
        ApiLogExpiredSeconds = 604800,      // API 日志过期秒数（默认 604800，即 7 天）
        ErrorLogMaxSize = 1024,             // 错误日志最大条数（默认 1024）
        ErrorLogExpiredSeconds = 604800     // 错误日志过期秒数（默认 604800）
    };
});
```

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| `Enabled` | `true` | 总开关，`false` 时清空 category 白名单，不写任何日志文件 |
| `Directory` | `"logs"` | 日志输出目录，相对 `PathUtil.CurrentPath`（程序根目录，即 `AppDomain.BaseDirectory`） |
| `MaxFileSizeMB` | `100` | 单文件超过该大小即滚动 |
| `MaxRollBackups` | `5` | 滚动备份文件保留数量 |
| `EnableConsole` | `false` | 赋值给 `Logger.EnableConsoleOutput`，控制 LuBan.Common.Logger 是否控制台输出 |
| `EnableDb` | `false` | 供 LuBan.Orm 的 `LoggerCollector` 判断是否写库 |
| `Categories` | 5 类均为 `true` | category → 是否输出，键为 `loginfo`/`logdebug`/`logwarn`/`logerror`/`logcall` |
| `LogLimit` | `null`（使用时取默认值） | `LogLimitConfig`，数据库日志条数上限与过期秒数 |

---

## 与 LuBan.Common.Logger 的关系

LuBan.Common.Logger 是静态门面类，提供 `Logger.Info()`、`Logger.Error()` 等静态方法。它内部委托 5 个 `ILogger` 实例（按 category name 区分），通过 `SetLogger(ILoggerFactory)` 在启动时注入。

LuBan.Logging 负责创建 `ILoggerFactory` 并注册文件日志 Provider，然后在启动时调用 `Logger.SetLogger()` 和 `Logger.SetSerializer()` 完成注入。

```
LuBan.Logging（Provider 注册 + 序列化器）
       ↓ SetLogger/SetSerializer
LuBan.Common.Logger（静态门面，按 category 分发到 5 个 ILogger）
       ↓ ILogger.LogInformation
FileLoggerProvider → RollingFileWriter → logs/info.txt
```

---

## 日志文件输出示例

`logs/info.txt`（`WriteIndented = false`，单行输出）：

```json
{"created":"2026-07-31 14:30:00.000","serviceName":"MyService","level":0,"description":"订单模块\t订单 10086 创建成功"}
```

`logs/error.txt`：

```json
{"created":"2026-07-31 14:30:01.000","serviceName":"MyService","level":1,"description":"支付模块","exception":{"ExceptionType":"System.Net.Http.HttpRequestException","Message":"连接超时","StackTrace":"   at System.Net.Http...","Source":"MyService"}}
```

---

## 许可证

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
