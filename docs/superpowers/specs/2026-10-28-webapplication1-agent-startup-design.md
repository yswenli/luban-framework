# WebApplication1 的 LuBan.AIAgent 接入方式设计（v3，决策已定稿）

> 设计日期：2026-10-28（v2 按代码核实修订；v3 四项待决事项已由需求方拍板）
> 涉及仓库：`luban-framework`
> 涉及工程：`LuBan.Web.Core`、`LuBan.AIAgent`（只读依赖）、`WebApplication1`、`LuBan.XTestProject`

## 0. v2 修订说明

v1 提交后做了两轮审查：一轮 builder check（假设我是实现者，问还缺什么），一轮逐条代码核实。以下结论**推翻了 v1 的部分内容**，均已按源码证据改写：

| v1 内容 | v2 结论 | 证据 |
|---|---|---|
| 「Nacos 可用时优先」作为第二级配置源 | **启动期拿不到 Nacos**，实际等价 appsettings | `NacosConfigUtil.Enable` 只在 `Set()` 内置 true，而 `Set()` 由 `NacosConfigInitializerService : IHostedService` 在 host 启动后才调用；插入点在 `builder.Build()` 之前 |
| `DefaultModel` 缺失不算不完整 | **缺失即每次请求 500**，已改为必填（D-A） | `AgentHost.cs:113`、`AgentHost.cs:317` 均 `throw new InvalidOperationException("未选择模型")` |
| `Program.cs` 用三行手写最小启动 | 框架真正的标准入口是 `WebApp.RunWebHost(args)`（D-C 已采纳） | `LuBan.Web.Core/WebApp.cs:80` → `ServiceHost.cs:230` → `CreateWebApplication`（额外做 `Logger.SetLogger`、`HostingOptions.AppOptions.Urls`、生命周期事件） |
| 控制器 DTO 加 `[Required]` | **加了也没用**，模型校验过滤器被全局关闭 | `LuBan.Web.Core/AspNetCore/ApiConfiguration.cs:47` `SuppressModelStateInvalidFilter = true` |

同时核实通过的：无循环引用（`LuBan.AIAgent` 只引用 `Common`/`DI`/`Threading`）、`[AllowAnonymous]` 可覆盖 `BaseApiController` 的 `[Authorize]`（`OnlineUserTestController` 现有先例）、`ProviderDefaults.ProviderIds` 为小写 id（`deepseek` 在列）、`LuBan.XTestProject` 已引用 `LuBan.Web.Core`（单测可直达）、`EnableSignalR=false` 时 `SetEndPoints` 不会因 `SignalROptions` 为 null 抛出。

## 1. 背景

`WebApplication1` 是框架的 Agent 集成演示/手动验证入口，接入方式分三处散落：

- `WebApplication1/Program.cs` 手写三行注册（`AddLuBanAgentDefaults` / `AddLuBanAgentHost` / `AddLuBanAgentOpenAI`）；
- 同文件用最小 API 内联了 `/agent/minimal` 示例端点；
- 配置只来自 `appsettings.json` 的 `LuBanAgent` 节，与框架「先查 `db_config`、再回退配置」的惯例不一致（现成先例：`LuBanOrmExtention.InitDataBaseOrm` 里 `SmsConfigResolver.Register(...)` 读 `db_config` 的 `sys-luban-framework:sms`）。
- `WebApplication1` 完全没走 `LuBan.Web.Core` 标准启动链，文件日志、全局 DI 扫描、JWT、Swagger、ORM 在示例站里都没跑起来。

另外 `AddLuBanAgentOpenAI` 在 `LuBanAgent:Models` 不完整时只记一条 `ChatClientFactoryWarning` 就不注册 `IChatClient`，问题被推迟到第一次请求才炸。

## 2. 目标

1. **注册收口**：Agent 注册从 `WebApplication1/Program.cs` 移到 `LuBan.Web.Core` 的 `StartupUtil.ConfigureServices`。
2. **配置来源分层**：
   - 首选 `db_config` 表中 `Code = sys-luban-framework:aiagent` 的记录，`Value` 为 `LuBanAgent` 节 JSON；
   - 该记录不存在时，回退宿主 `IConfiguration` 的 `LuBanAgent` 节（即 appsettings + 环境变量；`NacosConfigUtil.Read` 在启动期等价于此，见 §0）；
   - 两处都没有 → 不启用 Agent。
3. **启动期校验**：拿到配置后校验完整性/正确性，不通过同样不启用，且**不回退**（记录存在即权威）。
4. **不启用必须无副作用**：不注册任何 Agent 服务、不抛异常、Web 应用照常启动。
5. **端点进控制器**：`/agent/minimal` 的内联实现改由 `Controllers/Default/` 下控制器承载；未启用时返回 **503**。
6. **示例站走标准启动**：`WebApplication1` 改为框架标准启动方式。

## 3. 非目标

- 不改 `LuBan.AIAgent` 内部注册逻辑与任何 `AddLuBanAgent*` 方法签名。
- 不改 `db_config` 表结构、不改 `InitDataBaseOrm` 的既有行为（含数据库连不上即 `Environment.Exit(-1)`）。
- 不做运行期热更新：配置改了要重启。
- 不动 `LubanAgentCli` / `LubanAgentCodex`（走自己的 `AgentHostBuilder`）。
- 不改 `SmsConfigResolver`，只参照其兜底范式。
- 不引入新三方依赖。

## 4. 反目标（即使"技术上能跑"也算失败）

- 配置缺失或写错导致 WebApplication1 起不来，或起得来但请求必然 500。
- Agent 未启用时 DI 图里注册了半个 Agent 服务。
- 有人以后又把注册写回 `Program.cs`，形成两份入口。
- 框架源码出现厂商专有词（`Yunxiao` / `Codeup` / `AliyunDevOps` / 云效）。
- 未启用时静默无日志。

## 5. 约束

- 目标框架 `net10.0`；遵守既有文件头规范。
- `LuBan.Web.Core` 新增对 `LuBan.AIAgent` 的 `ProjectReference`（方向只能是 Web.Core → AIAgent）。
- 组件中英文 README 成对；涉及对外行为说明需两份同步。
- 测试为 MSTest，入口 `LuBan.XTestProject/LuBan.UnitTestProject.csproj`。
- 版本号日期制，发包人工执行。

## 6. 已定决策（来自需求方，不再讨论）

| # | 决策 |
|---|---|
| D1 | Agent 注册统一放在 `StartupUtil.ConfigureServices` |
| D2 | `LuBan.Web.Core` 直接引用 `LuBan.AIAgent`，接受"所有引用 Web.Core 的项目都会带上 AIAgent"的代价 |
| D3 | `WebApplication1` 整体切成框架标准启动方式 |
| D4 | `db_config` 用固定 `Code = sys-luban-framework:aiagent`，`Value` 就是 `LuBanAgent` 那一节的 JSON |
| D5 | 记录存在但内容坏 → 直接不启用，**不回退**配置 |
| D6 | 校验口径：`Models.Provider` 必须在框架已知 Provider 列表内（大小写不敏感）；`Models.Model`、`Models.ApiKey`、`DefaultModel` 非空；`BaseUrl` 若给出必须是合法 http(s) URL；Provider 未知即算错误 |
| D7 | 未启用时控制器返回 503 |
| D8 | 端点实现在 `Controllers/Default/` 下 |
| D9 | `InitDataBaseOrm` 失败即退出的既有行为不变 |

## 7. 决策记录（v3 定稿，实现按此执行）

| # | 结论 | 依据 |
|---|---|---|
| **D-A** | `DefaultModel` **必填**，缺失即不启用 | `AgentHost.cs:113`、`:317` 在 `QualifiedModelName` 为空时抛「未选择模型」；`DefaultModel` 是其唯一来源 |
| **D-B** | 第二级配置源 = 宿主 `IConfiguration`；代码保留 `NacosConfigUtil.Read` 调用但**不承诺 Nacos** | `NacosConfigUtil.Enable` 只由 `Set()` 置 true，而 `Set()` 在 `IHostedService.StartAsync` 之后；插入点在 `builder.Build()` 前 |
| **D-C** | `WebApplication1/Program.cs` 改为 `WebApp.RunWebHost(args)` 一行 | `WebApp.cs:80` → `ServiceHost.cs:230` → `CreateWebApplication`，是框架唯一完整启动链 |
| **D-D** | db 配置生效时，合并配置**注册进 DI 顶掉 `IConfiguration`** | 避免 `IOptions<LuBanAgentOptions>` 与直注 `IConfiguration` 的服务在一次启动内看到两套值 |

## 8. 留给实现者的默认（可推翻）

| # | 默认 | 理由 |
|---|---|---|
| C1 | 控制器路由 `POST /api/agent/chat`，保留 `[AllowAnonymous]` | 与被替换的 `/agent/minimal` 一致，示例站免登录 |
| C2 | 读库过程抛异常 → 当作"没有记录"，继续回退 | 与 `SmsConfigResolver` 的 `catch → null` 一致；数据库故障不该连带禁用 Agent |
| C3 | 插入点：`services.InitSms()` 之后、`services.AddServiceCache()` 之前 | 读 `db_config` 必须先有 ORM（`InitDataBaseOrm` 在 `:109`）；且必须在 `services.BuildProvider()`（`:121`）之前注册完 |
| C4 | 未启用时用 `ConsoleUtil` 打一条 WARN，含具体原因；不写 `Logger` | 此刻 `Logger` 未必就绪（`InitDataBaseOrm` 内同款注释） |
| C5 | 新增 `CommonConst.SysLuBanAgentCode = "sys-luban-framework:aiagent"` | 与 `SysSmsCode` 并列，避免魔法字符串 |
| C6 | 解析/校验逻辑独立成 `LuBan.Web.Core` 新类，db 读取通过委托注入 | 便于无数据库单测（照 `SmsConfigResolver` 范式） |
| C7 | 控制器内显式校验 `Prompt` 非空 → 400 | 全局模型校验过滤器被 `SuppressModelStateInvalidFilter=true` 关掉，`[Required]` 不会生效 |
| C8 | 请求体 DTO 放 `WebApplication1/Models/Entities/` | 与本项目既有输入模型同目录风格 |

## 9. 详细设计

### 9.1 新增：`LuBan.Web.Core/AIAgent/LuBanAgentStartupOptions.cs`

仅用于类型化读取与校验，字段与 `LuBanAgent` 节一致：

```csharp
public sealed class LuBanAgentStartupOptions
{
    public string? WorkspaceRoot { get; set; }
    public string? DefaultModel { get; set; }
    public LuBanAgentModelsOptions? Models { get; set; }
}

public sealed class LuBanAgentModelsOptions
{
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
}
```

公开（而非 internal）：单测项目需要直接构造，且 `LuBan.XTestProject` 未配置 `InternalsVisibleTo`。

**为什么不直接复用 `LuBanAgentOptions`？** 因为它的 `Models` 声明是 `Dictionary<string, ModelEndpointOptions>`（`Configuration/LuBanAgentOptions.cs:52`），而 `appsettings.json` 里 `LuBanAgent:Models` 是**扁平对象** `{"Provider","Model","ApiKey"}`（`WebApplication1/appsettings.json:88-92`）。用 `LuBanAgentOptions` 绑定会得到 3 条垃圾字典项（键 `Provider`/`Model`/`ApiKey`，值全是空 `ModelEndpointOptions`），校验必然判错。运行期真正读 `Models` 的是 `AddLuBanAgentOpenAI`，它直接读原始配置键（`LuBanAgentOpenAIExtensions.cs:55-59`），绕开了这个字典——所以这是框架既有缺陷但当前无人消费，本次只为校验建一个扁平 DTO，不动 `LuBanAgentOptions`（留档见 §12）。

### 9.2 新增：`LuBan.Web.Core/AIAgent/LuBanAgentConfigResolver.cs`

职责：产出「是否启用 + 用哪份 `IConfiguration` + 来源 + 原因」。

```csharp
public static class LuBanAgentConfigResolver
{
    public static LuBanAgentResolution Resolve(
        IConfiguration configuration,
        Func<string, string?>? dbConfigReader = null);
}

public sealed class LuBanAgentResolution
{
    public bool Enabled { get; init; }
    public IConfiguration? Configuration { get; init; }  // 仅启用时非空，用于注册
    public string Source { get; init; } = "";             // "db" / "host"（仅诊断）
    public string? DisabledReason { get; init; }          // 仅未启用时非空
}
```

**算法**

1. `raw = SafeReadDb(dbConfigReader ?? DefaultDbReader)`；`SafeReadDb` 内 `try/catch`，异常返回 `null`（C2）。
2. `raw` 非空：
   1. 解析 JSON：若顶层已含 `LuBanAgent` 属性则原样用，否则包成 `{"LuBanAgent": <raw>}`（容忍两种写法）；解析失败 → 不启用，`Source="db"`，不回退。
   2. `using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));` → `merged = new ConfigurationBuilder().AddConfiguration(configuration).AddJsonStream(stream).Build()`（后加的源优先，db 覆盖宿主配置）。`Build()` 会立即触发所有 provider 的 `Load()`，所以 `stream` 可以在 `using` 作用域内释放。
   3. `Validate(merged.GetSection("LuBanAgent").Get<LuBanAgentStartupOptions>())`；不过 → 不启用、不回退（D5）。
   4. 通过 → `Enabled=true`、`Configuration=merged`、`Source="db"`。
3. `raw` 为空：
   1. `var options = NacosConfigUtil.Read<LuBanAgentStartupOptions>("LuBanAgent")`（当前必然走 `ConfigUtil` → 宿主 `IConfiguration`，见 §0）。
   2. `null` 或校验不过 → 不启用，`Source="host"`。
   3. 通过 → `Enabled=true`、`Configuration=configuration`、`Source="host"`。

两条路径都先绑定成 `LuBanAgentStartupOptions` 再走**同一个** `Validate(LuBanAgentStartupOptions?)`，保证口径一致。

**已知边界（写入文档，不在本次修）**：第 3 步的「读」走 `NacosConfigUtil.Read`（受 `Enable` 开关影响），而「注册」用 `configuration`（宿主）。当前 `Enable` 恒为 false，两者同源，等价。**若将来有人给 Web 启动加了 Nacos 预加载**，就会出现「校验用 Nacos 值、注册用 appsettings 值」的分叉——那时代码必须改为用同一份来源重建配置。届时本节的这一行就是修改点。

**校验规则**（`Validate` 输入 `LuBanAgentStartupOptions?`）

| 项 | 规则 | 失败文案（示例） |
|---|---|---|
| JSON 可解析 | `Value` 必须是合法 JSON（db 路径独有） | `db_config 中 sys-luban-framework:aiagent 的 Value 不是合法 JSON：<msg>` |
| 整体存在 | `options is not null` | `未找到 LuBanAgent 配置` |
| Models 存在 | `options.Models is not null` | `缺少 LuBanAgent:Models 配置` |
| Provider | 非空且 `ProviderDefaults.ProviderIds.Contains(x, OrdinalIgnoreCase)` | `Provider 'xxx' 不在框架已知列表内` |
| Model | 非空 | `缺少 Models.Model` |
| ApiKey | 非空 | `缺少 Models.ApiKey` |
| BaseUrl | 空则跳过；非空 `Uri.TryCreate(Absolute)` 且 scheme ∈ {http, https} | `Models.BaseUrl 不是合法的 http(s) 地址` |
| DefaultModel | 非空（格式 `provider:model` 不强制校验，交给运行期） | `缺少 DefaultModel` |

`BaseUrl` 必须在此拦截：`AddLuBanAgentOpenAI` 把 `new Uri(baseUrl)` 放在 `IChatClient` 工厂 lambda 里（`LuBanAgentOpenAIExtensions.cs:70-76`），非法值不是注册期抛，而是**首次解析 `IChatClient`（即第一个请求）**才抛 —— 启动期校验是唯一能提前拦住的地方。

**默认 db 读取实现**

```csharp
static string? DefaultDbReader(string code)
{
    if (LuBanOrm.SqlSugarScope is null) return null;   // 库未初始化时 InitDataBaseOrm 已退出，这里只做防御
    return new DbRepository<DbConfig>().GetFirst(q => q.Code == code && q.IsDelete == false)?.Value;
}
```

用 `GetFirst`（`LuBan.Orm/BaseRepository.cs:605`，注释明确"不存在返回 null"）而不是 `First`；`SmsConfigResolver` 里的 `First` 是既有写法（`AsQueryable().First(where)`），本次不复刻。
`DbRepository.GetTenantId()` 在无 `HttpContext` 时回退 `LuBanOrmConst.MainConfigId`（`LuBan.Web.Core/DbRepository.cs:47-58`），启动期调用安全。

### 9.3 修改：`LuBan.Web.Core/Utils/StartupUtil.cs`

在 `services.InitSms();`（`StartupUtil.cs:112`）之后、`services.AddServiceCache();`（`:115`）之前插入。**整段必须包在 try/catch 里**：`ConfigureServices` 外层是 `catch → Environment.Exit(-1)`（`StartupUtil.cs:124-130`），任何从 Agent 冒出的异常都会把站点直接杀掉，违反「配置不完整/错误不得导致启动失败」。

```csharp
//配置 Agent（db_config 优先，回退宿主配置；不完整则不启用）
try
{
    var agentResolution = LuBanAgentConfigResolver.Resolve(configuration);
    if (agentResolution.Enabled && agentResolution.Configuration is { } agentConfig)
    {
        services.AddLuBanAgentDefaults(agentConfig);
        services.AddLuBanAgentHost();
        services.AddLuBanAgentOpenAI(agentConfig);

        if (!ReferenceEquals(agentConfig, configuration))
        {
            // D-D：让运行期直接注入 IConfiguration 的 Agent 服务看到同一份值
            services.Replace(ServiceDescriptor.Singleton<IConfiguration>(agentConfig));
        }
    }
    else
    {
        ConsoleUtil.WriteLine($"LuBan Agent 未启用：{agentResolution.DisabledReason}", color: ConsoleColor.Yellow);
    }
}
catch (Exception ex)
{
    // Agent 属可选能力：初始化失败只降级为不启用，不得拖垮整个 Web 应用
    ConsoleUtil.WriteLine($"LuBan Agent 初始化失败，已跳过：{ex}", color: ConsoleColor.Yellow);
}
```

- 注册顺序与 `Program.cs` 原实现一致（Defaults → Host → OpenAI）。
- 未启用时一行都不注册、不抛异常。
- `Replace` 需 `Microsoft.Extensions.DependencyInjection.Extensions`（Web.Core `GlobalUsing.cs` 已引入）；合并配置是原配置超集，只有 `LuBanAgent` 节被覆盖。
- **不要**试图用 `ValidateOnBuild` 兜底：`ServiceProviderUtil.BuildProvider()`（`LuBan.DI/ServiceProviderUtil.cs:107`）用的是无参 `BuildServiceProvider()`，未开 `ValidateOnBuild`/`ValidateScopes`，缺依赖只会在首次请求暴露。所以启动期校验必须自己把该拦的都拦住。
- `ConsoleUtil.WriteLine` 是 `string` 扩展方法（`LuBan.Common/ConsoleUtil.cs:105`），以静态形式调用可用。**不用 `Logger`**：此刻静态 `Logger` 未必已挂 loggerFactory（`Logger.SetLogger` 在 `CreateWebApplication` 里 `Build()` 之后才调）。

### 9.4 修改：`WebApplication1/Program.cs`

```csharp
WebApp.RunWebHost(args);
```

删除：三行 `AddLuBanAgent*`、`/agent/minimal` 端点、`using LuBan.AIAgent*` 三行。
`WebApplication1/GlobalUsing.cs:16` 已有 `global using LuBan.Web.Core;`，所以连 `using` 都不需要。
`ServiceHost.CreateWebApplication` 内部已调用 `ConfigureServices` / `Configure`，Agent 注册随之生效。

### 9.5 新增：`WebApplication1/Controllers/Default/AgentController.cs`

```csharp
[Route("api/agent")]
[AllowAnonymous, AllowAccess]
public class AgentController : BaseApiController
{
    [DisplayName("对话"), HttpPost("chat")]
    public async Task<IActionResult> ChatAsync([FromBody] AgentChatInput? input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input?.Prompt))
            return StatusCode(StatusCodes.Status400BadRequest, ErrorResult("prompt 不能为空", 400));

        var host = HttpContext.RequestServices.GetService<IAgentHost>();
        if (host is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ErrorResult("LuBan Agent 未启用：请配置 db_config 的 sys-luban-framework:aiagent 或 appsettings 的 LuBanAgent 节", 503));

        var sessions = HttpContext.RequestServices.GetRequiredService<ISessionManager>();
        var session = await sessions.CreateSessionAsync("web-demo", null);
        var sessionId = session.SessionId;
        await sessions.SetCurrentSessionAsync(sessionId);
        await host.InitializeAsync(sessionId, cancellationToken);

        var sb = new StringBuilder();
        await foreach (var evt in host.RunStreamingAsync(
            new AgentRunRequest { Input = input.Prompt, SessionId = sessionId }, cancellationToken))
        {
            if (evt is TextDeltaEvent delta) sb.Append(delta.Delta);
        }
        return Ok(SuccessResult(sb.ToString()));
    }
}
```

配套 DTO（`WebApplication1/Models/Entities/AgentChatInput.cs`）：

```csharp
/// <summary>Agent 对话入参。</summary>
public class AgentChatInput
{
    /// <summary>用户输入。</summary>
    public string Prompt { get; set; } = "";
}
```

- **保持与被替换的 `/agent/minimal` 完全等价**：每次请求新建会话，不提供 `SessionId` 复用入参（YAGNI；原端点也没有）。
- 用 `RequestServices.GetService<IAgentHost>()` 而不是构造注入：未启用时控制器仍能构造，返回 503 而不是 500。
- `IAgentHost` / `ISessionManager` 均为 scoped，从请求作用域取，不跨请求持有。
- `ErrorResult(string, int)` 来自 `BaseController`（`LuBan.Web.Core/AspNetCore/BaseController.cs:233`），返回 `Fail` 结构，符合框架响应约定。
- `ISessionManager` 默认实现 `InMemorySessionManager` 注册为 **Singleton**，`SetCurrentSessionAsync` 是进程级当前会话；并发请求会互相串。这是原 `/agent/minimal` 就有的示例级缺陷，本次只搬不改（留档，见 §12）。
- 参数类型写成 `AgentChatInput?` 是必需的：项目开了 `<Nullable>enable</Nullable>`，body 缺失时确实为 null。

### 9.6 新增常量：`LuBan.Common/Consts/CommonConst.cs`

插在 `SysSmsCode`（`CommonConst.cs:117`）之后：

```csharp
/// <summary>
/// LuBan Agent 配置（db_config 中整段 JSON，结构与 appsettings 的 LuBanAgent 节一致）
/// </summary>
public const string SysLuBanAgentCode = "sys-luban-framework:aiagent";
```

### 9.7 工程引用

`LuBan.Web.Core/LuBan.Web.Core.csproj` 增加：

```xml
<ProjectReference Include="..\LuBan.AIAgent\LuBan.AIAgent.csproj" />
```

`WebApplication1` 对 `LuBan.AIAgent` 的直接引用保留（控制器直接用其类型）。

### 9.8 命名空间与 using（已逐个核对，避免编译期返工）

| 文件 | 需要补的 using | 说明 |
|---|---|---|
| `LuBan.Web.Core/GlobalUsing.cs` | `global using LuBan.Web.Core.AIAgent;` | `StartupUtil` 在 `LuBan.Web.Core.Utils`，不补则解析不到解析器 |
| `LuBan.Web.Core/AIAgent/*.cs` | 无需 | Web.Core GlobalUsing 已含 `Microsoft.Extensions.Configuration`、`Microsoft.Extensions.DependencyInjection(.Extensions)`、`LuBan.Orm.Entities`、`LuBan.Common.Consts`、`System.Text.Json`、`System.Text`、`LuBan.AIAgent` 需显式（见下） |
| 解析器文件 | `using LuBan.AIAgent.Configuration.Providers;` | `ProviderDefaults` 在该命名空间 |
| `WebApplication1/Controllers/Default/AgentController.cs` | `using LuBan.AIAgent.Hosting;`、`using LuBan.AIAgent.Sessions;`、`using System.Text;` | `IAgentHost`/`AgentRunRequest`/`TextDeltaEvent` 在 `Hosting`，`ISessionManager` 在 `Sessions`；`System.Text` 不在 Web SDK 隐式 using 内 |

已核实存在的全局 using（无需重复）：`WebApplication1/GlobalUsing.cs` 含 `LuBan.Web.Core`、`LuBan.Web.Core.Attributes`（`AllowAccess`）、`LuBan.Common.Data`（`Fail`/`Success`）、`System.ComponentModel`（`DisplayName`）。
`AddJsonStream` 依赖的 `Microsoft.Extensions.Configuration.Json 10.0.14` 由 `LuBan.Common` 传递提供（已确认在 `LuBan.Web.Core/obj/project.assets.json` 中）。

## 10. 日志与可观测性

- 未启用：`ConsoleUtil` 黄字一条 `LuBan Agent 未启用：<原因>`；原因需能区分「没有配置」「JSON 坏」「Provider 未知」「缺 Model/ApiKey」「BaseUrl 非法」（及 D-A 定稿后的 DefaultModel）。
- 启用：不加噪音日志。
- 不用 `ChatClientFactoryWarning` 作主要提示——启动期校验已接管。

## 11. 测试计划

### 11.1 单测（`LuBan.XTestProject`，新增 `LuBanAgentConfigResolverUnitTest.cs`）

通过 `Resolve(configuration, dbConfigReader)` 注入假读取委托，不需要数据库：

1. db 合法 JSON → `Enabled=true`、`Source="db"`、合并配置里 `LuBanAgent:Models:Provider` 取 db 值而非宿主值。
2. db 记录 JSON 非法 → `Enabled=false`、原因含 JSON 关键字、**不回退**（即便宿主配置合法）。
3. db 无记录 + 宿主配置合法 → `Enabled=true`、`Source="host"`。
4. db 无记录 + 宿主无 `LuBanAgent` → `Enabled=false`。
5. db 读取委托抛异常 → 按"无记录"处理，回退（C2）。
6. Provider 未知（即便给了合法 BaseUrl）→ `Enabled=false`。
7. `BaseUrl="not-a-url"` → 失败；`https://api.example.com/v1/` → 通过。
8. `Value` 多包一层 `{"LuBanAgent": {...}}` → 仍可解析。
9. 缺 `DefaultModel` → `Enabled=false`。
10. db 覆盖只作用于 `LuBanAgent` 节：合并后 `DbConnectionOptions` 等原节仍可读，且宿主原有 `LuBanAgent:WorkspaceRoot` 在 db 未给该键时保留（配置源后者优先、同键覆盖、异键互补）。

测试结构沿用仓库现有 MSTest 风格（`[TestClass]` / `[TestMethod]`）。`LuBan.UnitTestProject.csproj` 已同时引用 `LuBan.Web.Core` 与 `LuBan.AIAgent`，无需加 `InternalsVisibleTo`（解析器与 DTO 均为 `public`）。

### 11.2 构建

- 单项目验收：`dotnet build luban-framework/LuBan.Web.Core/LuBan.Web.Core.csproj -p:RestoreIgnoreFailedSources=true`、`.../WebApplication1.csproj`、`.../LuBan.XTestProject/LuBan.UnitTestProject.csproj`。
- `WebApplication1/Nuget.Config` 指向已下线源（`https://dev.coding.net/...`），该工程 restore 会 NU1301。绕法：`-p:RestoreIgnoreFailedSources=true`，或先 `dotnet restore` 一次后 `--no-restore`。
- 整解决方案构建不作为验收门槛，验收看单项目退出码。

### 11.3 手工冒烟

| 场景 | 预期 |
|---|---|
| db 无记录 + appsettings 无 `LuBanAgent` 节 | 站点正常起；一条"未启用"日志；`POST /api/agent/chat` → 503 |
| 只配 appsettings 合法 | 站点起；接口可用；`/swagger` 可开 |
| db 记录合法且与 appsettings 不同 | 生效的是 db 值（用不同 Model 名验证） |
| db 记录 JSON 坏 | 不启用、日志写明原因、不回退 |
| Provider 填未知值 | 不启用、日志写明原因 |

## 12. 风险与验证点

| 风险 | 说明 | 处理 |
|---|---|---|
| minimal hosting 与 `UseRouting`/`UseEndpoints` | `StartupUtil.Configure` 显式 `UseRouting` + `SetEndPoints` 内 `UseEndpoints`，`WebApplication` 对二者自动补全有敏感点；仓库内此前无第二个 Web 宿主可对照 | 冒烟必须验证站点起得来、`/swagger`、控制器路由都通；若冲突退回 `CreateWebApplication`+`MapControllers` 变体并回报 |
| 首次走完整标准启动 | `WebApplication1` 历史上没跑过 `ConfigureServices` 全家桶（ORM 建表/种子数据/后台任务） | 冒烟时确认无初始化异常；`EnableRedisCache=false`、`EnableSignalR=false`、`EnableHealthCheck=false` 已核对，不触发 Redis/SignalR 依赖 |
| HTTPS 端口需要开发证书 | `HostingOptions.AppOptions.Urls = ["https://127.0.0.1:7000"]`（`appsettings.json:43`/`appsettings.dev.json:43`），`SetWebAppUrls` 原样写入 `webApp.Urls` | 机器无 dev cert 时启动即失败，属环境问题；冒烟前 `dotnet dev-certs https --check`，或用 `--urls http://127.0.0.1:7000` 启动参数覆盖 |
| 后台任务会真的跑起来 | `EnableBackgroundJob=true` + `ServiceHost.CreateWebApplication` 注册 `Started`→`JobServiceLoader.Start()` | 首次运行可能因任务依赖外部服务报错；不影响站点存活，冒烟时留意控制台 |
| 无 `ValidateOnBuild` 兜底 | `BuildProvider()` 用无参 `BuildServiceProvider()`（`LuBan.DI/ServiceProviderUtil.cs:112`） | 缺依赖只在首次请求暴露，故启动期校验必须做全（本设计的核心价值） |
| 覆盖注册 `IConfiguration` | 其他服务注入 `IConfiguration` 时拿到合并版 | 合并版是超集；冒烟重点看 `DbConnectionOptions`、`HostingOptions` 仍正常 |
| `Replace` 会削弱 appsettings 文件热更新 | 新的根由 `AddConfiguration(原配置)` + `AddJsonStream` 组成；`ChainedConfigurationProvider` 转发原根的 reload token，但 `IOptionsMonitor` 走的是**注册时捕获**的 section，不受影响；受影响面只有「运行期直注 `IConfiguration` 的服务」 | 目前只有 `SessionRuntimeManager`（读一个 int 阈值），实例站在示例站无热更新需求；若将来要求热更新，改为「不 Replace，只给 Agent 传 `agentConfig`」 |
| `ConfigUtil` / `NacosConfigUtil` 静态读源**不受** `Replace` 影响 | `ConfigUtil._configuration` 在 `ConfigureServices` 开头就被赋成原始宿主配置（`ConfigUtil.cs:40-44`），`Replace` 只改 DI 容器 | 合并配置是超集，其他节读取结果不变，无实际影响；但不要指望「静态读」能看到 db 覆盖值 |
| 默认 appsettings 会直接启用 Agent | `WebApplication1/appsettings.json:85-93` 已有完整 `LuBanAgent` 节（`ApiKey=sk-xxxxxxxx` 占位） | 冒烟「不启用」场景必须手动删节或清空 `ApiKey`，不能默认就期待 503 |
| `LuBanAgentOptions.Models` 类型与配置形状不符（既有缺陷，留档不改） | 声明是 `Dictionary<string, ModelEndpointOptions>`，实际配置是扁平 `{Provider,Model,ApiKey}`；绑定会产出 3 条垃圾字典项 | 当前无任何代码消费该属性（`AddLuBanAgentOpenAI` 直读原始键），本次不修，只在上游用它做校验时避开（§9.1） |
| 启动期读库 | 位于 `InitDataBaseOrm` 之后，`Logger` 未必就绪 | 只用 `ConsoleUtil`；读库异常按无记录处理 |
| 依赖传染 | 引用 `Web.Core` 的项目被动带上 AIAgent（含 Playwright），且 `LuBan.Web.Core.nupkg` 会新增对 `LuBan.AIAgent` 的包依赖 | 已由 D2 接受，发布说明点明 |
| appsettings 现占位 ApiKey | `sk-xxxxxxxx` 非空会通过校验，运行期 401 | 示例配置问题，冒烟用真实 Key |
| 路由变更 | `/agent/minimal` → `/api/agent/chat` | 示例站无外部消费者 |
| 会话串号（既有，留档不改） | `ISessionManager` 默认实现注册为 Singleton，`SetCurrentSessionAsync` 是进程级 | 原 `/agent/minimal` 同样问题；本次只搬不改，示例站单并发使用 |

## 13. 验收标准

1. `Program.cs` 不再出现任何 `AddLuBanAgent*` 调用，也不再内联端点。
2. 删掉 db 记录与 appsettings 节后：站点正常启动、接口 503、日志有唯一一条明确原因。
3. 仅 appsettings 合法时接口可用；db 记录覆盖时能观测到 db 值生效。
4. 记录坏 / Provider 未知时不启用且不回退。
5. 新增单测全绿，三处构建退出码 0。
6. 框架源码厂商词扫描为 0。
7. Agent 相关代码出现任何异常时，站点仍能启动（验证 §9.3 的 try/catch 有效）。

## 14. 附录：本轮审查核实过的代码事实

| 事实 | 位置 |
|---|---|
| `ConfigureServices` 顺序：`InitConfigUtil` → `AddNacos...` → `AutoInject` → `SetControllers` → `AddJwt` → `InitDataBaseOrm` → `InitSms` → `AddServiceCache` → `AddLuBanReporting` → `BuildProvider` | `LuBan.Web.Core/Utils/StartupUtil.cs:58-140` |
| 外层 `catch → Environment.Exit(-1)` | `StartupUtil.cs:141-148` |
| `Configure` 内 `UseRouting` → … → `SetEndPoints(UseEndpoints)` | `StartupUtil.cs:150-215` |
| `SetEndPoints` 仅 `EnableSignalR=true` 才碰 `SignalROptions` | `LuBan.Web.Core/AspNetCore/RequestLimit.cs:158-190` |
| 标准启动链 `WebApp.RunWebHost` → `ServiceHost.RunWebHost` → `CreateWebApplication`（含 `Logger.SetLogger`、生命周期事件、`SetWebAppUrls`） | `LuBan.Web.Core/WebApp.cs:80`、`ServiceHost.cs:165-250` |
| `BuildProvider` 用无参 `BuildServiceProvider()`，未开 Validate* | `LuBan.DI/ServiceProviderUtil.cs:107-118` |
| `NacosConfigUtil.Enable` 仅由 `Set()` 置 true；`Read<T>` 在 `Enable=false` 时走 `ConfigUtil.Read<T>`（读宿主 `IConfiguration`） | `LuBan.Common/NacosConfigUtil.cs:152,349-353`、`ConfigUtil.cs:40-60` |
| `SmsConfigResolver` + `db_config` 兜底范式 | `LuBan.Web.Core/Database/LuBanOrmExtention.cs`（`InitSms` 段） |
| `DbRepository.GetTenantId()` 无 `HttpContext` 时回退 `MainConfigId` | `LuBan.Web.Core/DbRepository.cs:47-58` |
| `GetFirst` 不存在返回 null | `LuBan.Orm/BaseRepository.cs:605` |
| `DefaultModel` 缺失 → `throw InvalidOperationException("未选择模型")` | `LuBan.AIAgent/Hosting/AgentHost.cs:113,317` |
| `DefaultAgentModelSelection` 读 `IOptions<LuBanAgentOptions>.DefaultModel` | `LuBan.AIAgent/Hosting/DefaultSeams.cs:62-80` |
| `AddLuBanAgentOpenAI` 的 `new Uri(baseUrl)` 在 `IChatClient` 工厂内（惰性） | `LuBan.AIAgent/LuBanAgentOpenAIExtensions.cs:55-77` |
| `ProviderIds` 来源 `DefaultModels.Keys`，字典 `OrdinalIgnoreCase`，含 `deepseek` | `LuBan.AIAgent/Configuration/Providers/ProviderDefaults.cs:46-47,132` |
| `AddLuBanAgentHost` 全部 `TryAdd*`，无 HostedService | `LuBan.AIAgent/LuBanAgentHostExtensions.cs:52-75` |
| `AddLuBanAgent` 无 `AddHostedService`，`PlaywrightSession` 惰性 | `LuBan.AIAgent/LuBanAgentExtensions.cs:42-140` |
| `SuppressModelStateInvalidFilter = true` → `[Required]` 不生效 | `LuBan.Web.Core/AspNetCore/ApiConfiguration.cs:47` |
| `ErrorResult(string, int)` protected，返回 `Fail` | `LuBan.Web.Core/AspNetCore/BaseController.cs:233` |
| `ConsoleUtil.WriteLine` 是 `string` 扩展方法 | `LuBan.Common/ConsoleUtil.cs:105` |
| `IAgentHost` / `AgentRunRequest` / `TextDeltaEvent` 在 `LuBan.AIAgent.Hosting`，`ISessionManager` / `SessionInfo` 在 `LuBan.AIAgent.Sessions` | `IAgentHost.cs:25`、`AgentRunRequest.cs:17`、`AgentStreamEvent.cs:20`、`ISessionManager.cs:30,153` |
| 无循环引用：`LuBan.AIAgent` 仅引 `Common`/`DI`/`Threading` | `LuBan.AIAgent/LuBan.AIAgent.csproj` |
| `WebApplication1` 已引 `LuBan.AIAgent`，`GlobalUsing.cs` 已含 `LuBan.Web.Core` / `Attributes` / `Common.Data` / `System.ComponentModel` | `WebApplication1/WebApplication1.csproj`、`GlobalUsing.cs` |
| `LuBanAgentOptions.Models` 是 `Dictionary<string, ModelEndpointOptions>`，与实际配置的扁平形状不符；`ModelEndpointOptions` 只有 `BaseUrl`/`ApiKey` 两个属性 | `LuBan.AIAgent/Configuration/LuBanAgentOptions.cs:52`、`ModelEndpointOptions.cs` |
| `WebApplication1/appsettings.json` 的 `LuBanAgent` 节已完整（含 `ApiKey=sk-xxxxxxxx` 占位） | `appsettings.json:85-93` |
| `AppOptions.SignalROptions` **存在**（`HubUrl=/hubs/common` 等），只是 `EnableSignalR=false` | `appsettings.json:67-82` |
| `ConfigUtil.InitConfigUtil(configuration)` 把 `_configuration` 指向宿主配置；`Replace(IConfiguration, …)` 改不到它 | `LuBan.Common/ConfigUtil.cs:40-44` |
| `HostingOptions.Default` 通过 `ConfigUtil.Read<HostingOptions>()` 读上述静态配置 | `LuBan.Web.Core/HostingOptions.cs:104-117` |
| `ServiceProviderUtil.InitServiceProvider(services)` 保存的是**同一个 `IServiceCollection` 实例**，`BuildProvider()` 用它另建一个容器 | `LuBan.DI/ServiceProviderUtil.cs:34-65,107-118` |
| `RouteConst.ROUTE_TEMPLATE_API = "api/[controller]/[action]"`；`OnlineUserTestController` 已有「派生类 `[Route("api/test/onlineuser")]` 覆盖 `BaseApiController`」的成型先例 | `LuBan.Web.Core/Consts/RouteConst.cs:34`、`WebApplication1/Controllers/Default/OnlineUserTestController.cs:34-36` |

## 15. 附录：历次审查修正的问题

| # | 原表述 | 事实 | 处置 |
|---|---|---|---|
| F1 | 「Nacos 可用时优先」 | 启动期 Nacos 未加载 | D-B，文档与日志降级为「宿主配置」 |
| F2 | 「DefaultModel 缺失不算不完整」 | 缺它每次请求抛异常 | D-A，改必填 |
| F3 | 「Program.cs 三行手写即标准启动」 | 缺 Logger/URLs/生命周期 | D-C，改 `WebApp.RunWebHost` |
| F4 | 「DTO 加 `[Required]`」 | 模型校验过滤器被关 | 控制器内显式判空返 400 |
| F5 | 「BaseUrl 非法会在注册期抛」 | 在 `IChatClient` 工厂 lambda 内惰性抛 | 仅改表述，校验必要性不变 |
| F6 | 注册段无异常保护 | 外层 `catch` 会 `Environment.Exit(-1)` | 整段加 try/catch |
| F7 | `First(...)?.Value` | `First` 未定义"空返回 null"语义 | 改 `GetFirst` |
| F8 | 缺命名空间/using 清单 | 逐文件核对 | 新增 §9.8 |
| F9 | 校验 DTO 直接复用 `LuBanAgentOptions` | 其 `Models` 是字典，配置是扁平对象，绑定必错 | §9.1 说明并保留独立扁平 DTO |
| F10 | `HostingOptions.AppsOptions.Urls` | 属性名是 `AppOptions`，行号也错 | §12 已更正为 `appsettings.json:43` |
| F11 | 端点 DTO 带 `SessionId` | 原 `/agent/minimal` 每次新建会话，加 `SessionId` 属行为扩展 | 删除该字段，保持完全等价 |
| F12 | `AddJsonStream` 的 stream 生命周期未交代 | `Build()` 时才 `Load()`，需在 using 作用域内 Build | §9.2 已写明 |