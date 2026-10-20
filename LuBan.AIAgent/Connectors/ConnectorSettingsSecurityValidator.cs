/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： ConnectorSettingsSecurityValidator
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/20
*描述：连接器参数明文凭据校验 —— 疑似令牌一律走 $credential 引用，禁止落盘
*
*****************************************************************************/
using LuBan.AIAgent.MCP;

namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器参数安全校验：确保<b>凭据不落盘</b>。
/// </summary>
/// <remarks>
/// <para>
/// 规则与 <see cref="McpConfigSecurityValidator"/> 同口径，但作用于连接器的
/// <c>settings</c> 键值对：键名疑似承载令牌（<c>token</c> / <c>secret</c> / <c>password</c>
/// / <c>pat</c> / <c>apikey</c> / <c>accesskey</c> / <c>credential</c>）时，
/// 其值必须是凭据引用 <c>$credential:&lt;凭据键&gt;</c>。
/// </para>
/// <para>
/// 该校验是<b>写入前</b>门禁，由连接器配置写入器与连接器管理工具共用。
/// </para>
/// </remarks>
public static class ConnectorSettingsSecurityValidator
{
    /// <summary>参数键名里出现这些片段即视为「敏感参数」，其值必须是凭据引用。</summary>
    private static readonly string[] SensitiveSettingHints =
    {
        "token", "secret", "password", "passwd", "pat", "api_key", "apikey", "accesskey", "credential"
    };

    /// <summary>
    /// 校验连接器参数是否允许落盘。
    /// </summary>
    /// <param name="settings">待写入的参数（键值对）。</param>
    /// <returns>合规返回 <c>null</c>；否则返回中文错误说明。</returns>
    public static string? Validate(IEnumerable<KeyValuePair<string, string?>>? settings)
    {
        if (settings is null) return null;

        foreach (var (key, value) in settings)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (!LooksSensitive(key)) continue;
            if (CredentialValueResolver.IsReference(value)) continue;

            return $"参数 '{key}' 疑似承载令牌，必须使用凭据引用（$credential:<凭据键>），不得写明文。";
        }

        return null;
    }

    /// <summary>
    /// 判断某个参数键是否疑似敏感（含敏感片段）。
    /// </summary>
    /// <param name="key">参数键。</param>
    /// <returns>疑似敏感返回 <c>true</c>。</returns>
    public static bool LooksSensitive(string key)
        => SensitiveSettingHints.Any(hint => key.Contains(hint, StringComparison.OrdinalIgnoreCase));
}