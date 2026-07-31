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
| .NET 8.0 | 目标框架 |
| Microsoft.Extensions.Logging | 日志抽象与 Provider 基础 |
| System.Text.Json | 日志序列化（camelCase + indented + 自定义转换器） |
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
builder.Logging.AddLuBanFileLogger();  // 注册文件日志 Provider

// 在 Build 之后注入到 static Logger
Logger.SetLogger(ServiceProviderUtil.GetRequiredService<ILoggerFactory>());
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

    public MyService(ILuBanLogger loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MyService>();
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

日志内容使用 System.Text.Json 序列化，输出格式与原 `SerializeUtil.Serialize(indented:true, defalutVal:false, nullValue:true, camelCase:true)` 一致：

- `WriteIndented = true` — 缩进输出
- `PropertyNamingPolicy = CamelCase` — 驼峰命名
- `DefaultIgnoreCondition = WhenWritingDefault` — 忽略默认值和 null
- 自定义转换器：
  - `LuBanDateTimeConverter` — 日期格式 `yyyy-MM-dd HH:mm:ss.fff`
  - `ExceptionJsonConverter` — 异常对象（exceptionType/message/stackTrace/innerException/source）
  - `AssemblyJsonConverter` — 程序集信息
  - `MemberInfoJsonConverter` — 成员信息

### DI 集成

`ILuBanLogger` 实现 `ISingleton` 接口，通过 LuBan.DI 自动注册为单例：

```csharp
public interface ILuBanLogger
{
    ILogger CreateLogger(string categoryName);
    ILogger<T> CreateLogger<T>();
}
```

---

## 配置选项

所有配置项均有默认值，无需配置即可使用：

```csharp
builder.Logging.AddLuBanFileLogger(options =>
{
    options.Directory = "logs";        // 日志目录（默认 logs）
    options.MaxFileSizeMB = 100;       // 单文件最大大小 MB（默认 100）
    options.MaxRollBackups = 5;        // 最大备份数（默认 5）
    options.IncludeScopes = false;     // 是否包含作用域（默认 false）
});
```

---

## 与 LuBan.Common.Logger 的关系

LuBan.Common.Logger 是静态门面类，提供 `Logger.Info()`、`Logger.Error()` 等静态方法。它内部委托 5 个 `ILogger` 实例（按 category name 区分），通过 `SetLogger(ILoggerFactory)` 在启动时注入。

LuBan.Logging 负责创建 `ILoggerFactory` 并注册文件日志 Provider，然后在启动时调用 `Logger.SetLogger()` 和 `Logger.SetSerializer()` 完成注入。

```
LuBan.Logging（Provider 注册 + 序列化器）
       ↓ SetLogger/SetSerializer
LuBan.Common.Logger（静态门面，265 处调用零改动）
       ↓ ILogger.LogInformation
FileLoggerProvider → RollingFileWriter → logs/info.txt
```

---

## 日志文件输出示例

`logs/info.txt`：

```json
{
  "created": "2026-07-31 14:30:00.000",
  "serviceName": "MyService",
  "level": 0,
  "description": "订单模块\t订单 10086 创建成功"
}
```

`logs/error.txt`：

```json
{
  "created": "2026-07-31 14:30:01.000",
  "serviceName": "MyService",
  "level": 1,
  "description": "支付模块",
  "exception": {
    "exceptionType": "System.Net.Http.HttpRequestException",
    "message": "连接超时",
    "stackTrace": "   at System.Net.Http...",
    "source": "MyService"
  }
}
```

---

## 许可证

MIT
