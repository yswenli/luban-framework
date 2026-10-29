/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.AIAgent
*文件名： LuBanAgentConfigResolver
*版本号： V1.0.0.0
*唯一标识：db88b16a-59f7-4870-8d8b-d858c8179af2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：LuBan Agent 配置解析与校验（db_config 优先，回退宿主配置）
*
*****************************************************************************/

namespace LuBan.Web.Core.AIAgent;

/// <summary>
/// LuBan Agent 配置解析结果。
/// </summary>
public sealed class LuBanAgentResolution
{
    /// <summary>
    /// 是否启用 Agent
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 启用时实际用于注册的配置；未启用时为 null
    /// </summary>
    public IConfiguration? Configuration { get; init; }

    /// <summary>
    /// 配置来源：db / host（仅诊断用）
    /// </summary>
    public string Source { get; init; } = "";

    /// <summary>
    /// 未启用原因；启用时为 null
    /// </summary>
    public string? DisabledReason { get; init; }
}

/// <summary>
/// LuBan Agent 配置解析：db_config 优先，回退宿主配置，两处都不完整则不启用。
/// </summary>
public static class LuBanAgentConfigResolver
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "LuBanAgent";

    /// <summary>
    /// 解析 Agent 配置
    /// </summary>
    /// <param name="configuration">宿主配置（appsettings + 环境变量）</param>
    /// <param name="dbConfigReader">db_config 读取委托，参数为配置编码；缺省走 ORM</param>
    /// <returns>解析结果</returns>
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
    /// 校验配置完整性
    /// </summary>
    /// <param name="options">待校验配置</param>
    /// <returns>合法返回 null，否则返回中文原因</returns>
    /// <remarks>
    /// <para><c>Models</c> 为数组，每项是一个 <c>provider:model</c> 组合；同一 Provider 可配置多个模型，
    /// 但 <c>ApiKey</c> / <c>BaseUrl</c> 必须一致（运行期每个 Provider 只建一个客户端）。</para>
    /// <para><c>Provider</c> 不限于内置表：内置项可省略 <c>BaseUrl</c>（有内置端点），表外项（自建网关、代理、
    /// 未收录的厂商）只要显式配了合法 <c>BaseUrl</c> 即视为自定义 Provider 放行。</para>
    /// <para><c>DefaultModel</c> 可省略，省略时由 <see cref="CreateEffectiveConfiguration"/> 取数组第一项；
    /// 一旦配置，则必须是合法 <c>provider:model</c> 且能在数组中找到对应项。</para>
    /// </remarks>
    public static string? Validate(LuBanAgentStartupOptions? options)
    {
        if (options is null) return "未找到 LuBanAgent 配置";

        var models = options.Models;
        if (models is null || models.Count == 0) return "缺少 LuBanAgent:Models 配置";

        for (var i = 0; i < models.Count; i++)
        {
            var item = models[i];
            var label = $"Models[{i}]";
            if (item is null) return $"{label} 为空";

            var provider = item.Provider?.Trim();
            if (string.IsNullOrWhiteSpace(provider)) return $"缺少 {label}.Provider";

            // 内置 Provider 有内置端点，可省略 BaseUrl；表外 Provider（自建网关/代理）必须显式给出，
            // 否则 OpenAI SDK 会静默落到 https://api.openai.com/v1/，启动期是唯一能提前拦住的地方
            var isBuiltInProvider = ProviderDefaults.ProviderIds
                .Any(p => string.Equals(p, provider, StringComparison.OrdinalIgnoreCase));
            if (!isBuiltInProvider && string.IsNullOrWhiteSpace(item.BaseUrl))
                return $"{label}.Provider '{provider}' 是自定义 Provider，必须显式配置 BaseUrl（内置可用：{string.Join(", ", ProviderDefaults.ProviderIds)}）";

            if (string.IsNullOrWhiteSpace(item.Model)) return $"缺少 {label}.Model";
            if (string.IsNullOrWhiteSpace(item.ApiKey)) return $"缺少 {label}.ApiKey";

            // 非法 BaseUrl 会在解析 IChatClient 时才抛，启动期是唯一能提前拦住的地方
            if (!IsValidBaseUrl(item.BaseUrl))
                return $"{label}.BaseUrl 不是合法的 http(s) 地址";
        }

        // 同一 Provider 允许多个模型，但凭据/端点必须一致，否则无法只建一个客户端
        foreach (var group in models.GroupBy(m => m.Provider!.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            var first = group.First();
            foreach (var item in group.Skip(1))
            {
                if (!SameCredentials(first, item))
                    return $"Models 中 Provider '{first.Provider}' 出现多条但 ApiKey/BaseUrl 不一致";
            }
        }

        // DefaultModel 可省略：省略即取数组第一项（AgentHost 仍要求必须有模型）
        var defaultModel = options.DefaultModel?.Trim();
        if (string.IsNullOrEmpty(defaultModel)) return null;

        var idx = defaultModel.IndexOf(':');
        if (idx <= 0 || idx >= defaultModel.Length - 1)
            return $"DefaultModel '{defaultModel}' 格式应为 provider:model";

        var dmProvider = defaultModel[..idx];
        var dmModel = defaultModel[(idx + 1)..];
        if (!models.Any(m => string.Equals(m.Provider?.Trim(), dmProvider, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(m.Model?.Trim(), dmModel, StringComparison.Ordinal)))
        {
            return $"DefaultModel '{defaultModel}' 未在 Models 数组中找到对应项";
        }

        return null;
    }

    /// <summary>
    /// 生成运行期实际注册用的配置：相对 <c>WorkspaceRoot</c> / <c>Attachments:ContentRoot</c> 归一化为绝对路径，缺省 <c>DefaultModel</c> 补成第一项。
    /// </summary>
    /// <param name="configuration">已通过校验的配置（宿主配置或 db 合并后的配置）</param>
    /// <param name="contentRootPath">站点内容根；为空时按 <see cref="AppContext.BaseDirectory"/> 解析相对路径</param>
    /// <returns>需要调整时返回叠加后的新配置，否则原样返回入参</returns>
    public static IConfiguration CreateEffectiveConfiguration(IConfiguration configuration, string? contentRootPath = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var rawRoot = configuration[$"{SectionName}:WorkspaceRoot"];
        var resolvedRoot = ResolveWorkspaceRoot(rawRoot, contentRootPath);
        if (resolvedRoot is not null && !string.Equals(resolvedRoot, rawRoot, StringComparison.Ordinal))
            overrides[$"{SectionName}:WorkspaceRoot"] = resolvedRoot;

        var rawContentRoot = configuration[$"{SectionName}:Attachments:ContentRoot"];
        var resolvedContentRoot = ResolveWorkspaceRoot(rawContentRoot, contentRootPath);
        if (resolvedContentRoot is not null && !string.Equals(resolvedContentRoot, rawContentRoot, StringComparison.Ordinal))
            overrides[$"{SectionName}:Attachments:ContentRoot"] = resolvedContentRoot;
        else if (rawContentRoot is { Length: 0 })
            // 链式配置（AddConfiguration）会把空串当缺失吞掉，显式保留以维持「留空=不启用站点相对解析」语义
            overrides[$"{SectionName}:Attachments:ContentRoot"] = "";

        var rawDefault = configuration[$"{SectionName}:DefaultModel"];
        if (string.IsNullOrWhiteSpace(rawDefault))
        {
            var firstProvider = configuration[$"{SectionName}:Models:0:Provider"]?.Trim();
            var firstModel = configuration[$"{SectionName}:Models:0:Model"]?.Trim();
            if (!string.IsNullOrWhiteSpace(firstProvider) && !string.IsNullOrWhiteSpace(firstModel))
                overrides[$"{SectionName}:DefaultModel"] = $"{firstProvider}:{firstModel}";
        }

        if (overrides.Count == 0) return configuration;

        // 后加的源优先：只覆盖 WorkspaceRoot / DefaultModel / Attachments:ContentRoot，其余键（含 db 合并结果）保持不变
        return new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(overrides)
            .Build();
    }

    /// <summary>
    /// 解析工作区根目录：绝对路径原样规范化；相对路径以站点内容根为基准（非 Web 宿主回退到程序目录）。
    /// </summary>
    /// <remarks>
    /// Windows 上以 <c>/</c> 开头的值会被 <see cref="Path.IsPathRooted"/> 判为绝对路径（<c>/aiagent</c> → 当前盘根），
    /// 要落到站点内应写 <c>aiagent</c> 或 <c>./aiagent</c>。
    /// </remarks>
    /// <param name="workspaceRoot">配置值</param>
    /// <param name="contentRootPath">站点内容根；为空时用 <see cref="AppContext.BaseDirectory"/></param>
    /// <returns>绝对路径；配置为空时返回 null</returns>
    public static string? ResolveWorkspaceRoot(string? workspaceRoot, string? contentRootPath = null)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot)) return null;

        var root = workspaceRoot.Trim();
        if (Path.IsPathRooted(root)) return Path.GetFullPath(root);

        var basePath = string.IsNullOrWhiteSpace(contentRootPath)
            ? AppContext.BaseDirectory
            : contentRootPath!.Trim();

        return Path.GetFullPath(Path.Combine(basePath, root));
    }

    private static bool IsValidBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return true;

        return Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool SameCredentials(LuBanAgentModelsOptions a, LuBanAgentModelsOptions b)
        => string.Equals(a.ApiKey?.Trim(), b.ApiKey?.Trim(), StringComparison.Ordinal)
           && string.Equals(NormalizeUrl(EffectiveBaseUrl(a)), NormalizeUrl(EffectiveBaseUrl(b)), StringComparison.OrdinalIgnoreCase);

    private static string? EffectiveBaseUrl(LuBanAgentModelsOptions item)
        => string.IsNullOrWhiteSpace(item.BaseUrl)
            ? ProviderDefaults.GetDefaultBaseUrl(item.Provider?.Trim() ?? "")
            : item.BaseUrl.Trim();

    private static string NormalizeUrl(string? url) => url?.Trim().TrimEnd('/') ?? string.Empty;

    private static LuBanAgentResolution ResolveFromDb(IConfiguration configuration, string raw)
    {
        var json = NormalizeDbJson(raw, out var jsonError);
        if (json is null)
            return Disabled("db", jsonError!);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        // 后加的源优先：db 覆盖宿主同名键、异键互补；Build() 会立即 Load，故 stream 可在 using 内释放
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
        // Nacos 未启用时等价于宿主 IConfiguration；注册用的也是同一份宿主配置
        var options = NacosConfigUtil.Read<LuBanAgentStartupOptions>(SectionName);
        var reason = Validate(options);
        return reason is null
            ? new LuBanAgentResolution { Enabled = true, Configuration = configuration, Source = "host" }
            : Disabled("host", reason);
    }

    /// <summary>
    /// 容忍两种 db 写法：直接是 LuBanAgent 节对象，或已包一层 {"LuBanAgent":{...}}
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