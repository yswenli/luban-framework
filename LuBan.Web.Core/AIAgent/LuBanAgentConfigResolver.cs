/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AIAgent
*文件名： LuBanAgentConfigResolver
*版本号： V1.0.0.0
*唯一标识：Agent 配置解析
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：LuBan Agent 配置解析与校验（db_config 优先，回退宿主配置）
*
*****************************************************************************/

using LuBan.AIAgent.Configuration.Providers;

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
            // 非法 BaseUrl 会在首次解析 IChatClient 时才抛，启动期是唯一能提前拦住的地方
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