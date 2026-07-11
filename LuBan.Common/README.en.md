[中文](README.md) | English

# LuBan.Common

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"The Swiss Army knife for enterprise development — ready out of the box, write 80% less boilerplate."**

Are you still struggling with these chores?

- Writing Excel import/export code for every project?
- HTTP requests wrapped in layers when you just want a simple API call?
- Caching, logging, files, strings, dates, IPs, QR codes... reinventing the wheel for every project?

If so, **LuBan.Common** is built for you.

It's a general-purpose utility class library based on **.NET 8**, packaging the most frequent and tedious enterprise development needs into a handy toolkit: from Excel, HTTP, file IO, caching, logging, to SMS, imaging, QR codes, IP geolocation, pinyin conversion — it covers everything. It's not a framework — it's your **"development power-up"** — just reference it and get to work.

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Threading](../LuBan.Threading/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Redis](../LuBan.Redis/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md)

---

## Why Choose LuBan.Common?

| Pain Point | LuBan.Common's Solution |
|---|---|
| Excel/CSV handling is tedious | `ExcelUtil`, `CsvUtil` one-line import/export |
| HTTP call wrapping is complex | `HttpClientProxy` + string extensions, GET/POST as natural as breathing |
| Cache has no expiry strategy | `MemoryCache<T>` with built-in timed cleanup |
| Logging configuration is endless | `Logger` one-line printing, auto-handled |
| File read/write and path handling always go wrong | `FileUtil`, `PathUtil` wrap common operations as static methods |
| Need SMS, QR codes, IP geolocation | `SmsSender`, `ImageUtil`, `IPToRegion` ready to use |

---

## One-Minute Preview

```csharp
using LuBan.Common;
using LuBan.Common.Data;
using LuBan.Common.IO;

// Excel instant read
using var fs = File.OpenRead("orders.xlsx");
var dt = ExcelUtil.ImportFromStream(fs, v2003: false, sheetName: "sheet1");

// HTTP instant call
var proxy = new HttpClientProxy();
var json = await proxy.GetAsync("https://api.example.com/data");

// Cache instant store
var cache = MemoryCache<string>.GetInstance();
cache.Set("user:1001", "active", TimeSpan.FromMinutes(10));

// File instant write
FileUtil.WriteString("log.txt", "Saved successfully");

// Log instant print
Logger.Info("Order Module", "Order created successfully");
```

> No fluff, just productivity.

---

## Tech Stack

- **Target Framework**: .NET 8.0
- **Project Type**: Class Library
- **NuGet Package**: `LuBan.Common`

---

## Installation

```bash
dotnet add package LuBan.Common
```

Then `using` the appropriate namespace wherever you need it and get to work.

---

## Toolbox Overview

### String & Data Conversion

`StringUtil`, `StringPlus`, `Base64Util`, `HexConverter`, `NumberUtil`, `DecimalUtil`, `GuidUtil`, `CodeUtil`, `PasswordUtil`, `ModelUtil`, `DynamicUtil`, `AnonymousTypeUtil`

> String processing, unit conversion, encoding/decoding, password validation, object mapping... if you can think of a conversion, it's probably here.

### Date & Time

`DateTimeUtil` + `Calendar.ChineseCalendar`

> Formatting, workday calculation, Chinese weekdays, lunar calendar, solar terms, zodiac signs — all covered.

### Collections & Data Structures

`CollectionUtil`, `LinqExtention`, `SmartList`, `PagedList`, `PagedDictionary`, `ThreadSafeList`, `Batcher`, `DataSplitter`

> Pagination, thread safety, batching, splitting — common collection pain points solved at once.

### Excel / CSV

`Data.ExcelUtil`, `Data.CsvUtil`, `DataTableUtil`

> Excel import/export, streaming read, directly produces `DataTable`.

### Caching

`MemoryCache<T>`, `IServiceCache`, `LocalCacheUtil`

> `MemoryCache<T>` has built-in expiry cleanup with generics support; `LocalCacheUtil` persists to local files.

Note: `MemoryCache<T>`, `IServiceCache`, `Logger` are in the `System` namespace.

### HTTP Communication

`HttpClientProxy`, `Http.HttpExtension`, `Http.FormUpload`, `Http.FileParameter`

> Supports GET / POST / upload / custom headers, and strings can directly `.HttpGetAsync()`, `.HttpPostAsync()`.

### File IO

`IO.FileUtil`, `IO.PathUtil`, `IO.FileTypeUtil`, `IO.TempFile`, `IO.PersistenceFile`

> File read/write, path handling, temporary files, serialized storage — common operations in one line.

### Logging

`Logger` (namespace `System`), `LogInfo`, `ApiLogInfo`

> Static calls: `Logger.Info`, `Logger.Error`, `Logger.Debug` — absurdly convenient.

### System & Process

`ComputerUtil`, `ProcessUtil`, `ShellUtil`, `EnvironmentArgsUtil`, `RuntimeUtil`, `ServiceUtil`, `CallContext`

> Machine info, process management, Shell execution, environment parameters, runtime info — system-level operations handled with ease.

### Reflection & High Performance

`ReflectionUtil`, `FastILUtil`, `FastCopy`, `EnumUtil`, `BaseSingleInstance<T>`

> Fast reflection, easy enum reading, singleton base class available on demand.

### Network & IP

`IPUtil`, `LANUtil`, `IPToRegion.IPRegion`

> IP geolocation based on ip2region, local offline resolution, no internet required.

### Image & QR Code

`ImageUtil`, QR code generation and recognition

> Image processing, QR code generation and recognition, directly embedded in business code.

### SMS Service

`Sms.SmsSender`, `Sms.SmsOption`

> SMS sending abstraction, configuration-based integration.

### Security & Anti-Replay

`AraReplayAttacksUtil`, `AntiReplayAttacks.AraInfo`

> API security, anti-replay attack protection.

### Configuration & Constants

`ConfigUtil`, `NacosConfigUtil`, `Consts.*`

> Configuration files, configuration center, project constants — unified management.

### Error Handling

`Errors.FriendlyException`, `Errors.EnumErrorCode`

> Standardized business exceptions, frontend gets error codes, backend throws friendly messages.

### Event Bus Interfaces

`EventBus.IEventBus`, `IEventData`, `IEventHandler<T>`

> Define event bus contracts, concrete implementations injected by upper-layer framework.

### Other Treasures

`SerializeUtil`, `StreamUtil`, `ZipUtil`, `Tracer`, `TryUtil`, `TokenUtil`, `EmailUtil`, `ConsoleUtil`, `NPinyin.Pinyin`

> Serialization, streams, compression, tracing, tokens, email, console, pinyin conversion... quite a few surprises hidden here.

---

## Practical Examples

### Excel Import: From Chaos to Clarity

```csharp
using LuBan.Common.Data;
using System.Data;

DataTable? dt;
using (var fs = File.OpenRead("orders.xlsx"))
{
    dt = ExcelUtil.ImportFromStream(fs, v2003: false, sheetName: "sheet1", hasHeader: true);
}
```

No boilerplate, just one core call.

### HTTP Requests: Write Remote Calls Like Local Methods

```csharp
using LuBan.Common;
using LuBan.Common.Http;

var client = new HttpClientProxy();

// Simple GET
string html = await client.GetAsync("https://api.example.com/data");

// Generic deserialization
var user = await client.GetAsync<UserDto>("https://api.example.com/user/1");

// POST object, auto-convert to JSON
var result = await client.PostAsync("https://api.example.com/order", new { Id = 1, Name = "VIP" });

// Even lazier: string extensions
string data = await "https://api.example.com/data".HttpGetAsync();
```

### Caching: Built-in Expiry, No GC Worries

```csharp
using System;

var cache = MemoryCache<string>.GetInstance();

cache.Set("session:123", "user-data", TimeSpan.FromMinutes(30));

var value = cache.Get("session:123");
```

### File Operations: Read, Write, Serialize, Delete — All in One

```csharp
using LuBan.Common.IO;

// Read text
string content = FileUtil.ReadString("readme.txt");

// Write text
FileUtil.WriteString("output.txt", "Hello LuBan!");

// Save object to file
FileUtil.Write("config.json", new { Host = "localhost", Port = 5000 });

// Read object
var config = FileUtil.Read<Config>("config.json");

// Does file exist?
bool exists = FileUtil.Exists("output.txt");
```

### Logging: Three Lines Config, One Line Print

```csharp
using System;

Logger.Info("Order Module", "Order 10086 created successfully");
Logger.Error("Payment Module", exception, "Payment callback handling failed");
Logger.Debug("Debug", $"Elapsed {elapsedMs}ms");
```

> Make sure a logging configuration file exists in the project root before use.

### Date & Time: Say Goodbye to Manual Time Logic

```csharp
using LuBan.Common;

var now = DateTimeUtil.Now;
string weekday = DateTimeUtil.GetChineseWeekDay(now);     // Thursday
int workDays = DateTimeUtil.GetWorkDays(now, now.AddDays(10));
```

### Business Exceptions: Make Errors Elegant Too

```csharp
using LuBan.Common.Errors;

try
{
    throw new FriendlyException(EnumErrorCode.InvalidParameter, "Invalid phone number format");
}
catch (FriendlyException ex)
{
    Console.WriteLine($"[{ex.ErrorCode}] {ex.Message}");
}
```

### Event Bus: Interface First, Implementation Flexible

```csharp
using LuBan.Common.EventBus;

public class UserCreatedEvent : IEventData
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class UserCreatedHandler : IEventHandler<UserCreatedEvent>
{
    public Task HandleAsync(UserCreatedEvent e)
    {
        Console.WriteLine($"Welcome new user: {e.UserName}");
        return Task.CompletedTask;
    }
}

// The concrete IEventBus implementation is injected by the upper-layer framework or business project
```

---

## Usage Tips

1. **Don't mix up namespaces**: `LuBan.Common` is the root namespace, but specific utilities are spread across `LuBan.Common.Data`, `LuBan.Common.IO`, `LuBan.Common.Http`, `LuBan.Common.Sms`, `LuBan.Common.Errors`, `LuBan.Common.EventBus`, `LuBan.Common.Calendar`, `LuBan.Common.Consts`, `LuBan.Common.IPToRegion`, `LuBan.Common.AntiReplayAttacks`, `LuBan.Common.NPinyin` and other sub-namespaces; `MemoryCache<T>`, `IServiceCache`, `Logger` are in `System`.
2. **Configure logging first**: `Logger` depends on a logging configuration file, otherwise output targets may not be found.
3. **Choose thread safety wisely**: For multi-threaded scenarios, prefer `ThreadSafeList`, concurrent dictionaries, or `MemoryCache<T>`.
4. **Set cache expiry**: Cache is not infinite memory — set reasonable expiry times based on business needs.
5. **Use Nacos for configuration**: Use `NacosConfigUtil` for frequently changing configurations to avoid redeploying for config changes.

---

## License

MIT License

---

**LuBan.Common** — Making .NET enterprise development simpler, faster, and more enjoyable.

> If you also find reinventing the wheel boring, add it to your toolbox.
