/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： CredentialKeys
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：凭据键约定（厂商无关）—— 连接器令牌/字段键的规范拼写
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 凭据键约定：连接器令牌与字段键的规范拼写，以及通用环境变量映射。
/// </summary>
/// <remarks>
/// <para>
/// 本类<b>只</b>提供通用约定，<b>不含任何厂商知识</b>：规范键由「连接器实例名 + 字段」拼出，
/// 历史别名（如某个平台旧版的 <c>xxx_pat</c> 拼写）一律由宿主/产品实现
/// <see cref="ICredentialKeyAliases"/> 提供，框架不做假设。
/// </para>
/// <para>
/// 规范键形如 <c>connector:&lt;实例名&gt;:token</c>（令牌）或
/// <c>connector:&lt;实例名&gt;:&lt;字段&gt;</c>（其它字段，如 <c>organizationid</c>）。
/// </para>
/// </remarks>
public static class CredentialKeys
{
    /// <summary>连接器键前缀。</summary>
    public const string ConnectorPrefix = "connector:";

    /// <summary>连接器令牌字段名。</summary>
    public const string TokenField = "token";

    /// <summary>
    /// 取连接器令牌凭据的规范键名。
    /// </summary>
    /// <param name="connectorName">连接器实例名（如 <c>myconnector</c>）。</param>
    /// <returns>形如 <c>connector:&lt;实例名&gt;:token</c> 的键名（小写、去空白）。</returns>
    public static string ForConnectorToken(string connectorName)
        => $"{ConnectorPrefix}{Norm(connectorName)}:{TokenField}";

    /// <summary>
    /// 取连接器一般字段的规范键名。
    /// </summary>
    /// <param name="connectorName">连接器实例名。</param>
    /// <param name="field">字段名（如 <c>username</c> / <c>password</c> / <c>organizationid</c>）。</param>
    /// <returns>形如 <c>connector:&lt;实例名&gt;:&lt;字段&gt;</c> 的键名（小写、去空白）。</returns>
    public static string ForConnectorField(string connectorName, string field)
        => $"{ConnectorPrefix}{Norm(connectorName)}:{Norm(field)}";

    /// <summary>
    /// 键归一化：去首尾空白 + 转小写。
    /// </summary>
    /// <param name="key">原始键名。</param>
    /// <returns>归一化后的键名。</returns>
    /// <remarks>
    /// <b>不</b>做别名映射：厂商历史拼写的归一化由 <see cref="ICredentialKeyAliases"/> 负责。
    /// </remarks>
    public static string Normalize(string key) => Norm(key);

    /// <summary>
    /// 解析连接器规范键。
    /// </summary>
    /// <param name="key">凭据键（任意大小写）。</param>
    /// <param name="instanceName">连接器实例名。</param>
    /// <param name="field">字段名。</param>
    /// <returns>符合 <c>connector:&lt;实例名&gt;:&lt;字段&gt;</c> 约定时返回 <c>true</c>。</returns>
    public static bool TryParseConnectorKey(string? key, out string instanceName, out string field)
    {
        instanceName = string.Empty;
        field = string.Empty;

        var normalized = Norm(key);
        if (!normalized.StartsWith(ConnectorPrefix, StringComparison.Ordinal))
            return false;

        var remainder = normalized[ConnectorPrefix.Length..];
        var separator = remainder.IndexOf(':');
        if (separator <= 0 || separator >= remainder.Length - 1)
            return false;

        instanceName = remainder[..separator];
        field = remainder[(separator + 1)..];
        return true;
    }

    /// <summary>
    /// 取凭据键对应的环境变量名（供默认存储兜底读取）。
    /// </summary>
    /// <param name="key">凭据键。</param>
    /// <returns>环境变量名。</returns>
    /// <remarks>
    /// 通用映射：<c>connector:&lt;实例名&gt;:&lt;字段&gt;</c> →
    /// <c>LUBAN_CONNECTOR_&lt;实例名&gt;_&lt;字段&gt;</c>；其它键 → <c>LUBAN_&lt;键&gt;</c>
    /// （非字母数字一律转下划线、转大写）。厂商历史变量名（如 <c>LUBAN_LEGACY_PAT</c>）
    /// 由 <see cref="ICredentialKeyAliases.GetLegacyEnvironmentVariableName"/> 提供。
    /// </remarks>
    public static string GetEnvironmentVariableName(string key)
    {
        var normalized = Normalize(key);

        if (TryParseConnectorKey(normalized, out var instanceName, out var field))
            return $"LUBAN_CONNECTOR_{Sanitize(instanceName)}_{Sanitize(field)}";

        return $"LUBAN_{Sanitize(normalized)}";
    }

    private static string Norm(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string Sanitize(string value)
    {
        var chars = value.Select(c => char.IsLetterOrDigit(c) ? char.ToUpperInvariant(c) : '_').ToArray();
        return new string(chars);
    }
}