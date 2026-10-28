# WebApplication1 接入 LuBan.AIAgent 启动方式 实施计划

> **Goal:** 把 Agent 注册从 `WebApplication1/Program.cs` 收口到 `LuBan.Web.Core` 的 `StartupUtil.ConfigureServices`；配置按「`db_config` → 宿主 `IConfiguration` → 不启用」三级解析并做启动期校验；示例端点改为控制器承载；示例站整体改为框架标准启动。

**Spec:** `docs/superpowers/specs/2026-10-28-webapplication1-agent-startup-design.md`（v3 定稿，已提交 `cd3837b`）

**涉及工程：** `LuBan.Common`、`LuBan.Web.Core`、`WebApplication1`、`LuBan.XTestProject`

---

## 0. 决策记录：宿主配置读取源（已拍板 = 方案 B）

Spec §9.2 规定宿主配置路径用 `NacosConfigUtil.Read<LuBanAgentStartupOptions>("LuBanAgent")` 读取。该方法读的是**静态** `ConfigUtil._configuration`（`LuBan.Common/NacosConfigUtil.cs:349-353` → `ConfigUtil.cs:51-59`），**不是** `Resolve(configuration, ...)` 传进来的那个 `configuration` 参数。

| 方案 | 内容 | 代价 |
|---|---|---|
| A | 宿主路径改为 `configuration.GetSection(SectionName).Get<LuBanAgentStartupOptions>()`，删掉 `NacosConfigUtil.Read` 调用 | 单测可直接注入；但偏离 D-B 字面表述 |
| **B（已选定，计划正文按此写）** | 保留 `NacosConfigUtil.Read`，单测里先 `ConfigUtil.InitConfigUtil(host)` 把宿主配置挂到静态指针上，用例结束用反射置回 `null` | 守住 D-B；单测依赖全局静态（本仓 MSTest 未开 `[Parallelize]`，同进程串行，可控），且「校验源/注册源可能分叉」这条边界**保留**在 §5 |

**因此实现时有两条必须遵守：**
1. `ResolveFromHost` **必须**用 `NacosConfigUtil.Read<LuBanAgentStartupOptions>(SectionName)`，不要改成读 `configuration` 参数。
2. Task 4 的测试统一走 `ResolveWithHost(host, reader)` 辅助方法（内部 `InitConfigUtil` → `Resolve` → `finally` 反射还原），**不要**在每个用例里裸调 `Resolve`。

## 1. 执行前必须知道的硬约束

1. **不改 `LuBan.AIAgent` 任何源码。** 本次它只是被引用方。工作区里已有的两个未提交改动（`LuBan.AIAgent/LuBan.AIAgent.csproj`、`LuBan.AIAgent/Skills/BuiltIn/AgentsMdGeneratorSkill.cs`）**不是本次任务**，不要 `git add`、不要改、不要回滚。
2. **不改 `<Version>`，不 `nuget push`。** 发包由人执行。
3. **`git add` 只允许显式路径**，禁止 `git add -A` / `git add .`。`docs/` 被 `.gitignore:367`（`/docs`）忽略，新增文档必须 `git add -f <path>`。
4. **提交信息中文。** framework 仓用纯中文短句（例：`新增 Agent 配置解析与启动接线`）。
5. **框架源码不得出现 `Yunxiao` / `Codeup` / `AliyunDevOps` / `云效`。**
6. **端点只搬不改。** 新控制器行为必须与被删除的 `/agent/minimal` 完全等价；既有缺陷（会话串号、`LuBanAgentOptions.Models` 类型不符）只留档不改。
7. **Agent 任何环节失败都不能让 Web 应用起不来。** 这是本次的核心价值，见 Task 6 的 try/catch。

## 2. 前置检查（一次性）

```powershell
cd D:\WorkBench\Walle\luban\luban-framework
git status --short          # 确认只有那两个 LuBan.AIAgent 改动 + 本次要动的文件
git log --oneline -3        # 基线应为 cd3837b（或其后）
dotnet dev-certs https --check   # 冒烟要用 https://127.0.0.1:7000，没有证书就走 --urls 覆盖
```

若 `git status` 出现意料之外的文件，先停下问需求方，不要自行清理。

## 3. 文件头规范（每个新增 .cs 都要）

本仓所有 `.cs` 都有 20 行左右的块注释头。新增文件时**复制同目录任一现有文件的前 23 行**，然后改这三处即可：

- `命名空间：` → 目标命名空间
- `文件名：` → 文件名（不带扩展名）
- `唯一标识：` → 该文件用途的一句话（本仓现有文件此处多为 GUID，但近期新增文件已写成文字描述，照近期写法即可）
- `创建时间：` → 当天日期
- `描述：` → 一句话用途

`namespace` 用文件作用域写法（`namespace X.Y;`），与现有代码一致。

---

## Task 1：新增常量 `CommonConst.SysLuBanAgentCode`

**Files:**
- Modify: `LuBan.Common/Consts/CommonConst.cs`（`SysSmsCode` 之后，即当前 L117 后）

- [ ] **Step 1: 插入常量**

在 `public const string SysSmsCode = "sys-luban-framework:sms";` 之后、空行之后插入：

```csharp
    /// <summary>
    /// LuBan Agent 配置（db_config 中整段 JSON，结构与 appsettings 的 LuBanAgent 节一致）
    /// </summary>
    public const string SysLuBanAgentCode = "sys-luban-framework:aiagent";
```

- [ ] **Step 2: 编译验证**

```powershell
dotnet build LuBan.Common\LuBan.Common.csproj -p:RestoreIgnoreFailedSources=true
```

期望：`Build succeeded`，退出码 0。

- [ ] **Step 3: 提交**

```powershell
git add LuBan.Common/Consts/CommonConst.cs
git commit -m "新增 Agent 配置的 db_config 编码常量"
```

---

## Task 2：`LuBan.Web.Core` 引用 `LuBan.AIAgent`

**Files:**
- Modify: `LuBan.Web.Core/LuBan.Web.Core.csproj`

- [ ] **Step 1: 在已有的 `<ProjectReference>` ItemGroup 里追加一行**

```xml
		<ProjectReference Include="..\LuBan.AIAgent\LuBan.AIAgent.csproj" />
```

放在该 ItemGroup 第一个位置（按字母序 `AIAgent` 在最前，与 `WebApplication1.csproj` 的写法一致）。

- [ ] **Step 2: 编译验证（此时应该还能过，代码没动）**

```powershell
dotnet build LuBan.Web.Core\LuBan.Web.Core.csproj -p:RestoreIgnoreFailedSources=true
```

期望：退出码 0。

> 注意：`LuBan.Web.Core` 开着 `GeneratePackageOnBuild=True`，这个 `ProjectReference` 会让 nupkg 新增对 `LuBan.AIAgent` 的依赖。发包时 `LuBan.AIAgent` 必须先有已发布版本（D2 已接受该代价）。

- [ ] **Step 3: 提交**

```powershell
git add LuBan.Web.Core/LuBan.Web.Core.csproj
git commit -m "Web 核心库引用 Agent 组件"
```

---

## Task 3：新增扁平校验 DTO

**Files:**
- Create: `LuBan.Web.Core/AIAgent/LuBanAgentStartupOptions.cs`

**为什么不用现成的 `LuBanAgentOptions`：** 它的 `Models` 是 `Dictionary<string, ModelEndpointOptions>`，而配置里 `Models` 是**扁平对象**。用它绑定必然产出 3 条垃圾字典项，校验必然判错。所以这里建一个形状正确的独立 DTO，只用于读取与校验，**不动** `LuBanAgentOptions`（既有缺陷，留档）。

- [ ] **Step 1: 建目录 + 建文件（带文件头，命名空间 `LuBan.Web.Core.AIAgent`）**

```csharp
namespace LuBan.Web.Core.AIAgent;

/// <summary>
/// LuBanAgent 配置节的校验用扁平模型（仅结构与 appsettings 一致，不代表运行期绑定类型）。
/// </summary>
public sealed class LuBanAgentStartupOptions
{
    /// <summary>Agent 工作目录。</summary>
    public string? WorkspaceRoot { get; set; }

    /// <summary>默认模型（格式 provider:model）。</summary>
    public string? DefaultModel { get; set; }

    /// <summary>模型接入点。</summary>
    public LuBanAgentModelsOptions? Models { get; set; }
}

/// <summary>
/// 模型接入点配置。
/// </summary>
public sealed class LuBanAgentModelsOptions
{
    /// <summary>Provider 标识。</summary>
    public string? Provider { get; set; }

    /// <summary>模型标识。</summary>
    public string? Model { get; set; }

    /// <summary>访问密钥。</summary>
    public string? ApiKey { get; set; }

    /// <summary>接口地址（可选，缺省用 Provider 内置地址）。</summary>
    public string? BaseUrl { get; set; }
}
```

> 必须 `public`：`LuBan.XTestProject` 没有 `InternalsVisibleTo`，单测要直接构造这两个类型。

- [ ] **Step 2: 在 `LuBan.Web.Core/GlobalUsing.cs` 追加全局 using**

按字母序插到 `global using LuBan.Web.Core;` 之前（`LuBan.Web.Core.AIAgent` 排在 `LuBan.Web.Core` 之后亦可，保持文件现有分组即可）：

```csharp
global using LuBan.Web.Core.AIAgent;
```

- [ ] **Step 3: 编译验证**

```powershell
dotnet build LuBan.Web.Core\LuBan.Web.Core.csproj -p:RestoreIgnoreFailedSources=true
```

期望：退出码 0。

- [ ] **Step 4: 提交**

```powershell
git add LuBan.Web.Core/AIAgent/LuBanAgentStartupOptions.cs LuBan.Web.Core/GlobalUsing.cs
git commit -m "新增 Agent 配置校验模型"
```

---

## Task 4：先写解析器单测（RED）

**Files:**
- Create: `LuBan.XTestProject/LuBanAgentConfigResolverUnitTest.cs`

**为什么先写：** 解析器有 8 条校验规则 + 2 条回退规则，逻辑密度高，写完再补测试一定会漏分支。这里用 TDD：先让 12 条用例全部编译失败（类不存在），再在 Task 5 实现到全绿。

- [ ] **Step 1: 建测试文件（本仓测试文件**不带**那 20 行头注释，直接从 `using` 开始，参考 `LuBan.XTestProject/AgentRunRequestValidationUnitTest.cs`）**

```csharp
using LuBan.Common;
using LuBan.Web.Core.AIAgent;

using Microsoft.Extensions.Configuration;

using System.Reflection;

namespace LuBan.XTestProject;

[TestClass]
public class LuBanAgentConfigResolverUnitTest
{
    /// <summary>造一个宿主配置。</summary>
    private static IConfiguration Host(params (string Key, string? Value)[] pairs)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(pairs.Select(p => new KeyValuePair<string, string?>(p.Key, p.Value)))
            .Build();

    /// <summary>
    /// 按方案 B 调解析器：宿主路径走静态 ConfigUtil，必须先把宿主配置挂上去。
    /// </summary>
    private static LuBanAgentResolution ResolveWithHost(IConfiguration host, Func<string, string?> dbReader)
    {
        ConfigUtil.InitConfigUtil(host);
        try
        {
            return LuBanAgentConfigResolver.Resolve(host, dbReader);
        }
        finally
        {
            // ConfigUtil 没有公开的重置入口，置回 null 恢复「未初始化」状态，避免污染其他用例
            typeof(ConfigUtil)
                .GetField("_configuration", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, null);
        }
    }

    /// <summary>合法宿主配置（可覆盖个别键）。</summary>
    private static IConfiguration ValidHost(string model = "host-model", string? defaultModel = "deepseek:host-model")
        => Host(
            ("LuBanAgent:Models:Provider", "deepseek"),
            ("LuBanAgent:Models:Model", model),
            ("LuBanAgent:Models:ApiKey", "sk-host"),
            ("LuBanAgent:DefaultModel", defaultModel));

    private const string ValidDbJson = """
        {"Models":{"Provider":"deepseek","Model":"db-model","ApiKey":"sk-db"},"DefaultModel":"deepseek:db-model"}
        """;

    [TestMethod]
    public void Resolve_DbValidJson_EnabledAndDbWins()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => ValidDbJson);

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("db", resolution.Source);
        Assert.IsNull(resolution.DisabledReason);
        Assert.IsNotNull(resolution.Configuration);
        Assert.AreEqual("db-model", resolution.Configuration!["LuBanAgent:Models:Model"]);
        Assert.AreEqual("deepseek:db-model", resolution.Configuration!["LuBanAgent:DefaultModel"]);
    }

    [TestMethod]
    public void Resolve_DbInvalidJson_DisabledAndNoFallback()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => "{not json");

        Assert.IsFalse(resolution.Enabled, "db 记录坏掉时不得回退到宿主配置");
        Assert.AreEqual("db", resolution.Source);
        StringAssert.Contains(resolution.DisabledReason, "JSON");
    }

    [TestMethod]
    public void Resolve_NoDbRecord_HostValid_EnabledFromHost()
    {
        var host = ValidHost();
        var resolution = ResolveWithHost(host, _ => null);

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
        Assert.AreSame(host, resolution.Configuration);
        Assert.IsNull(resolution.DisabledReason);
    }

    [TestMethod]
    public void Resolve_NoDbRecord_NoHostSection_Disabled()
    {
        var resolution = ResolveWithHost(Host(), _ => null);

        Assert.IsFalse(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
        StringAssert.Contains(resolution.DisabledReason, "未找到");
    }

    [TestMethod]
    public void Resolve_DbReaderThrows_TreatedAsNoRecord()
    {
        var resolution = ResolveWithHost(ValidHost(), _ => throw new InvalidOperationException("db down"));

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
    }

    [TestMethod]
    public void Resolve_UnknownProvider_DisabledEvenWithValidBaseUrl()
    {
        var dbJson = """
            {"Models":{"Provider":"no-such-provider","Model":"m","ApiKey":"sk","BaseUrl":"https://api.example.com/v1/"},"DefaultModel":"x:y"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "no-such-provider");
    }

    [TestMethod]
    public void Resolve_BaseUrlNotUrl_Disabled()
    {
        var dbJson = """
            {"Models":{"Provider":"deepseek","Model":"m","ApiKey":"sk","BaseUrl":"not-a-url"},"DefaultModel":"deepseek:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "BaseUrl");
    }

    [TestMethod]
    public void Resolve_BaseUrlValid_Enabled()
    {
        var dbJson = """
            {"Models":{"Provider":"deepseek","Model":"m","ApiKey":"sk","BaseUrl":"https://api.example.com/v1/"},"DefaultModel":"deepseek:m"}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled);
    }

    [TestMethod]
    public void Resolve_WrappedWithSectionName_StillParsed()
    {
        var dbJson = """
            {"LuBanAgent":{"Models":{"Provider":"deepseek","Model":"m","ApiKey":"sk"},"DefaultModel":"deepseek:m"}}
            """;
        var resolution = ResolveWithHost(Host(), _ => dbJson);

        Assert.IsTrue(resolution.Enabled, "顶层已含 LuBanAgent 时应原样使用");
        Assert.AreEqual("m", resolution.Configuration!["LuBanAgent:Models:Model"]);
    }

    [TestMethod]
    public void Resolve_MissingDefaultModel_Disabled()
    {
        var host = ValidHost(defaultModel: null);
        var resolution = ResolveWithHost(host, _ => null);

        Assert.IsFalse(resolution.Enabled);
        StringAssert.Contains(resolution.DisabledReason, "DefaultModel");
    }

    [TestMethod]
    public void Resolve_DbOverride_OnlyTouchesAgentSection()
    {
        var host = Host(
            ("LuBanAgent:WorkspaceRoot", @"C:\work\demo"),
            ("DbConnectionOptions:EnableConsoleSql", "false"),
            ("LuBanAgent:Models:Provider", "deepseek"),
            ("LuBanAgent:Models:Model", "host-model"),
            ("LuBanAgent:Models:ApiKey", "sk-host"),
            ("LuBanAgent:DefaultModel", "deepseek:host-model"));

        var resolution = ResolveWithHost(host, _ => ValidDbJson);

        Assert.IsTrue(resolution.Enabled);
        // db 未给的键保留宿主值（异键互补）
        Assert.AreEqual(@"C:\work\demo", resolution.Configuration!["LuBanAgent:WorkspaceRoot"]);
        // 其他节不受影响
        Assert.AreEqual("false", resolution.Configuration!["DbConnectionOptions:EnableConsoleSql"]);
        Assert.AreEqual("db-model", resolution.Configuration!["LuBanAgent:Models:Model"]);
    }

    [TestMethod]
    public void Resolve_BlankDbValue_TreatedAsNoRecord()
    {
        // db 读到空白串等同「没有记录」，走宿主回退
        var resolution = ResolveWithHost(ValidHost(), _ => "   ");

        Assert.IsTrue(resolution.Enabled);
        Assert.AreEqual("host", resolution.Source);
    }
}
```

- [ ] **Step 2: 确认 RED**

```powershell
dotnet build LuBan.XTestProject\LuBan.UnitTestProject.csproj -p:RestoreIgnoreFailedSources=true
```

期望：**编译失败**，报 `LuBanAgentConfigResolver` / `LuBanAgentResolution` 找不到。这就对了，不要在这步"修"测试。

> 若报的是 `AddInMemoryCollection` / `ConfigurationBuilder` 找不到，说明 `Microsoft.Extensions.Configuration` 相关包没传递到测试项目。此时**不要加包引用**，先跑
> `Select-String -Path LuBan.XTestProject\LuBan.UnitTestProject.csproj -Pattern ProjectReference` 确认它引了 `LuBan.Common`；引了就该能拿到，若仍失败再报需求方决策。

- [ ] **Step 3: 不提交**（RED 状态单独提交会把 CI 弄红，等 Task 5 全绿后一起提）

---

## Task 5：实现解析器（GREEN）

**Files:**
- Create: `LuBan.Web.Core/AIAgent/LuBanAgentConfigResolver.cs`

- [ ] **Step 1: 写文件（带文件头，命名空间 `LuBan.Web.Core.AIAgent`）**

`using` 只需一行：`using LuBan.AIAgent.Configuration.Providers;`（`ProviderDefaults` 在此命名空间）。其余（`IConfiguration`、`ConfigurationBuilder`、`MemoryStream`、`Encoding`、`JsonDocument`、`NacosConfigUtil`、`DbRepository`、`DbConfig`、`LuBanOrm`、`CommonConst`）已由 `LuBan.Web.Core/GlobalUsing.cs` 全局引入。

```csharp
namespace LuBan.Web.Core.AIAgent;

/// <summary>
/// LuBan Agent 配置解析结果。
/// </summary>
public sealed class LuBanAgentResolution
{
    /// <summary>是否启用 Agent。</summary>
    public bool Enabled { get; init; }

    /// <summary>启用时实际用于注册的配置；未启用时为 null。</summary>
    public IConfiguration? Configuration { get; init; }

    /// <summary>配置来源：db / host（仅诊断用）。</summary>
    public string Source { get; init; } = "";

    /// <summary>未启用原因；启用时为 null。</summary>
    public string? DisabledReason { get; init; }
}

/// <summary>
/// LuBan Agent 配置解析：db_config 优先，回退宿主配置，两处都不完整则不启用。
/// </summary>
public static class LuBanAgentConfigResolver
{
    /// <summary>配置节名称。</summary>
    public const string SectionName = "LuBanAgent";

    /// <summary>
    /// 解析 Agent 配置。
    /// </summary>
    /// <param name="configuration">宿主配置（appsettings + 环境变量）。</param>
    /// <param name="dbConfigReader">db_config 读取委托，参数为配置编码；缺省走 ORM。</param>
    public static LuBanAgentResolution Resolve(
        IConfiguration configuration,
        Func<string, string?>? dbConfigReader = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? raw;
        try
        {
            raw = (dbConfigReader ?? DefaultDbReader)(CommonConst.SysLuBanAgentCode);
        }
        catch
        {
            // 数据库故障不该连带禁用 Agent，按「没有记录」处理
            raw = null;
        }

        return string.IsNullOrWhiteSpace(raw)
            ? ResolveFromHost(configuration)
            : ResolveFromDb(configuration, raw);
    }

    /// <summary>
    /// 校验配置完整性。合法返回 null，否则返回中文原因。
    /// </summary>
    public static string? Validate(LuBanAgentStartupOptions? options)
    {
        if (options is null) return "未找到 LuBanAgent 配置";
        if (options.Models is null) return "缺少 LuBanAgent:Models 配置";

        var provider = options.Models.Provider;
        if (string.IsNullOrWhiteSpace(provider)) return "缺少 Models.Provider";
        if (!ProviderDefaults.ProviderIds.Any(p => string.Equals(p, provider, StringComparison.OrdinalIgnoreCase)))
            return $"Provider '{provider}' 不在框架已知列表内（可用：{string.Join(", ", ProviderDefaults.ProviderIds)}）";

        if (string.IsNullOrWhiteSpace(options.Models.Model)) return "缺少 Models.Model";
        if (string.IsNullOrWhiteSpace(options.Models.ApiKey)) return "缺少 Models.ApiKey";

        var baseUrl = options.Models.BaseUrl;
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return "Models.BaseUrl 不是合法的 http(s) 地址";
        }

        // DefaultModel 缺失会让 AgentHost 每次请求都抛「未选择模型」，必须在启动期拦下
        if (string.IsNullOrWhiteSpace(options.DefaultModel)) return "缺少 DefaultModel";

        return null;
    }

    private static LuBanAgentResolution ResolveFromDb(IConfiguration configuration, string raw)
    {
        var json = NormalizeDbJson(raw, out var jsonError);
        if (json is null)
            return Disabled("db", jsonError!);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        // 后加的源优先：db 覆盖宿主同名键，异键互补；Build() 会立即 Load，故 stream 可在 using 内释放
        var merged = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddJsonStream(stream)
            .Build();

        var reason = Validate(merged.GetSection(SectionName).Get<LuBanAgentStartupOptions>());
        return reason is null
            ? new LuBanAgentResolution { Enabled = true, Configuration = merged, Source = "db" }
            : Disabled("db", reason);
    }

    private static LuBanAgentResolution ResolveFromHost(IConfiguration configuration)
    {
        // 按 §0 方案 B：宿主配置经 NacosConfigUtil.Read 读取（Enable=false 时走 ConfigUtil → 宿主 IConfiguration）
        var options = NacosConfigUtil.Read<LuBanAgentStartupOptions>(SectionName);
        var reason = Validate(options);
        return reason is null
            ? new LuBanAgentResolution { Enabled = true, Configuration = configuration, Source = "host" }
            : Disabled("host", reason);
    }

    /// <summary>
    /// 容忍两种 db 写法：直接是 LuBanAgent 节对象，或已包一层 {"LuBanAgent":{...}}。
    /// </summary>
    private static string? NormalizeDbJson(string raw, out string? error)
    {
        error = null;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                error = $"db_config 中 {CommonConst.SysLuBanAgentCode} 的 Value 必须是 JSON 对象";
                return null;
            }

            if (doc.RootElement.TryGetProperty(SectionName, out _)) return raw;

            return "{\"" + SectionName + "\":" + raw + "}";
        }
        catch (JsonException ex)
        {
            error = $"db_config 中 {CommonConst.SysLuBanAgentCode} 的 Value 不是合法 JSON：{ex.Message}";
            return null;
        }
    }

    private static LuBanAgentResolution Disabled(string source, string reason)
        => new() { Enabled = false, Source = source, DisabledReason = reason };

    private static string? DefaultDbReader(string code)
    {
        // 库未初始化时 InitDataBaseOrm 已让进程退出，这里只做防御
        if (LuBanOrm.SqlSugarScope is null) return null;
        return new DbRepository<DbConfig>().GetFirst(q => q.Code == code && q.IsDelete == false)?.Value;
    }
}
```

**实现要点（别改）：**
- 宿主路径按 **§0 方案 B** 用 `NacosConfigUtil.Read<LuBanAgentStartupOptions>(SectionName)`，**不要**改成读 `configuration` 参数（虽然运行期等价，但那是方案 A，已被否决）。
- `GetFirst` 而不是 `First`：`LuBan.Orm/BaseRepository.cs:605` 明确"不存在返回 null"。`SmsConfigResolver` 用 `First` 是既有写法，不复刻。
- `new Uri(...)` 绝不能在这里替代 `Uri.TryCreate`：`AddLuBanAgentOpenAI` 的 `new Uri(baseUrl)` 在 `IChatClient` 工厂 lambda 里，非法值是**首个请求**才抛，启动期是唯一能提前拦住的地方。
- `Resolve` 外层 try/catch 只包**读库**，不包解析与校验。解析/校验的失败必须走「不启用」正常分支，不能被当成异常吞掉（否则日志里没有具体原因）。
- `ProviderIds` 上用 `Any + string.Equals(OrdinalIgnoreCase)`，不要用 `Contains(x, StringComparer.OrdinalIgnoreCase)`（`IReadOnlyList<string>` 上重载解析容易踩歧义）。

- [ ] **Step 2: 跑单测到全绿**

```powershell
dotnet build LuBan.Web.Core\LuBan.Web.Core.csproj -p:RestoreIgnoreFailedSources=true
dotnet test LuBan.XTestProject\LuBan.UnitTestProject.csproj --filter "FullyQualifiedName~LuBanAgentConfigResolverUnitTest"
```

期望：`Passed!` 12 个用例全过，退出码 0。

- [ ] **Step 3: 若有用例红，按原因定位（不要改测试迎合实现）**

| 现象 | 大概率原因 |
|---|---|
| `Resolve_DbValidJson_EnabledAndDbWins` 取到 `host-model` | 配置源顺序反了，`AddConfiguration(configuration)` 必须在 `AddJsonStream` **之前** |
| `Resolve_DbInvalidJson_DisabledAndNoFallback` 反而 Enabled | `NormalizeDbJson` 的 catch 没兜住，或 `ResolveFromDb` 里又回退了宿主 |
| `Resolve_NoDbRecord_HostValid_EnabledFromHost` 的 `AreSame` 失败 | `ResolveFromHost` 返回了 `merged` 而不是入参 `configuration` |
| `Resolve_NoReader_UsesDefaultDbReaderWithoutThrowing` 抛异常 | `DefaultDbReader` 少了 `SqlSugarScope is null` 防御 |
| 全部失败且原因是「未找到 LuBanAgent 配置」 | `ResolveWithHost` 里忘了 `ConfigUtil.InitConfigUtil(host)`，或 `SectionName` / DTO 属性名拼错 |

- [ ] **Step 4: 提交（测试 + 实现一起，此时才绿）**

```powershell
git add LuBan.Web.Core/AIAgent/LuBanAgentConfigResolver.cs LuBan.XTestProject/LuBanAgentConfigResolverUnitTest.cs
git commit -m "新增 Agent 配置解析与校验及单元测试"
```

---

## Task 6：把注册接进 `StartupUtil.ConfigureServices`

> **实施定稿（2026-10-28）**：注册段最终提取到 `LuBan.Web.Core/AIAgent/AIAgentExtension.cs` 的
> `AddAIAgent(configuration)` 扩展方法，`StartupUtil` 只保留一行 `services.AddAIAgent(configuration);`。
> 代码与本任务的下列内联写法逐行等价（多一条绿色「正在初始化 AIAgent」日志），
> 故 Step 1 的 `using LuBan.AIAgent;` 改由扩展方法所在项目提供，`StartupUtil.cs` 不再需要。
> 选择提取而非内联的原因：`StartupUtil` 已达千行，可选能力的注册逻辑内联会继续膨胀。

**Files:**
- Modify: `LuBan.Web.Core/Utils/StartupUtil.cs`
- Add: `LuBan.Web.Core/AIAgent/AIAgentExtension.cs`

- [ ] **Step 1: 文件顶部补 using**

在 `namespace LuBan.Web.Core.Utils;` **上方**（第 24 行位置）加一行：

```csharp
using LuBan.AIAgent;
```

> 必须补：`AddLuBanAgent*` 是 `LuBan.AIAgent` 命名空间下的扩展方法，`StartupUtil.cs` 目前一个 using 都没有（全靠全局），不补会解析不到。**不要**改成 `global using`，避免 `LuBan.AIAgent` 的类型污染整个 Web.Core。

- [ ] **Step 2: 在 `services.InitSms();` 之后、`services.AddServiceCache();` 之前插入注册段**

插入位置：当前 L112 `services.InitSms();` 与 L114 `//配置缓存` 之间。

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

**为什么整段必须 try/catch：** `ConfigureServices` 外层是 `catch → Environment.Exit(-1)`（同文件 L124-130）。任何从 Agent 冒出的异常都会直接杀掉站点，违反「配置错误不得导致启动失败」。

**为什么用 `ConsoleUtil` 不用 `Logger`：** 此刻静态 `Logger` 还没挂上 `loggerFactory`（`Logger.SetLogger` 在 `CreateWebApplication` 里 `Build()` 之后才调）。`InitDataBaseOrm` 内同款注释。

**为什么注册顺序必须是 Defaults → Host → OpenAI：** 与 `Program.cs` 原实现一致；`AddLuBanAgentDefaults` 需要 `IChatClient` 最终由 OpenAI 提供，顺序反了会缺依赖。

- [ ] **Step 3: 编译验证**

```powershell
dotnet build LuBan.Web.Core\LuBan.Web.Core.csproj -p:RestoreIgnoreFailedSources=true
```

期望：退出码 0。

- [ ] **Step 4: 提交**

```powershell
git add LuBan.Web.Core/Utils/StartupUtil.cs
git commit -m "Web 核心启动时按配置注册 Agent"
```

---

## Task 7：`WebApplication1/Program.cs` 改为标准启动

**Files:**
- Modify: `WebApplication1/Program.cs`（整文件重写）

- [ ] **Step 1: 重写文件**

保留原 23 行文件头，在"修改标记"里补一条本次修改（时间 `2026/10/28`、描述 `改用框架标准启动，Agent 注册移至 Web 核心`），然后把正文整体替换为：

```csharp
WebApp.RunWebHost(args);
```

- 删掉 `using LuBan.AIAgent;` / `using LuBan.AIAgent.Hosting;` / `using LuBan.AIAgent.Sessions;` 三行。
- 删掉 `builder` / 三行 `AddLuBanAgent*` / `app.MapPost("/agent/minimal", ...)` / `app.Run()`。
- 不需要补任何 `using`：`WebApplication1/GlobalUsing.cs:16` 已有 `global using LuBan.Web.Core;`，`WebApp` 在 `LuBan.Web.Core`。

- [ ] **Step 2: 确认已删干净**

```powershell
Select-String -Path WebApplication1\Program.cs -Pattern 'AddLuBanAgent|MapPost|agent/minimal'
```

期望：无输出。

- [ ] **Step 3: 编译验证**

```powershell
dotnet build WebApplication1\WebApplication1.csproj -p:RestoreIgnoreFailedSources=true
```

期望：退出码 0。

> `WebApplication1/Nuget.Config` 指向已下线的 `https://dev.coding.net/...`，不加 `-p:RestoreIgnoreFailedSources=true` 会 NU1301。这不是本次引入的问题。

- [ ] **Step 4: 提交**

```powershell
git add WebApplication1/Program.cs
git commit -m "示例站改用框架标准启动"
```

---

## Task 8：控制器承载示例端点

**Files:**
- Create: `WebApplication1/Models/Entities/AgentChatInput.cs`
- Create: `WebApplication1/Controllers/Default/AgentController.cs`

- [ ] **Step 1: 建入参 DTO（带文件头，命名空间 `WebApplication1.Models.Entities`）**

```csharp
namespace WebApplication1.Models.Entities;

/// <summary>
/// Agent 对话入参。
/// </summary>
public class AgentChatInput
{
    /// <summary>
    /// 用户输入。
    /// </summary>
    public string Prompt { get; set; } = "";
}
```

> **不要**加 `[Required]`：`LuBan.Web.Core/AspNetCore/ApiConfiguration.cs:47` 把 `SuppressModelStateInvalidFilter` 置为 true，模型校验过滤器整体关闭，加了也不生效。必填校验在控制器里显式做。

- [ ] **Step 2: 建控制器（带文件头，命名空间 `WebApplication1.Controllers.Default`）**

```csharp
using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Sessions;

using System.Text;

namespace WebApplication1.Controllers.Default;

/// <summary>
/// Agent 示例接口（免登录）。
/// </summary>
[Route("api/agent")]
[AllowAnonymous, AllowAccess]
public class AgentController : BaseApiController
{
    /// <summary>
    /// 一轮对话，返回模型输出的纯文本。
    /// </summary>
    [DisplayName("对话"), HttpPost("chat")]
    public async Task<IActionResult> ChatAsync([FromBody] AgentChatInput? input, CancellationToken cancellationToken)
    {
        // 全局模型校验过滤器被关闭，必须自己判空
        if (string.IsNullOrWhiteSpace(input?.Prompt))
            return StatusCode(StatusCodes.Status400BadRequest, ErrorResult("prompt 不能为空", 400));

        // 用 GetService 而不是构造注入：未启用时控制器仍能构造，返回 503 而非 500
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

- **与被删的 `/agent/minimal` 严格等价**：每次请求新建会话、不提供 `SessionId` 复用、只拼 `TextDeltaEvent`。不要"顺手"加会话复用参数。
- `AgentChatInput?` 必须可空：项目开了 `<Nullable>enable</Nullable>`，body 缺失时确实为 null。
- 会话串号（`ISessionManager` 默认实现是 Singleton，`SetCurrentSessionAsync` 是进程级）是原端点就有的示例级缺陷，**只搬不改**。

- [ ] **Step 3: 编译验证**

```powershell
dotnet build WebApplication1\WebApplication1.csproj -p:RestoreIgnoreFailedSources=true
```

期望：退出码 0。若报 `StatusCodes` 找不到，补 `using Microsoft.AspNetCore.Http;`（Web SDK 通常隐式提供，不用先加）。

- [ ] **Step 4: 提交**

```powershell
git add WebApplication1/Models/Entities/AgentChatInput.cs WebApplication1/Controllers/Default/AgentController.cs
git commit -m "示例 Agent 端点改为控制器承载"
```

---

## Task 9：README 中英同步

**Files:**
- Modify: `LuBan.Web.Core/README.md`
- Modify: `LuBan.Web.Core/README.en.md`

- [ ] **Step 1: 中文 README —— 在 `## 配置`（L58）的 appsettings 代码块之后、"---" 分隔线（L83）之前，追加两个小节**

```markdown
### LuBan Agent 接入

框架在标准启动链（`WebApp.RunWebHost`）中会自动尝试注册 LuBan Agent，配置按以下顺序解析：

1. `db_config` 表中 `Code = sys-luban-framework:aiagent` 的记录，`Value` 为与 `LuBanAgent` 节结构一致的 JSON；
2. 上述记录不存在时，回退宿主配置（`appsettings.json` 的 `LuBanAgent` 节、环境变量）；
3. 两处都没有、或内容不完整/不合法 → **不启用**，站点照常启动，控制台输出一条黄字原因。

```json
{
  "LuBanAgent": {
    "WorkspaceRoot": "D:\\work\\agent",
    "DefaultModel": "deepseek:deepseek-chat",
    "Models": {
      "Provider": "deepseek",
      "Model": "deepseek-chat",
      "ApiKey": "sk-***"
    }
  }
}
```

校验口径：`Models.Provider` 必须在框架已知 Provider 列表内（大小写不敏感）、`Models.Model` / `Models.ApiKey` / `DefaultModel` 非空、`Models.BaseUrl` 若给出必须是合法 http(s) 地址。

关闭方式：删除 `LuBanAgent` 节且不配置 `db_config` 记录即可（不存在"只关一半"的开关）。Agent 初始化失败只降级为不启用，不会导致 Web 应用启动失败。
```

- [ ] **Step 2: 英文 README —— 在 `## Configuration`（L58）的 appsettings 代码块之后追加对应小节**

```markdown
### LuBan Agent Integration

The standard startup chain (`WebApp.RunWebHost`) tries to register LuBan Agent automatically. Configuration is resolved in this order:

1. the `db_config` record with `Code = sys-luban-framework:aiagent`, whose `Value` is a JSON object shaped like the `LuBanAgent` section;
2. when that record is absent, the host configuration (`LuBanAgent` section in `appsettings.json`, environment variables);
3. neither present, or present but incomplete/invalid → **the agent stays disabled**, the web app still starts, and one yellow reason line is printed to the console.

```json
{
  "LuBanAgent": {
    "WorkspaceRoot": "D:\\work\\agent",
    "DefaultModel": "deepseek:deepseek-chat",
    "Models": {
      "Provider": "deepseek",
      "Model": "deepseek-chat",
      "ApiKey": "sk-***"
    }
  }
}
```

Validation: `Models.Provider` must be one of the built-in providers (case-insensitive), `Models.Model` / `Models.ApiKey` / `DefaultModel` must be non-empty, and `Models.BaseUrl` — when supplied — must be a valid http(s) URL.

To disable: remove the `LuBanAgent` section and leave no `db_config` record (there is no half-on switch). A failing agent initialization degrades to "disabled" and never prevents the web app from starting.
```

- [ ] **Step 3: 检查两份结构对称**

```powershell
Select-String -Path LuBan.Web.Core\README.md -Pattern '^### LuBan Agent'
Select-String -Path LuBan.Web.Core\README.en.md -Pattern '^### LuBan Agent'
```

期望：各输出 1 行。

> 落文档时避免出现真实内网域名/密钥（`LubanAgent.Tests` 有禁用值扫描用例）。示例里用 `sk-***` 与通用域名。

- [ ] **Step 4: 提交**

```powershell
git add LuBan.Web.Core/README.md LuBan.Web.Core/README.en.md
git commit -m "Web 核心文档补充 Agent 配置说明"
```

---

## Task 10：整体验收

### 10.1 构建（三处单项目，看退出码）

```powershell
dotnet build LuBan.Common\LuBan.Common.csproj -p:RestoreIgnoreFailedSources=true
dotnet build LuBan.Web.Core\LuBan.Web.Core.csproj -p:RestoreIgnoreFailedSources=true
dotnet build WebApplication1\WebApplication1.csproj -p:RestoreIgnoreFailedSources=true
dotnet build LuBan.XTestProject\LuBan.UnitTestProject.csproj -p:RestoreIgnoreFailedSources=true
```

四条都必须退出码 0。整解决方案构建**不作为门槛**（`WebApplication1/Nuget.Config` 的失效源会让 restore 报 NU1301）。

### 10.2 单测

```powershell
dotnet test LuBan.XTestProject\LuBan.UnitTestProject.csproj --filter "FullyQualifiedName~LuBanAgentConfigResolverUnitTest"
dotnet test LuBan.XTestProject\LuBan.UnitTestProject.csproj
```

- 第一条：12 个用例全绿。
- 第二条：整包全量在这里跑不动（大量 Redis/微信/轻流/SignalR/RagFlow/Office 集成用例依赖真实服务，实测 30 分钟未跑完），**不作为本次门槛**。按文件粒度回归与本次相关的 4 个类即可：

```powershell
dotnet test LuBan.XTestProject\LuBan.UnitTestProject.csproj --no-build --filter "FullyQualifiedName~ConfigUtilUnitTest|FullyQualifiedName~LuBanAgentConfigResolverUnitTest|FullyQualifiedName~DefaultSeamsUnitTest|FullyQualifiedName~AgentHostRegistrationUnitTest"
```

允许的失败（均已确认为**既有**、与本次无关）：

| 用例 | 原因 |
|---|---|
| `ConfigUtilUnitTest.ConfigUtilTest` | `ConfigUtil.Read("HostingOptions")` 对一个**对象节**走 `configuration["HostingOptions"]` 下标取叶子值，必然返回 null，用例本身写错了。单独跑也红 |
| `AIAgentUnitTest.TestDependencyInjection` | 依赖 `IChatClient` 已注册（`AddLuBanAgentDefaults` 的前置） |
| `DefaultsHostUnitTest.Defaults_FactoryCreateAsync_DoesNotThrow` | 测试里 `using var sp = Build()` 同步 Dispose 根容器，而 `ConnectorInstanceHost` 只实现 `IAsyncDisposable`（`ConnectorServiceCollectionExtensions` 注册为单例），`factory.CreateAsync` 解析工具组时把它实例化即抛 `...type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.`。已在实施前基线 `15901fd` 的临时 worktree 单跑复现，与本次改动无关；生产宿主走 `WebApplication` 的异步释放，不受影响 |

出现其他新失败，逐条定位，不要直接宣布"环境问题"。

> 注意回归过滤类名是 `DefaultSeamsUnitTest`，不含 `DefaultsHostUnitTest`；两条都要跑，否则会漏掉上表最后一条既有失败。

### 10.3 静态检查

```powershell
# 1) Program.cs 已无 Agent 注册与内联端点
Select-String -Path WebApplication1\Program.cs -Pattern 'AddLuBanAgent|MapPost'

# 2) 框架源码厂商词扫描（全部应为 0 条）
Get-ChildItem -Path LuBan.* -Recurse -Include *.cs,*.md -File |
  Select-String -Pattern 'Yunxiao|Codeup|AliyunDevOps|云效'

# 3) 魔法字符串已收口
Select-String -Path LuBan.Web.Core\AIAgent\LuBanAgentConfigResolver.cs -Pattern 'sys-luban-framework:aiagent'
```

- 第 1 条：无输出。
- 第 2 条：无输出（`LuBan.*` 下不含厂商词）。
- 第 3 条：无输出（应走 `CommonConst.SysLuBanAgentCode`）。

### 10.4 手工冒烟（交给需求方实机执行，助手不跑）

前置：数据库可连（否则 `InitDataBaseOrm` 会 `Environment.Exit(-1)`，属既有行为，不是本次缺陷）；无 dev cert 时用 `--urls http://127.0.0.1:7000` 覆盖。

| # | 场景 | 预期 |
|---|---|---|
| 1 | `appsettings.json` 删掉 `LuBanAgent` 节，`db_config` 无记录 | 站点正常起；控制台一条黄字「LuBan Agent 未启用：未找到 LuBanAgent 配置」；其余路由（含 `/swagger`）正常 |
| 2 | 同上，`POST /api/agent/chat` `{"prompt":"hi"}` | HTTP 503，body 为 `Fail` 结构，包含「未启用」 |
| 3 | `{"prompt":"  "}` 或空 body | HTTP 400（不是 500，也不是 503） |
| 4 | 只配 `appsettings` 的合法 `LuBanAgent`（真实 ApiKey） | 站点起；`POST /api/agent/chat` 返回模型文本；控制台无「未启用」 |
| 5 | `db_config` 写合法记录（`Model` 用与 appsettings 不同的值） | 生效的是 db 值（可用返回内容或日志确认） |
| 6 | `db_config` 的 `Value` 改成 `{not json` | 不启用；日志原因含「不是合法 JSON」；**不回退**（appsettings 合法也不用） |
| 7 | `db_config` 的 `Provider` 改成 `no-such` | 不启用；日志含「不在框架已知列表内」 |
| 8 | `db_config` 的 `BaseUrl` 改成 `not-a-url` | 不启用；日志含「BaseUrl 不是合法的 http(s) 地址」 |
| 9 | `db_config` 删掉 `DefaultModel` | 不启用；日志含「缺少 DefaultModel」 |
| 10 | 篡改 db 记录为一条会导致解析期异常的怪值（如超深嵌套） | 站点仍能起（验证 try/catch 兜底），日志含「初始化失败，已跳过」 |

### 10.5 收尾

```powershell
git status --short     # 应只剩那两个不属于本次任务的 LuBan.AIAgent 改动
git log --oneline -8   # 检查 8 条提交信息（中文、无 -A 误带文件）
```

若 `git status` 里出现 `bin/`、`obj/`、`docs/` 之外的杂项，说明某次 `git add` 越界了，回头用 `git reset HEAD <path>` 摘掉再提交，**不要** `git checkout --` 覆盖用户改动。

---

## 4. 完整验收清单（对照 Spec §13）

- [ ] `Program.cs` 不再出现任何 `AddLuBanAgent*` 调用，也不再内联端点
- [ ] 删掉 db 记录与 appsettings 节后：站点正常启动、接口 503、日志有唯一一条明确原因
- [ ] 仅 appsettings 合法时接口可用；db 记录覆盖时能观测到 db 值生效
- [ ] 记录坏 / Provider 未知时不启用且不回退
- [ ] 新增单测全绿（12 条），四处构建退出码 0
- [ ] 框架源码厂商词扫描为 0
- [ ] Agent 相关代码抛异常时站点仍能启动

## 5. 已知边界（不在本次修，写在这里防止后来者踩）

| 边界 | 说明 |
|---|---|
| `NacosConfigUtil.Read` 与注册用配置**理论上可能分叉** | 校验走 `NacosConfigUtil.Read`（受 `Enable` 影响，读静态 `ConfigUtil._configuration`），注册用宿主 `IConfiguration`。当前 `Enable` 恒 false（`Set()` 只在 `IHostedService.StartAsync` 后调用，而插入点在 `builder.Build()` 前），两者同源等价。**将来若给 Web 启动加 Nacos 预加载，必须改成用同一份来源重建配置**，修改点就在 `ResolveFromHost`（方案 B 已被需求方拍板，此风险保留） |
| `Replace(IConfiguration)` 削弱 appsettings 热更新 | 新根由 `AddConfiguration(原配置) + AddJsonStream` 组成，`IOptionsMonitor` 走注册时捕获的 section 不受影响；受影响面只有运行期直注 `IConfiguration` 的服务（目前只有 `SessionRuntimeManager` 读一个 int 阈值）。示例站无热更新需求 |
| `ConfigUtil` / `NacosConfigUtil` 静态读源**看不到** db 覆盖值 | `ConfigUtil._configuration` 在 `ConfigureServices` 开头就绑成原始宿主配置，`Replace` 只改 DI 容器。合并配置是超集，其他节读取结果不变 |
| `LuBanAgentOptions.Models` 类型与配置形状不符 | 声明是 `Dictionary<string, ModelEndpointOptions>`，配置是扁平对象。当前无代码消费（`AddLuBanAgentOpenAI` 直读原始键）。本次不动 |
| `ISessionManager` 默认实现 Singleton → 会话串号 | 原 `/agent/minimal` 同样问题，示例站单并发使用。只搬不改 |
| `BuildProvider()` 无 `ValidateOnBuild` | 缺依赖只在首次请求暴露，故启动期校验必须做全（本设计的核心价值）。不要指望容器帮兜底 |
| Agent 依赖传染 | 引用 `LuBan.Web.Core` 的项目都会被动带上 `LuBan.AIAgent`（含 Playwright），nupkg 也新增该依赖。D2 已接受，发布说明需点明 |
| 路由变更 | `/agent/minimal` → `/api/agent/chat`。示例站无外部消费者 |
| 无运行期热更新 | 配置改了要重启 |