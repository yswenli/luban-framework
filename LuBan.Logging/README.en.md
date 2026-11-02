[中文](README.md) | English

# LuBan.Logging

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> A file logging provider built on Microsoft.Extensions.Logging — works out of the box, rolls at 100MB or across days.

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## Why Do You Need It?

- Tired of log4net's XML configuration?
- Want the standard ILogger abstraction from Microsoft.Extensions.Logging?
- Need log files to roll by size or by date without manual cleanup?
- Prefer to use logging through a DI singleton instead of static calls?

LuBan.Logging ships a self-built file logging provider that integrates deeply with Microsoft.Extensions.Logging and dovetails with the LuBan.Common.Logger static facade.

---

## Tech Stack

| Component | Description |
|-----------|-------------|
| .NET 10.0 | Target framework |
| Microsoft.Extensions.Logging | Logging abstraction and provider base |
| System.Text.Json | Log serialization (camelCase, ignore defaults, custom converters) |
| LuBan.DI | ISingleton singleton injection |

---

## Installation

```bash
dotnet add package LuBan.Logging
```

---

## Quick Preview

### ASP.NET Core Web Project

```csharp
// Program.cs or ServiceHost
// Register the file logging provider — internally it calls builder.ClearProviders() first,
// removing the Console/Debug/EventSource providers registered by WebApplication.CreateBuilder,
// so framework logs (Microsoft.Hosting.Lifetime, etc.) no longer go to the console
builder.Logging.AddLuBanFileLogger();

// Inject into the static Logger after Build
Logger.SetLogger(DiContainer.Instance.GetRequiredService<ILoggerFactory>());
Logger.SetSerializer(LuBanLoggingServiceExtensions.CreateLuBanSerializer());
```

### Console/Agent Project

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddLuBanFileLogger());

var sp = services.BuildServiceProvider();
Logger.SetLogger(sp.GetRequiredService<ILoggerFactory>());
Logger.SetSerializer(LuBanLoggingServiceExtensions.CreateLuBanSerializer());
```

### Use Through a DI Singleton

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    // ILuBanLogger must be registered explicitly first (see "DI Integration" below);
    // type scanning does not register that interface
    public MyService(ILuBanLogger luBanLogger)
    {
        _logger = luBanLogger.CreateLogger<MyService>();
    }

    public void DoWork()
    {
        _logger.LogInformation("Work started");
        // ...
        _logger.LogInformation("Work finished");
    }
}
```

---

## Features

### File Logging Provider

| Feature | Description |
|---------|-------------|
| Routing by category | `loginfo`→info.txt, `logdebug`→debug.txt, `logwarn`→warn.txt, `logerror`→error.txt, `logcall`→calllog.txt |
| Size rolling | Rolls automatically once a file exceeds 100MB (configurable) |
| Date rolling | Rolls automatically across days, backup file names carry the date |
| Backup retention | Keeps at most 5 backup files (configurable) |
| Encoding | UTF-8 |
| File locking | FileShare.ReadWrite, readable by multiple processes |

### STJ Serializer

Log content is serialized with System.Text.Json; `CreateLuBanSerializer()` reuses LuBan.Common's `SerializeUtil.Serialize(indented:false, defalutVal:false, nullValue:true, camelCase:true)` directly, so the output is:

- `WriteIndented = false` — single-line JSON, no indentation
- `PropertyNamingPolicy = CamelCase` — camelCase naming
- `DefaultIgnoreCondition = WhenWritingDefault` — ignore default values (`defalutVal:false` selects this condition directly, so neither null nor default values are written; `nullValue:true` has no effect in this combination)
- Custom converters:
  - `DateTimeJsonConverter` — date format `yyyy-MM-dd HH:mm:ss.fff`
  - `ExceptionJsonConverter` — exception object (`ExceptionType`/`Message`/`StackTrace`/`InnerException`/`Source`; the property names are hard-coded in the converter and do not follow the camelCase policy)
  - `AssemblyJsonConverter` — assembly information
  - `MemberInfoJsonConverter` — member information

### DI Integration

`LuBanLogger` implements `ILuBanLogger` and also implements the `ISingleton` marker interface. Note that LuBan.DI's `AutoInjectAllCustomerServices()` registers **implementation classes only** (`RegisterService` does not register the interface), so after scanning, `LuBanLogger` is resolvable but `ILuBanLogger` requires one explicit registration:

```csharp
public interface ILuBanLogger
{
    ILogger CreateLogger(string categoryName);
    ILogger<T> CreateLogger<T>();
}

// Register the interface explicitly (type scanning does not do this)
services.AddSingleton<ILuBanLogger, LuBanLogger>();
```

> Note that the provider only recognizes the 5 fixed categories (`loginfo`/`logdebug`/`logwarn`/`logerror`/`logcall`): `CreateLogger<T>()` uses the type name as the category, and `FileLoggerProvider.Write` drops it when no file mapping exists, so nothing is written to disk. Use the static facade such as `Logger.Info()` when you need file output.

---

## Configuration Options

Every option has a default value, so no configuration is required. When the provider is registered it binds the `LuBanLoggingOptions` configuration section (when `IConfiguration` is registered); the `configure` callback runs afterwards and can override values from that section.

```csharp
builder.Logging.AddLuBanFileLogger(options =>
{
    options.Enabled = true;                 // Master switch (default true); false writes no files at all
    options.Directory = "logs";             // Log directory (default logs)
    options.MaxFileSizeMB = 100;            // Max size per file in MB (default 100)
    options.MaxRollBackups = 5;             // Max backup count (default 5)
    options.EnableConsole = false;          // Whether Logger also writes to the console (default false)
    options.EnableDb = false;               // Whether to write to the database (default false; used by LuBan.Orm log collection)
    options.Categories["logdebug"] = false; // Per-category switch (all of loginfo/logdebug/logwarn/logerror/logcall on by default)
    options.LogLimit = new LogLimitConfig   // Database log capacity and expiry (effective when EnableDb is on)
    {
        ApiLogMaxSize = 10240,              // Max API log rows (default 10240)
        ApiLogExpiredSeconds = 604800,      // API log expiry in seconds (default 604800, i.e. 7 days)
        ErrorLogMaxSize = 1024,             // Max error log rows (default 1024)
        ErrorLogExpiredSeconds = 604800     // Error log expiry in seconds (default 604800)
    };
});
```

| Option | Default | Description |
|--------|---------|-------------|
| `Enabled` | `true` | Master switch; when `false` the category whitelist is emptied and no log file is written |
| `Directory` | `"logs"` | Log output directory, relative to `PathUtil.CurrentPath` (the application root, i.e. `AppDomain.BaseDirectory`) |
| `MaxFileSizeMB` | `100` | Roll once a file exceeds this size |
| `MaxRollBackups` | `5` | Number of rolled backup files to keep |
| `EnableConsole` | `false` | Assigned to `Logger.EnableConsoleOutput`, controlling whether LuBan.Common.Logger writes to the console |
| `EnableDb` | `false` | Lets LuBan.Orm's `LoggerCollector` decide whether to write to the database |
| `Categories` | All 5 `true` | category → enabled; keys are `loginfo`/`logdebug`/`logwarn`/`logerror`/`logcall` |
| `LogLimit` | `null` (defaults applied on use) | `LogLimitConfig` with database log row limits and expiry seconds |

---

## Relationship with LuBan.Common.Logger

LuBan.Common.Logger is a static facade exposing `Logger.Info()`, `Logger.Error()` and other static methods. Internally it delegates to 5 `ILogger` instances (one per category name), injected at startup via `SetLogger(ILoggerFactory)`.

LuBan.Logging creates the `ILoggerFactory` and registers the file logging provider, then calls `Logger.SetLogger()` and `Logger.SetSerializer()` at startup to complete the injection.

```
LuBan.Logging (provider registration + serializer)
       ↓ SetLogger/SetSerializer
LuBan.Common.Logger (static facade, dispatches to 5 ILoggers by category)
       ↓ ILogger.LogInformation
FileLoggerProvider → RollingFileWriter → logs/info.txt
```

---

## Log File Output Examples

`logs/info.txt` (`WriteIndented = false`, single-line output):

```json
{"created":"2026-07-31 14:30:00.000","serviceName":"MyService","level":0,"description":"Orders\tOrder 10086 created"}
```

`logs/error.txt`:

```json
{"created":"2026-07-31 14:30:01.000","serviceName":"MyService","level":1,"description":"Payment module","exception":{"ExceptionType":"System.Net.Http.HttpRequestException","Message":"Connection timed out","StackTrace":"   at System.Net.Http...","Source":"MyService"}}
```

---

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
