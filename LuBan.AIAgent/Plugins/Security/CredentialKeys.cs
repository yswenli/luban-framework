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
*描述：已知凭据键目录与环境变量映射（PRD §3.7 / §4.9）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 已知凭据键目录：插件 <c>credential</c> 声明与 <see cref="ICredentialStore"/> 的键约定。
/// </summary>
/// <remarks>
/// PRD 中出现两种写法：§3.7 用 <c>yunxiao_pat</c>，§4.9 PAT 时序表用 <c>codeup:pat</c>。
/// 二者视为同一凭据，统一由 <see cref="Normalize"/> 归一化为 <see cref="YunxiaoPat"/>，
/// 避免「同一凭据两个键」导致声明与取用对不上。
/// </remarks>
public static class CredentialKeys
{
    /// <summary>
    /// 云效个人访问令牌（PAT）的规范键名 —— 即 <c>connector:yunxiao:token</c>。
    /// </summary>
    /// <remarks>
    /// 自 Q15 起连接器令牌统一泛化为 <c>connector:&lt;实例名&gt;:token</c>（见 <see cref="ForConnectorToken"/>）。
    /// 常量名 <c>YunxiaoPat</c> 保留为<b>兼容别名</b>，旧键拼写（<c>yunxiao_pat</c> / <c>codeup:pat</c> 等）
    /// 仍由 <see cref="Normalize"/> 归一化到本键，历史数据由 <see cref="GetLegacyKeyAliases"/> 支持回退读取。
    /// </remarks>
    public const string YunxiaoPat = "connector:yunxiao:token";

    /// <summary>云效组织 ID（非敏感，但与会话/连接配置同源存放）。</summary>
    public const string YunxiaoOrganizationId = "yunxiao_organization_id";

    /// <summary>
    /// 取连接器令牌凭据的规范键名（Q15 泛化约定）。
    /// </summary>
    /// <param name="connectorName">连接器实例名（如 <c>yunxiao</c>）。</param>
    /// <returns>形如 <c>connector:&lt;实例名&gt;:token</c> 的键名（小写、去空白）。</returns>
    public static string ForConnectorToken(string connectorName)
        => $"connector:{(connectorName ?? string.Empty).Trim().ToLowerInvariant()}:token";

    /// <summary>
    /// 各规范键对应的<b>历史键拼写</b>表（仅用于旧数据回退读取与一次性迁移）。
    /// </summary>
    private static readonly Dictionary<string, string[]> LegacyKeyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        [YunxiaoPat] = ["yunxiao_pat", "yunxiao:pat", "codeup_pat", "codeup:pat"],
        [YunxiaoOrganizationId] = ["yunxiao_org_id", "yunxiao:org", "codeup_org_id", "codeup:org"]
    };

    /// <summary>
    /// 取指定键（任意别名写法）对应的历史键拼写列表。
    /// </summary>
    /// <remarks>
    /// 迁移后旧键仍被保留，回退读取保证「配置迁移未落盘（如只读文件）」时凭据不丢。
    /// </remarks>
    /// <param name="key">凭据键（任意别名写法）。</param>
    /// <returns>历史键拼写；无别名时返回空集合。</returns>
    public static IReadOnlyList<string> GetLegacyKeyAliases(string key)
        => LegacyKeyAliases.TryGetValue(Normalize(key), out var aliases) ? aliases : Array.Empty<string>();

    /// <summary>
    /// 把别名键归一化为规范键名（忽略大小写与首尾空白）。
    /// </summary>
    /// <param name="key">原始键名。</param>
    /// <returns>规范键名；未知键返回小写去空白后的原值。</returns>
    public static string Normalize(string key)
    {
        var normalized = (key ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            "yunxiao_pat" or "yunxiao:pat" or "codeup_pat" or "codeup:pat" => YunxiaoPat,
            "yunxiao_org_id" or "yunxiao:org" or "codeup_org_id" or "codeup:org" => YunxiaoOrganizationId,
            _ => normalized
        };
    }

    /// <summary>
    /// 取凭据键对应的环境变量名（供默认存储兜底读取，PRD §4.9）。
    /// </summary>
    /// <param name="key">凭据键。</param>
    /// <returns>环境变量名；无映射时返回 <c>null</c>。</returns>
    public static string? GetEnvironmentVariableName(string key) => Normalize(key) switch
    {
        YunxiaoPat => "LUBAN_YUNXIAO_PAT",
        YunxiaoOrganizationId => "LUBAN_YUNXIAO_ORGANIZATION_ID",
        _ => ConnectorTokenVariableOf(Normalize(key))
    };

    /// <summary>
    /// 把泛化连接器令牌键 <c>connector:&lt;实例名&gt;:token</c> 映射为
    /// <c>LUBAN_CONNECTOR_&lt;实例名&gt;_TOKEN</c>。
    /// </summary>
    /// <remarks>仅用于<b>非默认实例</b>（默认实例已由 <see cref="YunxiaoPat"/> 映射到历史变量名）。</remarks>
    /// <param name="normalizedKey">已归一化的凭据键。</param>
    /// <returns>环境变量名；不符合泛化约定时返回 <c>null</c>。</returns>
    private static string? ConnectorTokenVariableOf(string normalizedKey)
    {
        const string prefix = "connector:";
        const string suffix = ":token";

        if (!normalizedKey.StartsWith(prefix, StringComparison.Ordinal) ||
            !normalizedKey.EndsWith(suffix, StringComparison.Ordinal) ||
            normalizedKey.Length <= prefix.Length + suffix.Length)
            return null;

        var name = normalizedKey[prefix.Length..^suffix.Length].Replace('-', '_').Replace('.', '_');
        return string.IsNullOrWhiteSpace(name) ? null : $"LUBAN_CONNECTOR_{name.ToUpperInvariant()}_TOKEN";
    }
}