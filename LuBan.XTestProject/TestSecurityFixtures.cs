/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： TestSecurityFixtures
*描述：框架单测自带的「示例宿主」安全策略与凭据键别名
*
* 背景：框架 LuBan.AIAgent 已去厂商化——白名单（网络域名 / 凭据目录）默认空，
*      厂商历史键的别名由宿主实现 ICredentialKeyAliases 提供。
*      本文件是测试侧的示例宿主：给出白名单与别名，用于验证宿主经配置 /
*      别名即可恢复既有语义（框架自身不含任何厂商知识）。
*
*****************************************************************************/
using LuBan.AIAgent.Plugins.Security;

namespace LuBan.XTestProject;

/// <summary>
/// 测试用「示例宿主」安全策略：白名单项与凭据目录均为<b>宿主配置</b>（非框架默认值）。
/// </summary>
internal static class TestSecurity
{
    /// <summary>规范令牌键（<c>connector:&lt;实例名&gt;:token</c>）。</summary>
    internal const string DemoTokenKey = "connector:demo:token";

    /// <summary>规范组织字段键。</summary>
    internal const string DemoOrgKey = "connector:demo:organizationid";

    /// <summary>示例宿主别名实现（把历史拼写映射到规范键）。</summary>
    internal static ICredentialKeyAliases Aliases { get; } = new DemoCredentialKeyAliases();

    /// <summary>
    /// 构造一份「示例宿主」白名单策略（等价于宿主在
    /// <c>LuBanAgent:Plugins:Security</c> 里配置的目录）。
    /// </summary>
    internal static PluginSecurityOptions Options() => new()
    {
        NetworkWhitelist = new List<string> { "codeup.aliyun.com", "flow.aliyun.com", "*.aliyuncs.com" },
        ProcessWhitelist = new List<string> { "dotnet", "git" },
        CredentialCatalog = new List<string> { DemoTokenKey, DemoOrgKey },
        GitWhitelist = new List<string> { "clone_push", "commit", "inspect" },
        FilesystemWhitelist = new List<string> { "workspace" }
    };

    /// <summary>构造判定器（示例宿主策略 + 示例别名）。</summary>
    internal static PluginPermissionEvaluator Evaluator(PluginSecurityOptions? options = null)
        => new(options ?? Options(), Aliases);
}

/// <summary>
/// 示例宿主的凭据键别名：演示「历史拼写 → 规范键」的映射与旧数据回退。
/// </summary>
/// <remarks>
/// 这里刻意使用 <c>codeup:pat</c> / <c>yunxiao_pat</c> 一类历史拼写作为<b>输入数据</b>，
/// 用于证明框架的别名机制可用；框架自身不识别这些键。
/// </remarks>
internal sealed class DemoCredentialKeyAliases : ICredentialKeyAliases
{
    private static readonly string[] TokenAliases =
    {
        "demo_pat", "demo:pat", "codeup_pat", "codeup:pat", "yunxiao_pat", "yunxiao:pat"
    };

    private static readonly string[] OrgAliases =
    {
        "demo_org_id", "demo:org", "codeup_org_id", "codeup:org", "yunxiao_org_id", "yunxiao:org"
    };

    /// <inheritdoc />
    public string Resolve(string key)
    {
        var normalized = CredentialKeys.Normalize(key);

        if (TokenAliases.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            return TestSecurity.DemoTokenKey;

        if (OrgAliases.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            return TestSecurity.DemoOrgKey;

        return normalized;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetLegacyAliases(string canonicalKey) => canonicalKey switch
    {
        TestSecurity.DemoTokenKey => new[] { "demo_pat", "codeup:pat", "yunxiao_pat" },
        TestSecurity.DemoOrgKey => new[] { "demo_org_id", "codeup:org", "yunxiao_org_id" },
        _ => Array.Empty<string>()
    };

    /// <inheritdoc />
    public string? GetLegacyEnvironmentVariableName(string canonicalKey) => canonicalKey switch
    {
        TestSecurity.DemoTokenKey => "LUBAN_DEMO_PAT",
        TestSecurity.DemoOrgKey => "LUBAN_DEMO_ORGANIZATION_ID",
        _ => null
    };
}