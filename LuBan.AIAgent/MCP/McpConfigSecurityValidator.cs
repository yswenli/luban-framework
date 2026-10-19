/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.MCP
*文件名： McpConfigSecurityValidator
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：MCP 配置明文凭据校验 —— 令牌一律走 $credential 引用，禁止落盘
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// MCP 配置安全校验：确保<b>凭据不落盘</b>。
/// </summary>
/// <remarks>
/// <para>
/// 规则：疑似承载令牌的<b>请求头</b>与<b>环境变量</b>，其值必须是凭据引用
/// <c>$credential:&lt;键&gt;</c>；URL 中禁止出现「user:password@host」形式的内嵌凭据。
/// </para>
/// <para>
/// 该校验是<b>写入前</b>门禁，由 MCP 管理工具与宿主设置页共用；不改变已存在配置的读取。
/// </para>
/// </remarks>
public static class McpConfigSecurityValidator
{
    /// <summary>请求头名里出现这些片段即视为「敏感头」，其值必须是凭据引用。</summary>
    private static readonly string[] SensitiveHeaderHints =
    {
        "authorization", "token", "secret", "api-key", "apikey", "password", "cookie"
    };

    /// <summary>环境变量名里出现这些片段即视为「敏感变量」，其值必须是凭据引用。</summary>
    private static readonly string[] SensitiveEnvironmentHints =
    {
        "token", "secret", "password", "pat", "api_key", "apikey", "credential"
    };

    /// <summary>
    /// 校验 MCP 配置是否允许落盘。
    /// </summary>
    /// <param name="config">待写入的配置。</param>
    /// <returns>合规返回 <c>null</c>；否则返回中文错误说明。</returns>
    public static string? Validate(McpServerConfig? config)
    {
        if (config is null) return "MCP 配置为空。";

        // 1) 敏感请求头必须是凭据引用
        foreach (var (name, value) in config.Headers)
        {
            if (string.IsNullOrWhiteSpace(name) || !LooksSensitive(name, SensitiveHeaderHints)) continue;
            if (!CredentialValueResolver.IsReference(value))
                return $"请求头 '{name}' 疑似承载令牌，必须使用凭据引用（$credential:<凭据键>），不得写明文。";
        }

        // 2) 敏感环境变量必须是凭据引用
        foreach (var (name, value) in config.Environment)
        {
            if (string.IsNullOrWhiteSpace(name) || !LooksSensitive(name, SensitiveEnvironmentHints)) continue;
            if (!CredentialValueResolver.IsReference(value))
                return $"环境变量 '{name}' 疑似承载令牌，必须使用凭据引用（$credential:<凭据键>），不得写明文。";
        }

        // 3) URL 不得内嵌凭据（https://user:pass@host/...）
        foreach (var candidate in EnumerateUrls(config))
        {
            if (candidate is not null && ContainsEmbeddedCredentials(candidate))
                return $"URL 中不得内嵌凭据（形如 https://用户名:密码@主机）：{Mask(candidate)}。请改用凭据引用。";
        }

        return null;
    }

    /// <summary>
    /// 判断某个名字是否疑似敏感（含敏感片段）。
    /// </summary>
    /// <param name="name">请求头名或环境变量名。</param>
    /// <param name="hints">敏感片段集合。</param>
    /// <returns>疑似敏感返回 <c>true</c>。</returns>
    private static bool LooksSensitive(string name, IReadOnlyList<string> hints)
        => hints.Any(hint => name.Contains(hint, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 枚举配置中出现 URL 的字段（Url / Command / Args）。
    /// </summary>
    /// <param name="config">MCP 配置。</param>
    /// <returns>候选 URL 字符串。</returns>
    private static IEnumerable<string?> EnumerateUrls(McpServerConfig config)
    {
        yield return config.Url;
        yield return config.Command;

        foreach (var arg in config.Args)
            yield return arg;
    }

    /// <summary>
    /// 判断 URL 是否含内嵌凭据（userinfo 段带冒号）。
    /// </summary>
    /// <param name="url">候选 URL。</param>
    /// <returns>含内嵌凭据返回 <c>true</c>。</returns>
    private static bool ContainsEmbeddedCredentials(string url)
    {
        var schemeIndex = url.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex < 0) return false;

        var afterScheme = url[(schemeIndex + 3)..];
        var atIndex = afterScheme.IndexOf('@');
        if (atIndex <= 0) return false;

        return afterScheme[..atIndex].Contains(':');
    }

    /// <summary>
    /// 掩码 URL 中的凭据段，避免错误信息回显敏感内容。
    /// </summary>
    /// <param name="url">原始 URL。</param>
    /// <returns>掩码后的 URL。</returns>
    private static string Mask(string url)
    {
        var schemeIndex = url.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex < 0) return url;

        var afterScheme = url[(schemeIndex + 3)..];
        var atIndex = afterScheme.IndexOf('@');
        if (atIndex <= 0) return url;

        return string.Concat(url.AsSpan(0, schemeIndex + 3), "***", afterScheme.AsSpan(atIndex));
    }
}