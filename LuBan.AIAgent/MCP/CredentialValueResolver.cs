/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.MCP
*文件名： CredentialValueResolver
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：MCP 凭据引用解析器 —— $credential:<key> → ICredentialStore
*
*****************************************************************************/
using System.Text;
using LuBan.AIAgent.Plugins.Security;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// 默认引用解析器：把配置值中的 <c>$credential:&lt;凭据键&gt;</c> 替换为 <see cref="ICredentialStore"/> 中的值。
/// </summary>
/// <remarks>
/// <para>
/// 语法：<c>$credential:connector:my:token</c>（凭据键本身可含冒号，读到空白符为止）。
/// 支持<b>内联</b>使用，例如 <c>Authorization: Bearer $credential:connector:my:token</c>；
/// 一个值里可出现多处引用，全部替换。
/// </para>
/// <para>
/// 键交给 <see cref="ICredentialStore"/> 自行归一化，框架不做厂商假设。
/// </para>
/// </remarks>
public sealed class CredentialValueResolver : IMcpConfigValueResolver
{
    /// <summary>凭据引用前缀。</summary>
    public const string Prefix = "$credential:";

    private readonly ICredentialStore _store;

    /// <summary>
    /// 创建解析器。
    /// </summary>
    /// <param name="store">凭据存储。</param>
    public CredentialValueResolver(ICredentialStore store)
        => _store = store ?? throw new ArgumentNullException(nameof(store));

    /// <summary>
    /// 判断给定值是否包含凭据引用。
    /// </summary>
    /// <param name="value">配置值。</param>
    /// <returns>包含引用返回 <c>true</c>。</returns>
    public static bool IsReference(string? value)
        => value is not null && value.Contains(Prefix, StringComparison.Ordinal);

    /// <summary>
    /// 取首个引用中的凭据键（读到空白符为止）。
    /// </summary>
    /// <param name="value">配置值。</param>
    /// <returns>凭据键；无引用或键为空时返回 <c>null</c>。</returns>
    public static string? ExtractKey(string? value)
    {
        if (!IsReference(value)) return null;

        var start = value!.IndexOf(Prefix, StringComparison.Ordinal) + Prefix.Length;
        var end = start;
        while (end < value.Length && !char.IsWhiteSpace(value[end])) end++;

        var key = value[start..end];
        return key.Length == 0 ? null : key;
    }

    /// <inheritdoc />
    public async Task<string?> ResolveAsync(string? template, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(template)) return template;
        if (!IsReference(template)) return template;

        var builder = new StringBuilder(template.Length + 32);
        var index = 0;

        while (true)
        {
            var start = template.IndexOf(Prefix, index, StringComparison.Ordinal);
            if (start < 0)
            {
                builder.Append(template, index, template.Length - index);
                break;
            }

            builder.Append(template, index, start - index);

            var keyStart = start + Prefix.Length;
            var keyEnd = keyStart;
            while (keyEnd < template.Length && !char.IsWhiteSpace(template[keyEnd])) keyEnd++;

            var key = template[keyStart..keyEnd];
            if (key.Length == 0)
            {
                Logger.Warn($"MCP 配置中的凭据引用缺少键名：{template}");
                return null;
            }

            var value = await _store.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(value))
            {
                Logger.Warn($"MCP 配置引用的凭据未配置：{key}");
                return null;
            }

            builder.Append(value);
            index = keyEnd;
        }

        return builder.ToString();
    }
}