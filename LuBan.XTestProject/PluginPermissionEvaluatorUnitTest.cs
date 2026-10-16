/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginPermissionEvaluatorUnitTest
*描述：验证插件权限判定器（PRD §3.7）：声明期「白名单覆盖」语义与运行期越界判定
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Loading;
using LuBan.AIAgent.Plugins.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// 插件权限判定器测试（PRD §3.7）：目录白名单 + 默认拒绝。
/// </summary>
/// <remarks>
/// 语义要点：插件声明必须被白名单<b>覆盖</b>；裸 <c>*</c> 只有在白名单本身含 <c>*</c> 时才被授权
/// （即「不得放行全网」）。运行期未声明即拒绝。
/// </remarks>
[TestClass]
public class PluginPermissionEvaluatorUnitTest
{
    private static PluginPermissions Perms(string dimension, params string[] scopes)
    {
        var permissions = new PluginPermissions();
        switch (dimension)
        {
            case "filesystem": permissions.Filesystem = scopes.ToList(); break;
            case "network": permissions.Network = scopes.ToList(); break;
            case "git": permissions.Git = scopes.ToList(); break;
            case "database": permissions.Database = scopes.ToList(); break;
            case "process": permissions.Process = scopes.ToList(); break;
            case "credential": permissions.Credential = scopes.ToList(); break;
            default: throw new ArgumentException($"未知维度：{dimension}", nameof(dimension));
        }
        return permissions;
    }

    private static ManifestValidationResult Validate(PluginPermissions? permissions, PluginSecurityOptions? options = null)
    {
        var result = new ManifestValidationResult();
        new PluginPermissionEvaluator(options).ValidateDeclaration(permissions, "permissions", result);
        return result;
    }

    private static void AssertRejected(PluginPermissions permissions, string dimension)
    {
        var result = Validate(permissions);
        Assert.IsFalse(result.IsValid, $"{dimension} 越界声明应被拒绝");
        Assert.IsTrue(result.Errors.Any(e => e.Contains("越界", StringComparison.Ordinal)),
            $"错误信息应含「越界」：{result.ToSummary()}");
    }

    private static void AssertAccepted(PluginPermissions permissions, string dimension)
    {
        var result = Validate(permissions);
        Assert.IsTrue(result.IsValid, $"{dimension} 合规声明应通过：{result.ToSummary()}");
    }

    // ---------------- 声明期：network ----------------

    [TestMethod]
    public void Declaration_NetworkInWhitelist_IsValid()
    {
        AssertAccepted(Perms("network", "codeup.aliyun.com", "flow.aliyun.com", "*.aliyuncs.com"), "network");
    }

    [TestMethod]
    public void Declaration_NetworkWildcardSubdomain_CoveredByWhitelistWildcard()
    {
        // *.aliyuncs.com 覆盖 *.foo.aliyuncs.com
        AssertAccepted(Perms("network", "*.foo.aliyuncs.com"), "network");
    }

    [TestMethod]
    public void Declaration_NetworkOutsideWhitelist_IsRejected()
    {
        AssertRejected(Perms("network", "evil.example.com"), "network");
    }

    [TestMethod]
    public void Declaration_NetworkBareWildcard_IsRejected()
    {
        // 宿主白名单不含 *，插件不得声明「放行全网」
        AssertRejected(Perms("network", "*"), "network");
    }

    [TestMethod]
    public void Declaration_NetworkBareWildcard_AllowedWhenWhitelistHasWildcard()
    {
        var options = new PluginSecurityOptions { NetworkWhitelist = new List<string> { "*" } };
        var result = Validate(Perms("network", "*"), options);
        Assert.IsTrue(result.IsValid, result.ToSummary());
    }

    [TestMethod]
    public void Declaration_NetworkBareHost_DoesNotCoverSubdomain()
    {
        // codeup.aliyun.com 只能覆盖裸域本身，不能覆盖其子域
        AssertRejected(Perms("network", "sub.codeup.aliyun.com"), "network");
    }

    // ---------------- 声明期：process ----------------

    [TestMethod]
    public void Declaration_Process_Whitelisted_IsValid()
    {
        AssertAccepted(Perms("process", "git", "dotnet"), "process");
    }

    [TestMethod]
    public void Declaration_Process_CaseInsensitive_IsValid()
    {
        AssertAccepted(Perms("process", "GIT"), "process");
    }

    [TestMethod]
    public void Declaration_Process_NotWhitelisted_IsRejected()
    {
        AssertRejected(Perms("process", "curl"), "process");
    }

    [TestMethod]
    public void Declaration_ProcessBareWildcard_IsRejected()
    {
        AssertRejected(Perms("process", "*"), "process");
    }

    // ---------------- 声明期：credential ----------------

    [TestMethod]
    public void Declaration_CredentialAlias_NormalizedAndAccepted()
    {
        // codeup:pat 与 yunxiao_pat 视为同一凭据
        AssertAccepted(Perms("credential", "codeup:pat"), "credential");
        AssertAccepted(Perms("credential", "yunxiao:pat"), "credential");
    }

    [TestMethod]
    public void Declaration_CredentialUnknownKey_IsRejected()
    {
        AssertRejected(Perms("credential", "aws_secret_key"), "credential");
    }

    [TestMethod]
    public void Declaration_CredentialBareWildcard_IsRejected()
    {
        AssertRejected(Perms("credential", "*"), "credential");
    }

    // ---------------- 声明期：git ----------------

    [TestMethod]
    public void Declaration_Git_ClonePush_IsValid()
    {
        AssertAccepted(Perms("git", "clone_push"), "git");
    }

    [TestMethod]
    public void Declaration_Git_SubOperation_CoveredByClonePush()
    {
        AssertAccepted(Perms("git", "clone"), "git");
        AssertAccepted(Perms("git", "push"), "git");
    }

    [TestMethod]
    public void Declaration_Git_UnrelatedOperation_IsRejected()
    {
        AssertRejected(Perms("git", "reset_hard"), "git");
    }

    [TestMethod]
    public void Declaration_GitBareWildcard_IsRejected()
    {
        AssertRejected(Perms("git", "*"), "git");
    }

    // ---------------- 声明期：filesystem / database ----------------

    [TestMethod]
    public void Declaration_Filesystem_Workspace_IsValid()
    {
        AssertAccepted(Perms("filesystem", "workspace"), "filesystem");
    }

    [TestMethod]
    public void Declaration_Filesystem_Outside_IsRejected()
    {
        AssertRejected(Perms("filesystem", "/etc"), "filesystem");
    }

    [TestMethod]
    public void Declaration_Database_ArbitraryScope_IsValid()
    {
        // PRD 未给定 database 白名单目录，仅校验非空
        AssertAccepted(Perms("database", "app_db"), "database");
    }

    // ---------------- 声明期：边界 ----------------

    [TestMethod]
    public void Declaration_NullOrEmptyPermissions_ProducesNoErrors()
    {
        Assert.IsTrue(Validate(null).IsValid);
        Assert.IsTrue(Validate(new PluginPermissions()).IsValid);
    }

    [TestMethod]
    public void Declaration_BlankScope_IsRejected()
    {
        var result = Validate(Perms("network", "   "));
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("空范围项", StringComparison.Ordinal)));
    }

    // ---------------- 运行期判定 ----------------

    [TestMethod]
    public void Runtime_Network_UndeclaredIsDenied()
    {
        var evaluator = new PluginPermissionEvaluator();
        Assert.IsFalse(evaluator.IsNetworkAllowed(null, "codeup.aliyun.com"));
        Assert.IsFalse(evaluator.IsNetworkAllowed(new PluginPermissions(), "codeup.aliyun.com"));
    }

    [TestMethod]
    public void Runtime_Network_DeclaredHostIsAllowed_OthersDenied()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("network", "codeup.aliyun.com");

        Assert.IsTrue(evaluator.IsNetworkAllowed(permissions, "codeup.aliyun.com"));
        Assert.IsFalse(evaluator.IsNetworkAllowed(permissions, "evil.example.com"));
    }

    [TestMethod]
    public void Runtime_Network_WildcardMatchesSubdomainOnly()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("network", "*.aliyuncs.com");

        Assert.IsTrue(evaluator.IsNetworkAllowed(permissions, "openapi-rdc.aliyuncs.com"));
        Assert.IsTrue(evaluator.IsNetworkAllowed(permissions, "a.b.aliyuncs.com"));
        Assert.IsFalse(evaluator.IsNetworkAllowed(permissions, "aliyuncs.com"));
    }

    [TestMethod]
    public void Runtime_Process_CommandPathIsNormalized()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("process", "git");

        Assert.IsTrue(evaluator.IsProcessAllowed(permissions, @"C:\Program Files\Git\bin\git.exe"));
        Assert.IsTrue(evaluator.IsProcessAllowed(permissions, "git"));
        Assert.IsFalse(evaluator.IsProcessAllowed(permissions, "curl"));
    }

    [TestMethod]
    public void Runtime_Credential_AliasAndCatalogKeyAreEquivalent()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("credential", "codeup:pat");

        Assert.IsTrue(evaluator.IsCredentialAllowed(permissions, CredentialKeys.YunxiaoPat));
        Assert.IsFalse(evaluator.IsCredentialAllowed(permissions, "other_key"));
    }

    [TestMethod]
    public void Runtime_Git_ClonePush_CoversCloneAndPush()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("git", "clone_push");

        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "clone"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "push"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "reset_hard"));
    }

    [TestMethod]
    public void Runtime_Filesystem_WorkspaceOnly()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("filesystem", "workspace");

        Assert.IsTrue(evaluator.IsFilesystemAllowed(permissions, "workspace"));
        Assert.IsFalse(evaluator.IsFilesystemAllowed(permissions, "/etc"));
    }

    // ---------------- 匹配原语 ----------------

    [TestMethod]
    public void HostMatches_WildcardAndExact()
    {
        Assert.IsTrue(PluginPermissionEvaluator.HostMatches("*", "anything.example.com"));
        Assert.IsTrue(PluginPermissionEvaluator.HostMatches("*.aliyuncs.com", "openapi-rdc.aliyuncs.com"));
        Assert.IsFalse(PluginPermissionEvaluator.HostMatches("*.aliyuncs.com", "aliyuncs.com"));
        Assert.IsTrue(PluginPermissionEvaluator.HostMatches("CodeUp.Aliyun.com", "codeup.aliyun.com"));
        Assert.IsFalse(PluginPermissionEvaluator.HostMatches("codeup.aliyun.com", "evil.example.com"));
        Assert.IsFalse(PluginPermissionEvaluator.HostMatches("", "codeup.aliyun.com"));
        Assert.IsFalse(PluginPermissionEvaluator.HostMatches("codeup.aliyun.com", null));
    }

    // ---------------- 策略默认值 ----------------

    [TestMethod]
    public void DefaultOptions_MatchPrdWhitelists()
    {
        var options = new PluginSecurityOptions();
        options.Normalize();

        CollectionAssert.Contains(options.NetworkWhitelist.ToArray(), "codeup.aliyun.com");
        CollectionAssert.Contains(options.NetworkWhitelist.ToArray(), "flow.aliyun.com");
        CollectionAssert.Contains(options.NetworkWhitelist.ToArray(), "*.aliyuncs.com");
        CollectionAssert.Contains(options.ProcessWhitelist.ToArray(), "git");
        CollectionAssert.Contains(options.ProcessWhitelist.ToArray(), "dotnet");
        CollectionAssert.Contains(options.CredentialCatalog.ToArray(), CredentialKeys.YunxiaoPat);
        CollectionAssert.Contains(options.GitWhitelist.ToArray(), "clone_push");
        CollectionAssert.Contains(options.FilesystemWhitelist.ToArray(), "workspace");
        Assert.IsTrue(options.EnforceAtRuntime);
        Assert.IsTrue(options.AuditDenials);
    }

    [TestMethod]
    public void OptionsNormalize_DeduplicatesAndTrims()
    {
        var options = new PluginSecurityOptions
        {
            ProcessWhitelist = new List<string> { " git ", "GIT", "", "dotnet" }
        };
        options.Normalize();

        Assert.AreEqual(2, options.ProcessWhitelist.Count);
        CollectionAssert.Contains(options.ProcessWhitelist.ToArray(), "git");
        CollectionAssert.Contains(options.ProcessWhitelist.ToArray(), "dotnet");
    }
}