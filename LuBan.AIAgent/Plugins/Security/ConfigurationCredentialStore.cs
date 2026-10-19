/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： ConfigurationCredentialStore
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：默认凭据存储 - 配置节点 + 环境变量兜底
*
*****************************************************************************/
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 默认凭据存储：从 <c>LuBanAgent:Credentials:&lt;key&gt;</c> 配置节点读取，未命中时回退到环境变量。
/// </summary>
/// <remarks>
/// <para>
/// 定位为<b>只读兜底</b>：适合最小宿主、CI、测试与「用环境变量注入凭据」的场景。
/// 桌面宿主应注册自己的可写实现（沿用其现有配置存储基线，PRD §4.9.3），本实现在被覆盖后不参与。
/// </para>
/// <para>
/// 环境变量映射见 <see cref="CredentialKeys.GetEnvironmentVariableName"/>（如 <c>yunxiao_pat</c> →
/// <c>LUBAN_YUNXIAO_PAT</c>）。
/// </para>
/// </remarks>
public sealed class ConfigurationCredentialStore : ICredentialStore
{
    private const string SectionName = "LuBanAgent:Credentials";

    private readonly IConfiguration _configuration;

    /// <summary>
    /// 创建配置凭据存储。
    /// </summary>
    /// <param name="configuration">宿主配置。</param>
    public ConfigurationCredentialStore(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        => Task.FromResult(Get(key));

    /// <inheritdoc />
    /// <remarks>默认实现为只读，写入会抛出 <see cref="NotSupportedException"/>。</remarks>
    public Task SetAsync(string key, string? value, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "ConfigurationCredentialStore 为只读实现（配置节点 + 环境变量兜底）。" +
            "如需在应用内录入凭据，请由宿主注册可写的 ICredentialStore 实现。");

    /// <inheritdoc />
    public bool Contains(string key) => !string.IsNullOrWhiteSpace(Get(key));

    /// <inheritdoc />
    public IReadOnlyCollection<string> Keys
    {
        get
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in EnumerateKeys(_configuration.GetSection(SectionName)))
                keys.Add(CredentialKeys.Normalize(key));

            // 已知键若可通过环境变量获得，也视为可用
            foreach (var known in PluginSecurityOptionsDefaults.KnownCredentialKeys)
            {
                if (!string.IsNullOrWhiteSpace(ReadEnvironment(known)))
                    keys.Add(CredentialKeys.Normalize(known));
            }

            return keys;
        }
    }

    /// <summary>
    /// 递归展平配置节点的键路径。
    /// </summary>
    /// <remarks>
    /// <c>IConfiguration</c> 以 <c>:</c> 为层级分隔符，而连接器令牌的规范键本身含冒号
    /// （<c>connector:&lt;实例名&gt;:token</c>，Q15），故 <c>connector:yunxiao:token</c> 在配置树里
    /// 是三层嵌套。直接取子节点短名只会得到 <c>connector</c> 这样的伪键，必须按路径拼回完整键。
    /// </remarks>
    /// <param name="section">配置节。</param>
    /// <returns>叶子节点的完整键路径。</returns>
    private static IEnumerable<string> EnumerateKeys(IConfigurationSection section)
    {
        foreach (var child in section.GetChildren())
        {
            var nested = child.GetChildren().ToList();
            if (nested.Count == 0)
            {
                yield return child.Key;
                continue;
            }

            // 既是节又有值（少见）：两者都算
            if (child.Value is not null)
                yield return child.Key;

            foreach (var path in EnumerateKeys(child))
                yield return $"{child.Key}:{path}";
        }
    }

    private string? Get(string key)
    {
        var normalized = CredentialKeys.Normalize(key);

        var configured = _configuration[$"{SectionName}:{normalized}"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        return ReadEnvironment(normalized);
    }

    private static string? ReadEnvironment(string key)
    {
        var variable = CredentialKeys.GetEnvironmentVariableName(key);
        if (string.IsNullOrWhiteSpace(variable))
            return null;

        var value = Environment.GetEnvironmentVariable(variable);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}

/// <summary>
/// 默认策略常量（供框架内部复用，避免多处硬编码）。
/// </summary>
internal static class PluginSecurityOptionsDefaults
{
    /// <summary>默认已知凭据键（与 <see cref="PluginSecurityOptions.CredentialCatalog"/> 默认值一致）。</summary>
    public static readonly IReadOnlyList<string> KnownCredentialKeys = new[]
    {
        CredentialKeys.YunxiaoPat
    };
}