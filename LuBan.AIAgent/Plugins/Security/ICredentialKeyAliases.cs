/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： ICredentialKeyAliases
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：凭据键历史别名来源 —— 框架不含厂商知识，别名由宿主/产品提供
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 凭据键历史别名来源：把厂商历史拼写映射到规范键，并支持旧数据回退读取。
/// </summary>
/// <remarks>
/// <para>
/// 框架只认 <see cref="CredentialKeys"/> 的通用规范拼写；任何厂商旧键（例如某平台早期的
/// <c>xxx_pat</c>）都由宿主/产品实现本接口后注册，框架据此完成「旧配置无需人工迁移」。
/// </para>
/// <para>
/// 未注册实现时使用 <see cref="NullCredentialKeyAliases"/>：仅做通用归一化，无别名回退。
/// </para>
/// </remarks>
public interface ICredentialKeyAliases
{
    /// <summary>
    /// 把任意写法归一化为规范键。
    /// </summary>
    /// <param name="key">凭据键（可能是历史拼写）。</param>
    /// <returns>规范键；无法识别时返回 <see cref="CredentialKeys.Normalize"/> 的结果。</returns>
    string Resolve(string key);

    /// <summary>
    /// 取规范键对应的<b>历史拼写</b>列表（仅用于回退读取）。
    /// </summary>
    /// <param name="canonicalKey">规范键。</param>
    /// <returns>历史键拼写；无别名时返回空集合。</returns>
    IReadOnlyList<string> GetLegacyAliases(string canonicalKey);

    /// <summary>
    /// 取规范键对应的<b>历史环境变量名</b>（仅用于回退兜底读取）。
    /// </summary>
    /// <param name="canonicalKey">规范键。</param>
    /// <returns>历史环境变量名；无映射时返回 <c>null</c>。</returns>
    string? GetLegacyEnvironmentVariableName(string canonicalKey);
}

/// <summary>
/// 空别名实现：只做通用归一化，不提供任何厂商别名。
/// </summary>
public sealed class NullCredentialKeyAliases : ICredentialKeyAliases
{
    /// <summary>单例。</summary>
    public static NullCredentialKeyAliases Instance { get; } = new();

    /// <inheritdoc />
    public string Resolve(string key) => CredentialKeys.Normalize(key);

    /// <inheritdoc />
    public IReadOnlyList<string> GetLegacyAliases(string canonicalKey) => Array.Empty<string>();

    /// <inheritdoc />
    public string? GetLegacyEnvironmentVariableName(string canonicalKey) => null;
}