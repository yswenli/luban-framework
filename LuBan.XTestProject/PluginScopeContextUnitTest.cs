/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginScopeContextUnitTest
*描述：验证权限环境（AsyncLocal）传递与运行期出口门禁（PRD §3.7）
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// 权限环境与出口门禁测试（PRD §3.7 / §4.8 第 5 条）。
/// </summary>
/// <remarks>
/// 核心语义：<b>无权限环境 = 宿主自身调用，一律放行</b>；有环境则按插件声明判定，越界即拒绝并审计。
/// </remarks>
[TestClass]
public class PluginScopeContextUnitTest
{
    private static PluginSecurityOptions GuardOptions(bool enforce = true, bool audit = false, int maxRecords = 64) => new()
    {
        EnforceAtRuntime = enforce,
        AuditDenials = audit,
        MaxAuditRecords = maxRecords,
    };

    private static PluginPermissions Network(params string[] scopes)
        => new() { Network = scopes.ToList() };

    private static PluginPermissions Credential(params string[] scopes)
        => new() { Credential = scopes.ToList() };

    // ---------------- 环境传递 ----------------

    [TestMethod]
    public void Current_IsNullByDefault()
    {
        Assert.IsNull(PluginScopeContext.Current);
    }

    [TestMethod]
    public void Enter_SetsScope_AndDisposeRestores()
    {
        using (PluginScopeContext.Enter("com.demo.a", Network("codeup.aliyun.com")))
        {
            Assert.IsNotNull(PluginScopeContext.Current);
            Assert.AreEqual("com.demo.a", PluginScopeContext.Current!.PluginId);
        }

        Assert.IsNull(PluginScopeContext.Current);
    }

    [TestMethod]
    public void Enter_IsNestable_AndRestoresOuter()
    {
        using (PluginScopeContext.Enter("outer", Network("codeup.aliyun.com")))
        {
            using (PluginScopeContext.Enter("inner", Network("flow.aliyun.com")))
            {
                Assert.AreEqual("inner", PluginScopeContext.Current!.PluginId);
            }

            Assert.AreEqual("outer", PluginScopeContext.Current!.PluginId);
        }

        Assert.IsNull(PluginScopeContext.Current);
    }

    [TestMethod]
    public void Enter_NullScope_MeansHostCall()
    {
        using (PluginScopeContext.Enter(scope: (PluginPermissionScope?)null))
        {
            Assert.IsNull(PluginScopeContext.Current);
        }
    }

    [TestMethod]
    public async Task Enter_FlowsIntoAsyncContinuations()
    {
        using (PluginScopeContext.Enter("com.demo.async", Network("codeup.aliyun.com")))
        {
            await Task.Yield();
            Assert.AreEqual("com.demo.async", PluginScopeContext.Current!.PluginId);

            await Task.Run(() => Assert.AreEqual("com.demo.async", PluginScopeContext.Current!.PluginId));
        }

        Assert.IsNull(PluginScopeContext.Current);
    }

    // ---------------- 门禁：宿主自身调用 ----------------

    [TestMethod]
    public void Guard_WithoutScope_AllowsEverything()
    {
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(GuardOptions()));

        Assert.IsTrue(guard.IsNetworkAllowed("evil.example.com"));
        Assert.IsTrue(guard.IsProcessAllowed("curl"));
        Assert.IsTrue(guard.IsCredentialAllowed("aws_secret_key"));
        Assert.IsTrue(guard.IsGitAllowed("reset_hard"));

        // 不应抛异常，也不应产生审计记录
        guard.RequireNetwork("evil.example.com", "host-call");
        guard.RequireProcess("curl", "host-call");
        guard.RequireCredential("aws_secret_key", "host-call");
        guard.RequireGit("reset_hard", "host-call");

        Assert.AreEqual(0, guard.RecentDenials.Count);
    }

    // ---------------- 门禁：插件声明范围内 ----------------

    [TestMethod]
    public void Guard_WithinDeclaration_Allows()
    {
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(GuardOptions()));

        using (PluginScopeContext.Enter("com.demo.ok", Network("codeup.aliyun.com")))
        {
            Assert.IsTrue(guard.IsNetworkAllowed("codeup.aliyun.com"));
            guard.RequireNetwork("codeup.aliyun.com", "plugin-call");
        }

        Assert.AreEqual(0, guard.RecentDenials.Count);
    }

    // ---------------- 门禁：越界即拒绝 + 审计 ----------------

    [TestMethod]
    public void Guard_OutOfDeclaration_ThrowsAndRecords()
    {
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(GuardOptions()));

        using (PluginScopeContext.Enter("com.demo.bad", Network("codeup.aliyun.com")))
        {
            var ex = Assert.ThrowsExactly<PluginPermissionDeniedException>(
                () => guard.RequireNetwork("evil.example.com", "plugin-call"));

            Assert.AreEqual("com.demo.bad", ex.PluginId);
            Assert.AreEqual("network", ex.Dimension);
            Assert.AreEqual("evil.example.com", ex.Target);
            Assert.IsTrue(ex.Message.Contains("未授权访问", StringComparison.Ordinal));
        }

        Assert.AreEqual(1, guard.RecentDenials.Count);
        Assert.AreEqual("evil.example.com", guard.RecentDenials[0].Target);
    }

    [TestMethod]
    public void Guard_UndeclaredCredential_IsDenied()
    {
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(GuardOptions()));

        // 声明了网络但未声明凭据 → 凭据出口仍被拒绝（PRD §3.7：未声明则无法读取）
        using (PluginScopeContext.Enter("com.demo.nocred", Network("codeup.aliyun.com")))
        {
            Assert.ThrowsExactly<PluginPermissionDeniedException>(
                () => guard.RequireCredential(CredentialKeys.YunxiaoPat, "plugin-call"));
        }
    }

    [TestMethod]
    public void Guard_DeclaredCredential_Allows()
    {
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(GuardOptions()));

        using (PluginScopeContext.Enter("com.demo.cred", Credential("codeup:pat")))
        {
            guard.RequireCredential(CredentialKeys.YunxiaoPat, "plugin-call");
        }

        Assert.AreEqual(0, guard.RecentDenials.Count);
    }

    // ---------------- 门禁：仅审计模式 ----------------

    [TestMethod]
    public void Guard_EnforceAtRuntimeFalse_RecordsButAllows()
    {
        var options = GuardOptions(enforce: false);
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(options));

        using (PluginScopeContext.Enter("com.demo.audit", Network("codeup.aliyun.com")))
        {
            guard.RequireNetwork("evil.example.com", "plugin-call");
            Assert.IsTrue(guard.IsNetworkAllowed("evil.example.com"));
        }

        Assert.AreEqual(1, guard.RecentDenials.Count);
    }

    // ---------------- 门禁：审计上限 ----------------

    [TestMethod]
    public void RecentDenials_IsBoundedByMaxAuditRecords()
    {
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(GuardOptions(maxRecords: 2)));

        using (PluginScopeContext.Enter("com.demo.bounded", Network("codeup.aliyun.com")))
        {
            for (var i = 0; i < 5; i++)
            {
                Assert.ThrowsExactly<PluginPermissionDeniedException>(
                    () => guard.RequireNetwork($"evil{i}.example.com", "plugin-call"));
            }
        }

        Assert.AreEqual(2, guard.RecentDenials.Count);
        Assert.AreEqual("evil4.example.com", guard.RecentDenials[1].Target);
    }

    [TestMethod]
    public void DefaultGuard_IsUsable()
    {
        Assert.IsNotNull(PluginPermissionGuard.Default);
        Assert.IsTrue(PluginPermissionGuard.Default.IsNetworkAllowed("evil.example.com"));
    }

    [TestMethod]
    public void GuardOptions_EnforceAtRuntimeDisabled_MakesEvaluationPermissive()
    {
        var options = GuardOptions(enforce: false);
        var guard = new PluginPermissionGuard(new PluginPermissionEvaluator(options));

        using (PluginScopeContext.Enter("com.demo.permissive", new PluginPermissions()))
        {
            Assert.IsTrue(guard.IsNetworkAllowed("evil.example.com"));
            Assert.IsTrue(guard.IsCredentialAllowed("aws_secret_key"));
        }
    }
}