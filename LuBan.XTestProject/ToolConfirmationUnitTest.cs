/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ToolConfirmationUnitTest
*唯一标识：工具确认引擎与子代理确认单测
*创建时间：2026/9/18
*描述：验证确认三态、用户拒绝钩子（排除取消）、子代理弹确认与授权共享、A2A 全放行代确认
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Tools.LocalMemory;
using Microsoft.Extensions.Options;
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
    public async Task SubAgentScope_NeedsConfirmation_PromptsUser_AndAllowsWhenApproved()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "子代理需要授权时应弹确认，用户允许后放行");
        Assert.IsTrue(callbackInvoked, "子代理应与其他工具一样向用户弹确认");
    }

    [TestMethod]
    public async Task SubAgentScope_DeniedByUser_DoesNotFireTurnTerminationHook()
    {
        var service = CreateService(out var context);
        var hookFired = false;
        Configure(context,
            callback: (_, _) => Task.FromResult(ConfirmResult.Deny),
            onUserDenied: (_, _) => hookFired = true);

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome);
        Assert.IsFalse(hookFired, "子代理被拒只失败当前节点，不应触发终止整轮的钩子");
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
    public void DeniedResult_SubAgentScopeWithoutAllowAll_IsUserDenial()
    {
        ToolResult<string> prompted;
        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            prompted = ToolResult.Denied<string>();
        }
        ToolResult<string> autoBlocked;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            autoBlocked = ToolResult.Denied<string>();
        }
        var outside = ToolResult.Denied<string>();

        Assert.IsTrue(prompted.UserCancelled, "已弹确认的子代理被拒即用户拒绝");
        StringAssert.Contains(prompted.Message, "用户拒绝");

        Assert.IsFalse(autoBlocked.UserCancelled, "A2A 全放行子代理无 UI 可确认，自动阻止不代表用户拒绝");
        StringAssert.Contains(autoBlocked.Message, "子代理");

        Assert.IsTrue(outside.UserCancelled);
        StringAssert.Contains(outside.Message, "用户拒绝");
    }

    [TestMethod]
    public async Task SubAgentScope_AllowAllGrant_IsSharedWithMainAgent()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => Task.FromResult(ConfirmResult.AllowAll));

        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            var inside = await service.EvaluateAsync("RunShellAsync", null, new Dictionary<string, object?>());
            Assert.AreEqual(EnumConfirmationOutcome.Allowed, inside);
        }

        var afterCallback = false;
        Configure(context, callback: (_, _) => { afterCallback = true; return Task.FromResult(ConfirmResult.Deny); });
        var outside = await service.EvaluateAsync("RunShellAsync", null, new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outside, "子代理授权写入共享的本轮已允许集合，主代理同样可见");
        Assert.IsFalse(afterCallback);
    }

    [TestMethod]
    public async Task ConcurrentConfirmations_AreSerialized()
    {
        var service = CreateService(out var context);
        var inFlight = 0;
        var maxInFlight = 0;
        Configure(context, callback: async (_, _) =>
        {
            var current = Interlocked.Increment(ref inFlight);
            InterlockedMax(ref maxInFlight, current);
            await Task.Delay(50);
            Interlocked.Decrement(ref inFlight);
            return ConfirmResult.Allow;
        });

        var first = service.EvaluateAsync("RunShellAsync", null, new Dictionary<string, object?>());
        var second = service.EvaluateAsync("RunShellAsync", null, new Dictionary<string, object?>());
        var outcomes = await Task.WhenAll(first, second);

        Assert.AreEqual(1, maxInFlight, "宿主确认 UI 为单槽，同一会话的确认必须串行");
        CollectionAssert.AreEqual(
            new[] { EnumConfirmationOutcome.Allowed, EnumConfirmationOutcome.Allowed }, outcomes);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref target)))
        {
            if (Interlocked.CompareExchange(ref target, value, current) == current)
                return;
        }
    }

    [TestMethod]
    public async Task CallbackException_ReleasesGate_ForLaterConfirmations()
    {
        var service = CreateService(out var context);
        Configure(context, callback: (_, _) => throw new InvalidOperationException("boom"));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>()));

        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });
        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\b.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
        Assert.IsTrue(callbackInvoked, "上一次回调抛异常不应永久占用确认闸门");
    }

    [TestMethod]
    public async Task SubAgentScope_DeleteTool_PromptsEveryTime_AndDoesNotRemember()
    {
        var service = CreateService(out var context);
        var calls = 0;
        Configure(context, callback: (_, _) => { calls++; return Task.FromResult(ConfirmResult.AllowAll); });

        using (ToolConfirmationContext.EnterSubAgentScope())
        {
            var first = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());
            var second = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

            Assert.AreEqual(EnumConfirmationOutcome.Allowed, first);
            Assert.AreEqual(EnumConfirmationOutcome.Allowed, second);
        }

        Assert.AreEqual(2, calls, "删除类即使选「本轮全部允许」也不记忆，子代理内每次都要确认");
        Assert.IsFalse(context.IsAllowedThisTurn("DeleteFileAsync"));
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
    public void SubAgentAllowAll_NestedScope_OnlyWidens_NeverNarrows()
    {
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            // 内层传 false 不得把外层已全放行的链路降级为逐项确认
            using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false))
            {
                Assert.IsTrue(ToolConfirmationContext.IsSubAgentAllowAll);
            }

            Assert.IsTrue(ToolConfirmationContext.IsSubAgentAllowAll, "内层退出应还原外层全放行状态");
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

    [TestMethod]
    public async Task Default_MemorySave_IsAllowedWithoutCallback()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Deny); });

        var outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
            new Dictionary<string, object?> { ["content"] = "偏好使用中文", ["category"] = "preference" });

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "记忆保存属内置免确认项，不应打断用户");
        Assert.IsFalse(callbackInvoked, "免确认工具不应触发宿主回调");
    }

    [TestMethod]
    [DataRow(LocalMemoryToolGroup.SearchToolName, DisplayName = "MemorySearch 免确认")]
    [DataRow(LocalMemoryToolGroup.ListToolName, DisplayName = "MemoryList 免确认")]
    public async Task Default_MemoryRead_IsAllowedWithoutCallback(string toolName)
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Deny); });

        var outcome = await service.EvaluateAsync(toolName, null,
            new Dictionary<string, object?> { ["query"] = "偏好" });

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "记忆读取属内置免确认项，不应打断用户");
        Assert.IsFalse(callbackInvoked, "免确认工具不应触发宿主回调");
    }

    [TestMethod]
    [DataRow(LocalMemoryToolGroup.SearchToolName, DisplayName = "Plan 下 MemorySearch 放行")]
    [DataRow(LocalMemoryToolGroup.ListToolName, DisplayName = "Plan 下 MemoryList 放行")]
    public async Task Plan_MemoryRead_IsAllowed_WithoutBeingPlanned(string toolName)
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, mode: ToolPermissionMode.Plan,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });
        string? planned = null;
        context.OnPlannedAction = (tool, _) => planned = tool;

        var outcome = await service.EvaluateAsync(toolName, null,
            new Dictionary<string, object?> { ["query"] = "偏好" });

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "读记忆无副作用，Plan 下应放行");
        Assert.IsNull(planned, "只读记忆工具不应记录为计划项");
        Assert.IsFalse(callbackInvoked, "Plan 模式不应弹确认");
    }

    [TestMethod]
    public void ConfiguredAutoConfirmTools_UnionsWithBuiltInDefaults()
    {
        var context = new ToolConfirmationContext();
        var options = new LuBanAgentOptions();
        options.Confirmation.AutoConfirmTools = ["CompactContextAsync"];
        var service = new ToolConfirmationService(context, Options.Create(options));

        Assert.IsTrue(service.AutoConfirmTools.Contains(LocalMemoryToolGroup.SaveToolName),
            "内置默认项不应被宿主配置整体替换");
        Assert.IsTrue(service.AutoConfirmTools.Contains("CompactContextAsync"), "宿主配置项应保留");
    }

    [TestMethod]
    public async Task Plan_MemorySave_IsPlanned_AndDoesNotExecute()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, mode: ToolPermissionMode.Plan,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });
        string? planned = null;
        context.OnPlannedAction = (tool, _) => planned = tool;

        var outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
            new Dictionary<string, object?> { ["content"] = "偏好使用中文", ["category"] = "preference" });

        Assert.AreEqual(EnumConfirmationOutcome.Planned, outcome, "Plan 模式下免确认工具也只记录计划、不执行");
        Assert.AreEqual(LocalMemoryToolGroup.SaveToolName, planned, "Plan 模式下应触发计划项回调");
        Assert.IsFalse(callbackInvoked, "Plan 模式不应弹确认");
    }

    [TestMethod]
    [DataRow(LuBan.AIAgent.Tools.Orchestration.OrchestrationToolGroup.PlanTaskToolName, DisplayName = "plan_task 免确认")]
    [DataRow(LuBan.AIAgent.Tools.Orchestration.OrchestrationToolGroup.RunOrchestrationToolName, DisplayName = "run_orchestration 免确认")]
    public async Task Default_OrchestrationTool_IsAllowedWithoutCallback(string toolName)
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Deny); });

        var outcome = await service.EvaluateAsync(toolName, null,
            new Dictionary<string, object?> { ["task"] = "拆分复合任务" });

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "编排工具属内置免确认项，不应打断用户");
        Assert.IsFalse(callbackInvoked, "免确认工具不应触发宿主回调");
    }

    [TestMethod]
    public async Task Plan_OrchestrationTool_IsPlanned_AndDoesNotExecute()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, mode: ToolPermissionMode.Plan,
            callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });
        string? planned = null;
        context.OnPlannedAction = (tool, _) => planned = tool;

        var outcome = await service.EvaluateAsync(
            LuBan.AIAgent.Tools.Orchestration.OrchestrationToolGroup.PlanTaskToolName, null,
            new Dictionary<string, object?> { ["task"] = "拆分复合任务" });

        Assert.AreEqual(EnumConfirmationOutcome.Planned, outcome, "Plan 模式下编排工具也只记录计划、不执行");
        Assert.AreEqual(LuBan.AIAgent.Tools.Orchestration.OrchestrationToolGroup.PlanTaskToolName, planned,
            "Plan 模式下应触发计划项回调");
        Assert.IsFalse(callbackInvoked, "Plan 模式不应弹确认");
    }

    [TestMethod]
    public void OriginScope_DefaultsToNull_AndRestoresAfterExit()
    {
        Assert.IsNull(ToolConfirmationContext.CurrentOrigin, "主代理调用链没有确认来源");

        var origin = new ToolConfirmationOrigin("build", "coder", "生成构建脚本");
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false, origin: origin))
        {
            Assert.AreSame(origin, ToolConfirmationContext.CurrentOrigin);
            Assert.AreEqual("编排节点 build（角色 coder）：生成构建脚本",
                ToolConfirmationContext.CurrentOrigin!.ToDisplayString());
        }

        Assert.IsNull(ToolConfirmationContext.CurrentOrigin, "退出作用域后来源必须还原");
    }

    [TestMethod]
    public void OriginScope_NestedScopes_RestoreOuterOrigin()
    {
        var outer = new ToolConfirmationOrigin("a");
        var inner = new ToolConfirmationOrigin("b", "tester");

        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false, origin: outer))
        {
            using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false, origin: inner))
            {
                Assert.AreSame(inner, ToolConfirmationContext.CurrentOrigin);
            }

            Assert.AreSame(outer, ToolConfirmationContext.CurrentOrigin, "内层退出应还原外层来源");
        }

        Assert.IsNull(ToolConfirmationContext.CurrentOrigin);
    }

    [TestMethod]
    public void EnterOriginScope_OverridesAndRestores_WithoutSubAgentSemantics()
    {
        var origin = new ToolConfirmationOrigin("remote-node", "reviewer", null);

        using (ToolConfirmationContext.EnterOriginScope(origin))
        {
            Assert.AreSame(origin, ToolConfirmationContext.CurrentOrigin);
            Assert.IsFalse(ToolConfirmationContext.IsInSubAgentScope, "来源作用域不改子代理语义");
        }

        Assert.IsNull(ToolConfirmationContext.CurrentOrigin);
    }

    [TestMethod]
    public void ToolConfirmationOrigin_DisplayString_FallsBackWhenFieldsMissing()
    {
        Assert.IsNull(new ToolConfirmationOrigin().ToDisplayString(), "三字段全空时不显示来源");
        Assert.IsNull(new ToolConfirmationOrigin(" ", null, "").ToDisplayString());
        Assert.AreEqual("编排节点 build", new ToolConfirmationOrigin("build").ToDisplayString());
    }

    [TestMethod]
    public void DefaultAlwaysConfirmTools_IncludesLocalMemoryDelete()
    {
        var service = CreateService(out _);

        Assert.IsTrue(service.AlwaysConfirmTools.Contains(LocalMemoryToolGroup.DeleteToolName),
            "本地记忆删除（LocalMemoryToolGroup.DeleteAsync → MemoryDelete）属破坏性操作，应逐次确认");
    }

    [TestMethod]
    public async Task Default_MemoryDelete_AsksUserEvenWhenPathless()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });

        var outcome = await service.EvaluateAsync(LocalMemoryToolGroup.DeleteToolName, null,
            new Dictionary<string, object?> { ["id"] = "m-1" });

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome);
        Assert.IsTrue(callbackInvoked, "删除类必须确认，不能因「无路径」被放行");
    }

    [TestMethod]
    [DataRow(LocalMemoryToolGroup.SaveToolName, DisplayName = "A2A 全放行下 MemorySave 被拒")]
    [DataRow(LocalMemoryToolGroup.DeleteToolName, DisplayName = "A2A 全放行下 MemoryDelete 被拒")]
    public async Task SubAgentAllowAll_MemoryWrite_IsDenied(string toolName)
    {
        var service = CreateService(out var context);
        Configure(context);

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync(toolName, null,
                new Dictionary<string, object?> { ["content"] = "子代理写入的记忆" });
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome,
            "子代理无人工确认通道，写/删记忆会以用户身份留下长期资产，必须拒绝");
    }

    [TestMethod]
    [DataRow(LocalMemoryToolGroup.SearchToolName, DisplayName = "A2A 全放行下 MemorySearch 放行")]
    [DataRow(LocalMemoryToolGroup.ListToolName, DisplayName = "A2A 全放行下 MemoryList 放行")]
    public async Task SubAgentAllowAll_MemoryRead_IsAllowed(string toolName)
    {
        var service = CreateService(out var context);
        Configure(context);

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            outcome = await service.EvaluateAsync(toolName, null,
                new Dictionary<string, object?> { ["query"] = "偏好" });
        }

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "读取记忆无副作用，子代理可正常使用");
    }

    [TestMethod]
    public async Task SubAgentScope_WithoutAllowAll_MemoryWrite_IsDenied()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false, origin: new ToolConfirmationOrigin("build")))
        {
            outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
                new Dictionary<string, object?> { ["content"] = "子代理写入的记忆" });
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "编排子代理同样不得写记忆");
        Assert.IsFalse(callbackInvoked, "该拒绝来自内置约束，不应弹确认");
    }

    [TestMethod]
    [DataRow(true, DisplayName = "A2A 全放行 + 策略放行：仍不得写记忆")]
    [DataRow(false, DisplayName = "编排子代理 + 策略放行：仍不得写记忆")]
    public async Task PolicyGuard_Allow_DoesNotBypassSubAgentForbiddenTools(bool allowAll)
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Allow); });
        // 策略放行只替代「询问用户」，不能越过子代理禁止名单
        context.PolicyGuard = (_, _) => ConfirmResult.Allow;

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll, origin: new ToolConfirmationOrigin("build")))
        {
            outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
                new Dictionary<string, object?> { ["content"] = "子代理写入的记忆" });
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "禁止名单早于策略放行生效");
        Assert.IsFalse(callbackInvoked, "禁止名单的拒绝不应弹确认");
    }

    [TestMethod]
    public async Task Plan_SubAgent_MemoryWrite_IsPlanned_NotDenied()
    {
        var service = CreateService(out var context);
        string? planned = null;
        Configure(context, mode: ToolPermissionMode.Plan);
        context.OnPlannedAction = (tool, _) => planned = tool;

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false, origin: new ToolConfirmationOrigin("build")))
        {
            outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
                new Dictionary<string, object?> { ["content"] = "计划中的记忆写入" });
        }

        Assert.AreEqual(EnumConfirmationOutcome.Planned, outcome, "Plan 下不执行，但应进计划清单而不是报拒绝");
        Assert.AreEqual(LocalMemoryToolGroup.SaveToolName, planned);
    }

    [TestMethod]
    public async Task PolicyGuard_Deny_BeatsAutoConfirmTool()
    {
        var service = CreateService(out var context);
        Configure(context);
        context.PolicyGuard = (tool, _) => tool == LocalMemoryToolGroup.SaveToolName ? ConfirmResult.Deny : null;

        var outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
            new Dictionary<string, object?> { ["content"] = "偏好使用中文" });

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome,
            "策略拒绝必须优先于内置免确认名单");
    }

    [TestMethod]
    public async Task PolicyGuard_Deny_BeatsBypassMode()
    {
        var service = CreateService(out var context);
        Configure(context, mode: ToolPermissionMode.BypassPermissions);
        context.PolicyGuard = (_, _) => ConfirmResult.Deny;

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "Bypass 也不能越过宿主策略");
    }

    [TestMethod]
    public async Task PolicyGuard_Deny_BeatsSubAgentAllowAll()
    {
        var service = CreateService(out var context);
        Configure(context);
        context.PolicyGuard = (_, _) => ConfirmResult.Deny;

        EnumConfirmationOutcome outcome;
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: true))
        {
            // 用非「子代理不可委派」工具，确保本用例只验证闸门优先级
            outcome = await service.EvaluateAsync("WriteFileAsync", @"C:\work\a.txt",
                new Dictionary<string, object?> { ["content"] = "x" });
        }

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "A2A 全放行也不能越过宿主策略");
    }

    [TestMethod]
    public async Task PolicyGuard_Allow_SkipsUserConfirmation()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Deny); });
        context.PolicyGuard = (_, _) => ConfirmResult.Allow;

        var outcome = await service.EvaluateAsync("WriteFileAsync", @"D:\other\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Allowed, outcome, "策略放行只免除询问");
        Assert.IsFalse(callbackInvoked, "策略放行不应再弹确认");
    }

    [TestMethod]
    public async Task PolicyGuard_Allow_DoesNotExecuteInPlanMode()
    {
        var service = CreateService(out var context);
        Configure(context, mode: ToolPermissionMode.Plan);
        context.PolicyGuard = (_, _) => ConfirmResult.Allow;
        string? planned = null;
        context.OnPlannedAction = (tool, _) => planned = tool;

        var outcome = await service.EvaluateAsync(LocalMemoryToolGroup.SaveToolName, null,
            new Dictionary<string, object?> { ["content"] = "偏好使用中文" });

        Assert.AreEqual(EnumConfirmationOutcome.Planned, outcome, "Plan 模式仍只记计划、不执行");
        Assert.AreEqual(LocalMemoryToolGroup.SaveToolName, planned);
    }

    [TestMethod]
    public async Task PolicyGuard_Allow_StillConfirmsDeletion()
    {
        var service = CreateService(out var context);
        var callbackInvoked = false;
        Configure(context, callback: (_, _) => { callbackInvoked = true; return Task.FromResult(ConfirmResult.Deny); });
        context.PolicyGuard = (_, _) => ConfirmResult.Allow;

        var outcome = await service.EvaluateAsync("DeleteFileAsync", @"C:\work\a.txt", new Dictionary<string, object?>());

        Assert.AreEqual(EnumConfirmationOutcome.Denied, outcome, "删除类不因策略放行而免确认");
        Assert.IsTrue(callbackInvoked, "删除类仍应询问用户");
    }

    [TestMethod]
    public void PolicyGuard_IsClearedByReset()
    {
        var context = new ToolConfirmationContext { PolicyGuard = (_, _) => ConfirmResult.Deny };

        context.Reset();

        Assert.IsNull(context.PolicyGuard, "每轮收尾后策略闸门必须清空，避免串到下一轮");
    }
}