[English](README.en.md) | 中文

# LuBan.Common

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"企业级开发的瑞士军刀，开箱即用，少写 80% 的样板代码。"**

你是否还在为这些琐事头疼？

- Excel 导入导出又要手写一遍？
- HTTP 请求想简单调个接口却层层封装？
- 缓存、日志、文件、字符串、日期、IP、二维码……每个项目都要重新造一遍轮子？

如果是，那 **LuBan.Common** 就是为你准备的。

它是一个基于 **.NET 8** 的通用工具类库，把企业开发里最高频、最琐碎的需求打包成一套顺手工具：从 Excel、HTTP、文件 IO、缓存、日志，到短信、图像、二维码、IP 归属地、拼音转换，应有尽有。它不是框架，而是你的 **"开发外挂"** —— 直接引用，马上开工。

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Threading](../LuBan.Threading/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Redis](../LuBan.Redis/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md)

---

## 为什么选择 LuBan.Common？

| 痛点 | LuBan.Common 的解法 |
|---|---|
| Excel/CSV 处理繁琐 | `ExcelUtil`、`CsvUtil` 一行导入导出 |
| HTTP 调用封装复杂 | `HttpClientProxy` + 字符串扩展， GET/POST 像呼吸一样自然 |
| 缓存没有过期策略 | `MemoryCache<T>` 自带定时清理 |
| 日志配置没完没了 | `Logger` 一句打印，自动接管 |
| 文件读写、路径处理总写错 | `FileUtil`、`PathUtil` 把常见操作封成静态方法 |
| 需要短信、二维码、IP 定位 | `SmsSender`、`ImageUtil`、`IPToRegion` 直接可用 |

---

## 一分钟预览

```csharp
using LuBan.Common;
using LuBan.Common.Data;
using LuBan.Common.IO;

// Excel 秒读
using var fs = File.OpenRead("orders.xlsx");
var dt = ExcelUtil.ImportFromStream(fs, v2003: false, sheetName: "sheet1");

// HTTP 秒调
var proxy = new HttpClientProxy();
var json = await proxy.GetAsync("https://api.example.com/data");

// 缓存秒存
var cache = MemoryCache<string>.GetInstance();
cache.Set("user:1001", "active", TimeSpan.FromMinutes(10));

// 文件秒写
FileUtil.WriteString("log.txt", "保存成功");

// 日志秒打
Logger.Info("订单模块", "订单创建成功");
```

> 没有废话，只有生产力。

---

## 技术栈

- **目标框架**：.NET 8.0
- **项目类型**：类库（Class Library）
- **NuGet 包**：`LuBan.Common`

---

## 安装

```bash
dotnet add package LuBan.Common
```

然后在你需要的地方 `using` 对应的命名空间即可开工。

---

## 工具箱全览

### 字符串与数据转换

`StringUtil`、`StringPlus`、`Base64Util`、`HexConverter`、`NumberUtil`、`DecimalUtil`、`GuidUtil`、`CodeUtil`、`PasswordUtil`、`ModelUtil`、`DynamicUtil`、`AnonymousTypeUtil`

> 字符串处理、单位换算、编码解码、密码校验、对象映射……能想到的转换，基本都有。

### 日期时间

`DateTimeUtil` + `Calendar.ChineseCalendar`

> 格式化、工作日计算、中文星期、农历、节气、生肖，全搞定。

### 集合与数据结构

`CollectionUtil`、`LinqExtention`、`SmartList`、`PagedList`、`PagedDictionary`、`ThreadSafeList`、`Batcher`、`DataSplitter`

> 分页、线程安全、批量、拆分，集合操作的常见痛点一次解决。

### Excel / CSV

`Data.ExcelUtil`、`Data.CsvUtil`、`DataTableUtil`

> Excel 导入导出，流式读取，直接出 `DataTable`。

### 缓存

`MemoryCache<T>`、`IServiceCache`、`LocalCacheUtil`

> `MemoryCache<T>` 自带过期清理，支持泛型；`LocalCacheUtil` 持久化到本地文件。

注意：`MemoryCache<T>`、`IServiceCache`、`Logger` 位于 `System` 命名空间。

### HTTP 通信

`HttpClientProxy`、`Http.HttpExtension`、`Http.FormUpload`、`Http.FileParameter`

> 支持 GET / POST / 上传 / 自定义 Header，字符串还能直接 `.HttpGetAsync()`、`.HttpPostAsync()`。

### 文件 IO

`IO.FileUtil`、`IO.PathUtil`、`IO.FileTypeUtil`、`IO.TempFile`、`IO.PersistenceFile`

> 文件读写、路径处理、临时文件、序列化存储，常见操作一行代码。

### 日志

`Logger`（命名空间 `System`）、`LogInfo`、`ApiLogInfo`

> 静态调用：`Logger.Info`、`Logger.Error`、`Logger.Debug`，省事到离谱。

### 系统与进程

`ComputerUtil`、`ProcessUtil`、`ShellUtil`、`EnvironmentArgsUtil`、`RuntimeUtil`、`ServiceUtil`、`CallContext`

> 机器信息、进程管理、Shell 执行、环境参数、运行时信息，系统级操作也不在话下。

### 反射与高性能

`ReflectionUtil`、`FastILUtil`、`FastCopy`、`EnumUtil`、`BaseSingleInstance<T>`

> 反射不卡、枚举好读、单例基类随拿随用。

### 网络与 IP

`IPUtil`、`LANUtil`、`IPToRegion.IPRegion`

> IP 归属地查询基于 ip2region，本地离线解析，无需联网。

### 图像与二维码

`ImageUtil`、二维码生成与识别

> 图片处理、二维码生成与识别，直接嵌入业务代码。

### 短信服务

`Sms.SmsSender`、`Sms.SmsOption`

> 短信发送抽象，配置化接入。

### 安全与防重放

`AraReplayAttacksUtil`、`AntiReplayAttacks.AraInfo`

> 接口安全，防重放攻击。

### 配置与常量

`ConfigUtil`、`NacosConfigUtil`、`Consts.*`

> 配置文件、配置中心、项目常量，统一管理。

### 错误处理

`Errors.FriendlyException`、`Errors.EnumErrorCode`

> 业务异常规范化，前端拿到错误码，后端抛出友好提示。

### 事件总线接口

`EventBus.IEventBus`、`IEventData`、`IEventHandler<T>`

> 定义事件总线契约，具体实现可由上层框架注入。

### 其他宝藏

`SerializeUtil`、`StreamUtil`、`ZipUtil`、`Tracer`、`TryUtil`、`TokenUtil`、`EmailUtil`、`ConsoleUtil`、`NPinyin.Pinyin`

> 序列化、流、压缩、追踪、Token、邮件、控制台、拼音转换……这里藏了不少小惊喜。

---

## 实战示例

### Excel 导入：从混乱到清晰

```csharp
using LuBan.Common.Data;
using System.Data;

DataTable? dt;
using (var fs = File.OpenRead("orders.xlsx"))
{
    dt = ExcelUtil.ImportFromStream(fs, v2003: false, sheetName: "sheet1", hasHeader: true);
}
```

没有样板代码，只有一行核心调用。

### HTTP 请求：像写本地方法一样写远程调用

```csharp
using LuBan.Common;
using LuBan.Common.Http;

var client = new HttpClientProxy();

// 普通 GET
string html = await client.GetAsync("https://api.example.com/data");

// 泛型反序列化
var user = await client.GetAsync<UserDto>("https://api.example.com/user/1");

// POST 对象，自动转 JSON
var result = await client.PostAsync("https://api.example.com/order", new { Id = 1, Name = "VIP" });

// 更懒的写法：字符串扩展
string data = await "https://api.example.com/data".HttpGetAsync();
```

### 缓存：自带过期，不用管GC

```csharp
using System;

var cache = MemoryCache<string>.GetInstance();

cache.Set("session:123", "user-data", TimeSpan.FromMinutes(30));

var value = cache.Get("session:123");
```

### 文件操作：读写、序列化、删除一把梭

```csharp
using LuBan.Common.IO;

// 读文本
string content = FileUtil.ReadString("readme.txt");

// 写文本
FileUtil.WriteString("output.txt", "Hello LuBan!");

// 对象存文件
FileUtil.Write("config.json", new { Host = "localhost", Port = 5000 });

// 读对象
var config = FileUtil.Read<Config>("config.json");

// 文件存在吗？
bool exists = FileUtil.Exists("output.txt");
```

### 日志：三行配置，一行打印

```csharp
using System;

Logger.Info("订单模块", "订单 10086 创建成功");
Logger.Error("支付模块", exception, "支付回调处理失败");
Logger.Debug("调试", $"耗时 {elapsedMs}ms");
```

> 使用前请确保项目根目录有日志配置文件。

### 日期时间：告别手撕时间逻辑

```csharp
using LuBan.Common;

var now = DateTimeUtil.Now;
string weekday = DateTimeUtil.GetChineseWeekDay(now);     // 星期四
int workDays = DateTimeUtil.GetWorkDays(now, now.AddDays(10));
```

### 业务异常：让错误也优雅

```csharp
using LuBan.Common.Errors;

try
{
    throw new FriendlyException(EnumErrorCode.InvalidParameter, "手机号格式不正确");
}
catch (FriendlyException ex)
{
    Console.WriteLine($"[{ex.ErrorCode}] {ex.Message}");
}
```

### 事件总线：接口先行，实现灵活

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
        Console.WriteLine($"欢迎新用户：{e.UserName}");
        return Task.CompletedTask;
    }
}

// IEventBus 的具体实现由上层框架或业务项目注入
```

---

## 使用小贴士

1. **命名空间别搞错**：`LuBan.Common` 是根命名空间，但具体工具分散在 `LuBan.Common.Data`、`LuBan.Common.IO`、`LuBan.Common.Http`、`LuBan.Common.Sms`、`LuBan.Common.Errors`、`LuBan.Common.EventBus`、`LuBan.Common.Calendar`、`LuBan.Common.Consts`、`LuBan.Common.IPToRegion`、`LuBan.Common.AntiReplayAttacks`、`LuBan.Common.NPinyin` 等子命名空间；`MemoryCache<T>`、`IServiceCache`、`Logger` 位于 `System`。
2. **日志先配置**：`Logger` 依赖日志配置文件，否则可能找不到输出目标。
3. **线程安全要选型**：多线程场景优先 `ThreadSafeList`、并发字典或 `MemoryCache<T>`。
4. **缓存设过期**：缓存不是无限内存，请按业务设置合理过期时间。
5. **配置走 Nacos**：经常变化的配置用 `NacosConfigUtil`，避免改配置就发版。

---

## 许可证

MIT License

---

**LuBan.Common** —— 让 .NET 企业开发更简单、更快、更爽。

> 如果你也觉得重复造轮子很无聊，那就把它加进你的工具箱吧。
