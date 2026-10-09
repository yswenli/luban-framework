/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ToolConfirmationUnitTest
*唯一标识：工具确认引擎与子代理代确认单测
*创建时间：2026/9/18
*描述：验证确认三态、用户拒绝钩子（排除取消）、子代理代确认与专用文案
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using System.Text.Json;

namespace LuBan.XTestProject;

[TestClass]
public class ToolConfirmationUnitTest
{
    private const string WorkspaceRoot = @"C:\work";

    private static ToolConfirmationService CreateService(out ToolConfirmationContext context)
    {
        context = new ToolConfirmationContext();
        return new ToolConfirmationService(context, null);
    }

    private static void Configure(
        ToolConfirmationContext context,
        ToolPermissionMode mode = ToolPermissionMode.Default,
        Func<string, IReadOnlyDictionary<string, object?>, Task<ConfirmResult>>? callback = null,
        Action<string, IReadOnlyDictionary<string, object?>>? onUserDenied = null,
        string? allowedTool = null)
    {
        context.Mode = mode;
        context.WorkspacePathChecker = path => path.StartsWith(WorkspaceRoot, StringComparison.OrdinalIgnoreCase);
        context.Callback = callback;
        context.OnUserDenied = onUserDenied;
        if (allowedTool is not null)
            context.AllowThisTurn(allowedTool);
    }

    [TestMethod]
    public async Task Default_WorkspaceInternalPath_IsAllowedWithoutCallback()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
        Assert.IsFalse(callbackInvoked);
    }

    [TestMethod]
    public async Task Default_WorkspaceExternalPath_CallbackTrue_IsAllowed()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Allow));

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
    }

    [TestMethod]
    public async Task Default_CallbackFalse_IsDenied_AndFiresUserDeniedHook()
    {
        var service = CreateService(out var context);
        string? deniedTool = null;
        Configure(context,
            callback: (_, _) => Task.FromResult(ConfirmResult.Deny),
            onUserDenied: (tool, _) => deniedTool = tool);

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
        Assert.AreEqual("WriteFileAsync", deniedTool);
    }

    [TestMethod]
    public async Task Default_CallbackNull_IsDenied_ButDoesNotFireHook()
    {
        var service = CreateService(out var context);
        var hookFired = false;
        Configure(context, callback: null, onUserDenied: (_, _) => hookFired = true);

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
        Assert.IsFalse(hookFired);
    }

    [TestMethod]
    public async Task TokenCanceled_IsDenied_WithoutCallbackOrHook()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        var hookFired = false;
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Configure(context,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); },
            onUserDenied: (_, _) => hookFired = true);
        context.CancellationToken = cts.Token;

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
        Assert.IsFalse(callbackInvoked, "取消后不应再弹确认");
        Assert.IsFalse(hookFired, "取消不应触发用户拒绝钩子");
    }

    [TestMethod]
    public async Task CanceledDuringWait_WakesWithDeny_AndDoesNotFireHook()
    {
        var service = CreateService(out var context);
        var hookFired = false;
        using var cts = new CancellationTokenSource();
        Configure(context,
            callback: async (_, _) => { await cts.CancelAsync(); return ConfirmResult.Deny; },
            onUserDenied: (_, _) => hookFired = true);
        context.CancellationToken = cts.Token;

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
        Assert.IsFalse(hookFired, "等待期被取消（token 已取消）不应触发用户拒绝钩子");
    }

    [TestMethod]
    public async Task Plan_NonReadOnly_IsPlanned_AndInvokesPlannedAction()
    {
        var service = CreateService(out var context);
        string? planned = null;
        Configure(context, mode: ToolPermissionMode.Plan);
        context.OnPlannedAction = (tool, _) => planned = tool;

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Planned, outcome);
        Assert.AreEqual("WriteFileAsync", planned);
    }

    [TestMethod]
    public async Task Bypass_IsAllowed()
    {
        var service = CreateService(out var context);
        Configure(context, mode: ToolPermissionMode.BypassPermissions);

        var outcome = await service.EvaluateAsync("RunShellAsync", null, new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
    }

    [TestMethod]
    public async Task AlwaysConfirmTool_CallbackFalse_IsDenied_EvenInsideWorkspace()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Deny));

        var outcome = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
    }

    [TestMethod]
    public async Task Default_ReadOnlyWorkspacePath_IsAllowedWithoutCallback()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Deny); });

        var outcome = await service.EvaluateAsync("ReadFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "工作区内只读免确认");
        Assert.IsFalse(callbackInvoked);
    }

    [TestMethod]
    public async Task Default_ReadOnlyExternalPath_CallbackFalse_IsDenied()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Deny));

        var outcome = await service.EvaluateAsync("ReadFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "工作区外只读需确认，用户拒绝则拒绝");
    }

    [TestMethod]
    public async Task Default_ReadOnlyExternalPath_CallbackAllow_IsAllowed()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Allow));

        var outcome = await service.EvaluateAsync("ReadFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "工作区外只读经用户允许后放行");
    }

    [TestMethod]
    public async Task SubAgentScope_WithoutAllowedThisTurn_IsDenied_WithoutCallbackOrHook()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        var hookFired = false;
        Configure(context,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); },
            onUserDenied: (_, _) => hookFired = true);

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
        Assert.IsFalse(callbackInvoked, "子代理不应弹用户确认");
        Assert.IsFalse(hookFired, "子代理代确认拒绝不是用户拒绝，不应触发终止钩子");
    }

    [TestMethod]
    public async Task SubAgentScope_WithAllowedThisTurn_IsAllowed_WithoutCallback()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); },
            allowedTool: "WriteFileAsync");

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
        Assert.IsFalse(callbackInvoked);
    }

    [TestMethod]
    public void SubAgentScope_DeniedResult_UsesSubAgentMessage()
    {
        ToolResult<string> inside;
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            inside = ToolResult.Denied<string>();
        }
        var outside = ToolResult.Denied<string>();

        Assert.IsFalse(inside.UserCancelled);
        StringAssert.Contains(inside.Message, "子代理");
        Assert.IsTrue(outside.UserCancelled);
        StringAssert.Contains(outside.Message, "用户拒绝");
    }

    [TestMethod]
    public void SubAgentScope_IsScopedPerAsyncFlow()
    {
        Assert.IsFalse(ToolConfirmationContext.IsInSubAgentScope);
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            Assert.IsTrue(ToolConfirmationContext.IsInSubAgentScope);
        }
        Assert.IsFalse(ToolConfirmationContext.IsInSubAgentScope);
    }

    [TestMethod]
    public void SubAgentScope_NestedAndDoubleDispose_RestoresDepthCorrectly()
    {
        Assert.IsFalse(ToolConfirmationContext.IsInSubAgentScope);

        var outer = ToolConfirmationContext.EnterSubAgentScope();
        var inner = ToolConfirmationContext.EnterSubAgentScope();
        Assert.IsTrue(ToolConfirmationContext.IsInSubAgentScope);

        inner.Dispose();
        inner.Dispose(); // 重复释放必须被忽略
        Assert.IsTrue(ToolConfirmationContext.IsInSubAgentScope, "内层重复释放不应退出外层作用域");

        outer.Dispose();
        Assert.IsFalse(ToolConfirmationContext.IsInSubAgentScope);
    }

    [TestMethod]
    public void IsUserCancelled_RawToolResult_And_SerializedJsonElement()
    {
        Assert.IsTrue(ToolResult.IsUserCancelled(ToolResult.Cancelled<string>()));
        Assert.IsFalse(ToolResult.IsUserCancelled(ToolResult.Ok("x")));

        // M.E.AI 的 AIFunctionFactory 默认把返回值序列化成 JsonElement（camelCase）
        var camel = JsonSerializer.Deserialize<JsonElement>("{\"isSuccess\":false,\"message\":\"x\",\"userCancelled\":true}");
        Assert.IsTrue(ToolResult.IsUserCancelled(camel));

        var pascal = JsonSerializer.Deserialize<JsonElement>("{\"IsSuccess\":false,\"UserCancelled\":true}");
        Assert.IsTrue(ToolResult.IsUserCancelled(pascal));

        var notCancelled = JsonSerializer.Deserialize<JsonElement>("{\"isSuccess\":false,\"userCancelled\":false}");
        Assert.IsFalse(ToolResult.IsUserCancelled(notCancelled));
    }

    [TestMethod]
    public void SubAgentAllowAll_ScopeFlag_IsOptInOnly()
    {
        Assert.IsFalse(ToolConfirmationContext.IsSubAgentAllowAll, "默认不在子代理作用域");

        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            Assert.IsTrue(ToolConfirmationContext.IsInSubAgentScope);
            Assert.IsFalse(ToolConfirmationContext.IsSubAgentAllowAll, "默认 EnterSubAgentScope 不启用全放行");
        }

        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            Assert.IsTrue(ToolConfirmationContext.IsSubAgentAllowAll);
        }

        Assert.IsFalse(ToolConfirmationContext.IsInSubAgentScope);
        Assert.IsFalse(ToolConfirmationContext.IsSubAgentAllowAll);
    }

    [TestMethod]
    public async Task SubAgentAllowAll_WorkspaceInternalWrite_IsAllowed_WithoutCallback()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync("WriteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
        Assert.IsFalse(callbackInvoked, "全放行子代理不应弹用户确认");
    }

    [TestMethod]
    public async Task SubAgentAllowAll_ScriptWithoutPath_IsAllowed()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Deny));

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync("RunShellAsync", null, new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
    }

    [TestMethod]
    public async Task SubAgentAllowAll_ReadOnlyExternalPath_IsDenied()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Deny));

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync("ReadFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "子代理全放行下越界路径为硬约束，只读也拒绝");
    }

    [TestMethod]
    public async Task SubAgentAllowAll_WorkspaceExternalWrite_IsDenied()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "写类越界为硬约束，必须拒绝");
        Assert.IsFalse(callbackInvoked);
    }

    [TestMethod]
    public async Task SubAgentAllowAll_DeleteTool_IsDenied_EvenInsideWorkspace()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Allow));

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "删除类为硬约束，必须拒绝");
    }

    [TestMethod]
    public async Task Default_AllowAll_RemembersRoot_ForSiblingFileInSameTurn()
    {
        var service = CreateService(out var context);
        var callbackCount = 0;
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.AllowAll); });

        var first = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        var second = await service.EvaluateAsync("WriteFileAsync", @"D:\other\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, first);
        Assert.AreEqual(EnumConfirmationOutcome.Allowed, second, "同目录下的兄弟文件应命中本轮授权根");
        Assert.AreEqual(1, callbackCount, "第二次不应再弹确认");
    }

    [TestMethod]
    public async Task Default_AllowSession_SurvivesReset_ForSameRoot()
    {
        var service = CreateService(out var context);
        var callbackCount = 0;
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.AllowSession); });

        var first = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        // 模拟新一轮：Reset 清空本轮集合，但本会话授权须存活
        context.Reset();
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.Deny); });

        var second = await service.EvaluateAsync("WriteFileAsync", @"D:\other\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, first);
        Assert.AreEqual(EnumConfirmationOutcome.Allowed, second, "本会话授权应跨轮存活");
        Assert.AreEqual(1, callbackCount);
    }

    [TestMethod]
    public async Task Default_AllowAll_DoesNotSurviveReset()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.AllowAll));

        await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        context.Reset();
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Deny));

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "本轮授权在 Reset 后应失效");
    }

    [TestMethod]
    public async Task Default_AllowAll_DeleteTool_DoesNotRemember_StillConfirmsAgain()
    {
        var service = CreateService(out var context);
        var callbackCount = 0;
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.AllowAll); });

        var first = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());
        var second = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, first);
        Assert.AreEqual(EnumConfirmationOutcome.Allowed, second);
        Assert.AreEqual(2, callbackCount, "删除类不记忆授权，每次都必须确认");
    }

    [TestMethod]
    public async Task Default_AllowSession_DeleteTool_DoesNotRemember_StillConfirmsAgain()
    {
        var service = CreateService(out var context);
        var callbackCount = 0;
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.AllowSession); });

        var first = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        context.Reset();
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.AllowSession); });

        var second = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, first);
        Assert.AreEqual(EnumConfirmationOutcome.Allowed, second);
        Assert.AreEqual(2, callbackCount, "删除类本会话授权也不记忆，每次都必须确认");
    }

    [TestMethod]
    public async Task Default_AllowAll_DriveRootPath_DoesNotRemember_StillConfirmsAgain()
    {
        var service = CreateService(out var context);
        var callbackCount = 0;
        Configure(context, callback: (_, _) => { callbackCount++; return Task.FromResult(ConfirmResult.AllowAll); });

        var first = await service.EvaluateAsync("WriteFileAsync", @"D:\a.txt", new Dictionary<string, object?>());
        var second = await service.EvaluateAsync("WriteFileAsync", @"D:\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, first);
        Assert.AreEqual(EnumConfirmationOutcome.Allowed, second);
        Assert.AreEqual(2, callbackCount, "授权根落到盘符根时不记忆，避免放行整盘");
    }

    [TestMethod]
    public async Task Plan_ReadOnlyExternalPath_IsPlanned_NotAllowed()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, mode: ToolPermissionMode.Plan,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });
        string? planned = null;
        context.OnPlannedAction = (tool, _) => planned = tool;

        var outcome = await service.EvaluateAsync("ReadFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Planned, outcome, "Plan 模式越界只读应记为计划项");
        Assert.AreEqual("ReadFileAsync", planned);
        Assert.IsFalse(callbackInvoked);
    }

    [TestMethod]
    public async Task Plan_ReadOnlyWorkspacePath_IsAllowed()
    {
        var service = CreateService(out var context);
        Configure(context, mode: ToolPermissionMode.Plan);

        var outcome = await service.EvaluateAsync("ReadFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "Plan 模式工作区内只读照常放行");
    }

    [TestMethod]
    public async Task Plan_SearchFilesWorkspacePath_IsAllowed()
    {
        var service = CreateService(out var context);
        Configure(context, mode: ToolPermissionMode.Plan);

        var outcome = await service.EvaluateAsync("SearchFilesAsync", @"C:\work", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "Plan 模式工作区内搜索只读照常放行");
    }

    [TestMethod]
    public async Task Default_AllowSession_CanBeClearedByResetSession()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.AllowSession));

        await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        context.ResetSession();
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.Deny));

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "ResetSession 后本会话授权应失效");
    }
}