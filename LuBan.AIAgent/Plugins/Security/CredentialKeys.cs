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
    /// <summary>云效个人访问令牌（PAT）的规范键名。</summary>
    public const string YunxiaoPat = "yunxiao_pat";

    /// <summary>云效组织 ID（非敏感，但与会话/连接配置同源存放）。</summary>
    public const string YunxiaoOrganizationId = "yunxiao_organization_id";

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
        _ => null
    };
}