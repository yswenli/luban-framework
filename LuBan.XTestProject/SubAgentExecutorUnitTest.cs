/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： SubAgentExecutorUnitTest
*唯一标识：ISubAgentExecutor 接缝与 DagScheduler 事件消费单测
*创建时间：2026/10/8
*描述：验证 DagScheduler 经 ISubAgentExecutor 消费事件流（输出/会话/活动/失败），
*      以及默认 SubAgentFactory 实现的事件顺序与默认路径行为（纯单元）
*
*****************************************************************************/
using System.Runtime.CompilerServices;
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class SubAgentExecutorUnitTest
{
    private sealed class FakeExecutor : ISubAgentExecutor
    {
        private readonly IReadOnlyList<SubAgentExecutionEvent> _events;

        public SubAgentSpec? Captured { get; private set; }

        public FakeExecutor(params SubAgentExecutionEvent[] events) => _events = events;

        public async IAsyncEnumerable<SubAgentExecutionEvent> ExecuteAsync(
            SubAgentSpec spec, [EnumeratorCancellation] CancellationToken ct = default)
        {
            Captured = spec;
            foreach (var e in _events)
            {
                await Task.Yield();
                yield return e;
            }
        }
    }

    private static DagScheduler Scheduler(ISubAgentExecutor executor)
    {
        var options = Options.Create(new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { MaxParallelism = 4, MaxNodes = 10 }
        });
        return new DagScheduler(executor, new ContextStore(), options);
    }

    private static TaskGraph Single(string id = "a") => new()
    {
        GraphId = "g",
        OriginalTask = "t",
        Nodes = { new TaskNode { Id = id, Prompt = "p" } }
    };

    [TestMethod]
    public async Task ExecuteAsync_CompletedEvent_BecomesNodeOutput_AndStartedCarriesSessionId()
    {
        var executor = new FakeExecutor(
            new SubAgentExecutionEvent(SubAgentEventKind.Started, "sess-1", null, null),
            new SubAgentExecutionEvent(SubAgentEventKind.Text, "sess-1", "hel", null),
            new SubAgentExecutionEvent(SubAgentEventKind.Text, "sess-1", "lo", null),
            new SubAgentExecutionEvent(SubAgentEventKind.Completed, "sess-1", "hello", null));

        var graph = Single();
        var result = await Scheduler(executor).ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual("hello", graph.Nodes[0].Output);
        Assert.AreEqual("sess-1", graph.Nodes[0].SessionId, "Started 事件的 SessionId 应传递到节点");
    }

    [TestMethod]
    public async Task ExecuteAsync_ForwardsActivitiesToProgress()
    {
        var executor = new FakeExecutor(
            new SubAgentExecutionEvent(SubAgentEventKind.Started, "s", null, null),
            new SubAgentExecutionEvent(SubAgentEventKind.Thinking, "s", "think", null),
            new SubAgentExecutionEvent(SubAgentEventKind.ToolCall, "s", "WriteFileAsync", "path=x"),
            new SubAgentExecutionEvent(SubAgentEventKind.ToolResult, "s", "ok", null),
            new SubAgentExecutionEvent(SubAgentEventKind.Text, "s", "body", null),
            new SubAgentExecutionEvent(SubAgentEventKind.Completed, "s", "body", null));

        var activities = new List<NodeActivityItem>();
        Action<OrchestrationProgress> onProgress = p =>
        {
            if (p.EventType == ProgressEventType.NodeActivity && p.Activity is not null)
                activities.Add(p.Activity);
        };

        var graph = Single();
        await Scheduler(executor).ExecuteAsync(graph, CancellationToken.None, onProgress);

        Assert.IsTrue(activities.Any(a => a.Kind == NodeActivityKind.Thinking && a.Content == "think"));
        Assert.IsTrue(activities.Any(a => a.Kind == NodeActivityKind.ToolCall && a.ToolName == "WriteFileAsync" && a.Content == "path=x"));
        Assert.IsTrue(activities.Any(a => a.Kind == NodeActivityKind.ToolResult && a.Content == "ok"));
        Assert.IsTrue(activities.Any(a => a.Kind == NodeActivityKind.Text && a.Content == "body"));
    }

    [TestMethod]
    public async Task ExecuteAsync_FailedEvent_MarksNodeFailed()
    {
        var executor = new FakeExecutor(
            new SubAgentExecutionEvent(SubAgentEventKind.Started, "s", null, null),
            new SubAgentExecutionEvent(SubAgentEventKind.Failed, "s", "boom", null));

        var graph = Single();
        graph.Nodes[0].IsCritical = true;
        var result = await Scheduler(executor).ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("failed", result.OverallStatus);
        Assert.AreEqual(TaskNodeStatus.Failed, graph.Nodes[0].Status);
        StringAssert.Contains(graph.Nodes[0].Error ?? "", "boom");
    }

    [TestMethod]
    public async Task ExecuteAsync_PassesSpecMappedFromNode()
    {
        var executor = new FakeExecutor(
            new SubAgentExecutionEvent(SubAgentEventKind.Started, "s", null, null),
            new SubAgentExecutionEvent(SubAgentEventKind.Completed, "s", "out", null));

        var graph = Single("n1");
        graph.Nodes[0].Role = "coder";
        await Scheduler(executor).ExecuteAsync(graph, CancellationToken.None, null);

        Assert.IsNotNull(executor.Captured);
        Assert.AreEqual("n1", executor.Captured!.NodeId);
        Assert.AreEqual("p", executor.Captured.Prompt);
        Assert.AreEqual("coder", executor.Captured.Role);
        Assert.AreEqual("g", executor.Captured.ParentSessionId);
    }

    [TestMethod]
    public async Task DefaultFactory_ExecuteAsync_EmitsStartedThenCompleted()
    {
        var options = Options.Create(new LuBanAgentOptions());
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var chatClient = new MockChatClient("hello");
        var registry = new ToolPluginRegistry(Array.Empty<ILuBanToolPlugin>(), options);
        var innerFactory = new LuBanAgentFactory(chatClient, registry, options, sp);
        var factory = new SubAgentFactory(innerFactory, new SubAgentRoleRegistry(), options);

        var spec = new SubAgentSpec { NodeId = "n1", Prompt = "do it" };
        var events = new List<SubAgentExecutionEvent>();
        await foreach (var e in factory.ExecuteAsync(spec))
            events.Add(e);

        Assert.IsTrue(events.Count >= 2);
        Assert.AreEqual(SubAgentEventKind.Started, events[0].Kind);
        Assert.IsFalse(string.IsNullOrEmpty(events[0].SessionId), "Started 应携带子代理 SessionId");
        Assert.AreEqual(SubAgentEventKind.Completed, events[^1].Kind);
        StringAssert.Contains(events[^1].Text ?? "", "hello");
    }
}