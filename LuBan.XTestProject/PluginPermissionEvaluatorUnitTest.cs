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
using System.Text.Json;

namespace LuBan.XTestProject;

/// <summary>
/// 插件权限判定器测试（PRD §3.7）：目录白名单 + 默认拒绝。
/// </summary>
/// <remarks>
/// 语义要点：插件写明的<b>具体范围</b>必须被白名单<b>覆盖</b>；通配 <c>*</c>（含数组简写的裸维度形式，
/// 如 <c>["filesystem", "network"]</c>）表示「声明该维度、具体范围由宿主白名单决定」——
/// 声明期放行并记警告，运行期以宿主白名单为上限判定（不得借 <c>*</c> 绕过白名单）。
/// 运行期未声明即拒绝。
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

    /// <summary>
    /// 断言通配声明（<c>*</c>）在声明期被放行，并留下「范围由宿主白名单决定」的警告。
    /// </summary>
    private static void AssertWildcardAcceptedWithWarning(PluginPermissions permissions, string dimension)
    {
        var result = Validate(permissions);

        Assert.IsTrue(result.IsValid, $"{dimension} 通配声明应放行：{result.ToSummary()}");
        Assert.IsTrue(
            result.Warnings.Any(w => w.Contains($"permissions.{dimension}", StringComparison.Ordinal) &&
                                     w.Contains("通配", StringComparison.Ordinal)),
            $"{dimension} 通配声明应记警告：{result.ToSummary()}");
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
    public void Declaration_NetworkBareWildcard_IsAcceptedWithWarning()
    {
        // 通配 = 「声明网络维度，范围由宿主白名单决定」：声明期放行，运行期以白名单为上限
        AssertWildcardAcceptedWithWarning(Perms("network", "*"), "network");
    }

    [TestMethod]
    public void Declaration_BareDimensionShorthand_EqualsWildcardAndIsAccepted()
    {
        // 数组简写里的裸维度（"network"）解析为 network: ["*"]，与显式通配同义
        var permissions = JsonSerializer.Deserialize<PluginPermissions>(
            """["filesystem:workspace", "network"]""")!;

        Assert.IsTrue(permissions.Network.Single() == "*", "裸维度应解析为通配 *");
        AssertWildcardAcceptedWithWarning(permissions, "network");
    }

    [TestMethod]
    public void Declaration_NetworkBareWildcard_WarningMentionsHostWhitelist()
    {
        var result = Validate(Perms("network", "*"));

        Assert.IsTrue(result.Warnings.Any(w => w.Contains("codeup.aliyun.com", StringComparison.Ordinal)),
            $"警告应列出宿主白名单：{result.ToSummary()}");
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
    public void Declaration_ProcessBareWildcard_IsAcceptedWithWarning()
    {
        AssertWildcardAcceptedWithWarning(Perms("process", "*"), "process");
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
    public void Declaration_CredentialBareWildcard_IsAcceptedWithWarning()
    {
        AssertWildcardAcceptedWithWarning(Perms("credential", "*"), "credential");
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
    public void Declaration_Git_CommitAndInspectGroups_AreValid()
    {
        AssertAccepted(Perms("git", "commit"), "git");
        AssertAccepted(Perms("git", "inspect"), "git");
    }

    [TestMethod]
    public void Declaration_Git_LocalOperations_CoveredByGroups()
    {
        AssertAccepted(Perms("git", "add"), "git");
        AssertAccepted(Perms("git", "commit"), "git");
        AssertAccepted(Perms("git", "rev-parse"), "git");
        AssertAccepted(Perms("git", "checkout"), "git");
    }

    [TestMethod]
    public void Declaration_Git_UnrelatedOperation_IsRejected()
    {
        AssertRejected(Perms("git", "reset_hard"), "git");
    }

    [TestMethod]
    public void Declaration_GitBareWildcard_IsAcceptedWithWarning()
    {
        AssertWildcardAcceptedWithWarning(Perms("git", "*"), "git");
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
    public void Runtime_Git_ClonePush_DoesNotCoverLocalCommitOrInspect()
    {
        // 回归：Step 6 本地提交/检查（add/commit/rev-parse）不在 clone_push 覆盖内，必须单独声明
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("git", "clone_push");

        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "add"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "commit"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "rev-parse"));
    }

    [TestMethod]
    public void Runtime_Git_CommitGroup_CoversAddAndCommit()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("git", "commit");

        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "add"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "commit"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "push"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "reset_hard"));
    }

    [TestMethod]
    public void Runtime_Git_InspectGroup_CoversLocalReadOperations()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("git", "inspect");

        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "rev-parse"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "status"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "diff"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "branch"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "config"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "checkout"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "add"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "push"));
    }

    [TestMethod]
    public void Runtime_Filesystem_WorkspaceOnly()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("filesystem", "workspace");

        Assert.IsTrue(evaluator.IsFilesystemAllowed(permissions, "workspace"));
        Assert.IsFalse(evaluator.IsFilesystemAllowed(permissions, "/etc"));
    }

    // ---------------- 运行期判定：通配声明以宿主白名单为上限 ----------------

    [TestMethod]
    public void Runtime_Network_WildcardIsBoundedByHostWhitelist()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("network", "*");

        // 白名单内的主机放行
        Assert.IsTrue(evaluator.IsNetworkAllowed(permissions, "codeup.aliyun.com"));
        Assert.IsTrue(evaluator.IsNetworkAllowed(permissions, "openapi-rdc.aliyuncs.com"));

        // 白名单外仍被拒绝：通配 ≠ 放行全网
        Assert.IsFalse(evaluator.IsNetworkAllowed(permissions, "evil.example.com"));
        Assert.IsFalse(evaluator.IsNetworkAllowed(permissions, "aliyuncs.com"));
    }

    [TestMethod]
    public void Runtime_Network_WildcardIsBoundedByCustomWhitelist()
    {
        var options = new PluginSecurityOptions { NetworkWhitelist = new List<string> { "corp.internal" } };
        var evaluator = new PluginPermissionEvaluator(options);
        var permissions = Perms("network", "*");

        Assert.IsTrue(evaluator.IsNetworkAllowed(permissions, "corp.internal"));
        Assert.IsFalse(evaluator.IsNetworkAllowed(permissions, "codeup.aliyun.com"));
    }

    [TestMethod]
    public void Runtime_Process_WildcardIsBoundedByHostWhitelist()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("process", "*");

        Assert.IsTrue(evaluator.IsProcessAllowed(permissions, @"C:\Program Files\Git\bin\git.exe"));
        Assert.IsFalse(evaluator.IsProcessAllowed(permissions, "curl"));
    }

    [TestMethod]
    public void Runtime_Credential_WildcardIsBoundedByHostCatalog()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("credential", "*");

        Assert.IsTrue(evaluator.IsCredentialAllowed(permissions, CredentialKeys.YunxiaoPat));
        Assert.IsFalse(evaluator.IsCredentialAllowed(permissions, "other_key"));
    }

    [TestMethod]
    public void Runtime_Git_WildcardIsBoundedByHostWhitelist()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("git", "*");

        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "clone"));
        Assert.IsTrue(evaluator.IsGitAllowed(permissions, "push"));
        Assert.IsFalse(evaluator.IsGitAllowed(permissions, "reset_hard"));
    }

    [TestMethod]
    public void Runtime_Filesystem_WildcardIsBoundedByHostWhitelist()
    {
        var evaluator = new PluginPermissionEvaluator();
        var permissions = Perms("filesystem", "*");

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
        CollectionAssert.Contains(options.GitWhitelist.ToArray(), "commit");
        CollectionAssert.Contains(options.GitWhitelist.ToArray(), "inspect");
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