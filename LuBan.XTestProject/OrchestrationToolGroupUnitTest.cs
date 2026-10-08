/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： OrchestrationToolGroupUnitTest
*唯一标识：编排工具化单测
*创建时间：2026/9/23
*描述：验证 plan_task 判定/登记、run_orchestration 取用与清理（手写 fake，不访问网络）
*
*****************************************************************************/
using System.Text.Json;

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;
using LuBan.AIAgent.Tools.Orchestration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class OrchestrationToolGroupUnitTest
{
    private sealed class FakeConfirmation : IToolConfirmationService
    {
        public EnumConfirmationOutcome Outcome { get; set; } = EnumConfirmationOutcome.Allowed;

        public string? LastTool { get; private set; }

        public IReadOnlyDictionary<string, object?>? LastArguments { get; private set; }

        public HashSet<string> AutoConfirmTools { get; set; } = [];

        public HashSet<string> AlwaysConfirmTools { get; set; } = [];

        public HashSet<string> ReadOnlyTools { get; set; } = [];

        public Task<EnumConfirmationOutcome> EvaluateAsync(
            string toolName, string? path, IReadOnlyDictionary<string, object?> arguments)
        {
            LastTool = toolName;
            LastArguments = arguments;
            return Task.FromResult(Outcome);
        }

        public Task<bool> RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
            => Task.FromResult(true);

        public Task<bool> TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments)
            => Task.FromResult(true);

        public string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200)
            => string.Empty;
    }

    private sealed class FakePlanner : ITaskPlanner
    {
        public TaskGraph? Graph { get; set; }

        public Exception? ThrowOnPlan { get; set; }

        public Task<TaskGraph?> PlanAsync(string task, CancellationToken ct = default)
            => ThrowOnPlan is null
                ? Task.FromResult(Graph)
                : Task.FromException<TaskGraph?>(ThrowOnPlan);

        public Task<ReflectionResult> ReflectAsync(ReplanContext context, CancellationToken ct = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeOrchestrator : IOrchestrator
    {
        public OrchestrationResult? Result { get; set; }

        public Exception? ThrowOnRun { get; set; }

        public Task<OrchestrationResult> RunAsync(string task, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<OrchestrationResult> RunAsync(TaskGraph graph, CancellationToken cancellationToken = default)
            => ThrowOnRun is null
                ? Task.FromResult(Result ?? new OrchestrationResult { OverallStatus = "completed" })
                : Task.FromException<OrchestrationResult>(ThrowOnRun);
    }

    private sealed class FakeSink : IOrchestrationProgressSink
    {
        private readonly object _gate = new();

        public List<OrchestrationProgress> Events { get; } = [];

        public void Publish(OrchestrationProgress progress)
        {
            lock (_gate)
                Events.Add(progress);
        }
    }

    private static TaskGraph BuildGraph(int nodeCount)
    {
        var graph = new TaskGraph { OriginalTask = "复合任务" };
        for (var i = 0; i < nodeCount; i++)
        {
            graph.Nodes.Add(new TaskNode
            {
                Id = $"n{i}",
                Description = $"节点{i}",
                Prompt = $"p{i}",
                Dependencies = i == 0 ? [] : [$"n{i - 1}"],
                ToolGroups = i == 1 ? ["filesystem"] : null
            });
        }
        return graph;
    }

    private static OrchestrationToolGroup CreateGroup(
        FakePlanner planner,
        FakeOrchestrator orchestrator,
        out GraphPlanStore store,
        out FakeSink sink,
        out FakeConfirmation confirmation,
        EnumConfirmationOutcome outcome = EnumConfirmationOutcome.Allowed,
        int maxParallelism = 3)
    {
        store = new GraphPlanStore();
        sink = new FakeSink();
        confirmation = new FakeConfirmation { Outcome = outcome };

        var services = new ServiceCollection();
        services.AddSingleton<IToolConfirmationService>(confirmation);
        services.AddSingleton<ITaskPlanner>(planner);
        services.AddSingleton<IOrchestrator>(orchestrator);
        services.AddSingleton<IOrchestrationProgressSink>(sink);
        services.AddSingleton(store);
        services.AddSingleton<IOptions<LuBanAgentOptions>>(
            Options.Create(new LuBanAgentOptions
            {
                Orchestration = new OrchestrationOptions { MaxParallelism = maxParallelism }
            }));

        return new OrchestrationToolGroup(services.BuildServiceProvider());
    }

    [TestMethod]
    public async Task PlanTask_SingleNode_DeclinesAndStoreEmpty()
    {
        var planner = new FakePlanner { Graph = BuildGraph(1) };
        var group = CreateGroup(planner, new FakeOrchestrator(), out var store, out var sink, out var confirmation);

        var result = await group.PlanTaskAsync("简单任务");

        Assert.IsTrue(result.IsSuccess);
        using var doc = JsonDocument.Parse(result.Data!);
        Assert.IsFalse(doc.RootElement.GetProperty("orchestrate").GetBoolean());
        Assert.AreEqual("single-node", doc.RootElement.GetProperty("reason").GetString());
        Assert.AreEqual(0, store.Count);
        Assert.AreEqual(OrchestrationToolGroup.PlanTaskToolName, confirmation.LastTool);
        Assert.AreEqual(1, sink.Events.Count);
        Assert.AreEqual(ProgressEventType.PlanningCompleted, sink.Events[0].EventType);
    }

    [TestMethod]
    public async Task PlanTask_NullGraph_Declines()
    {
        var planner = new FakePlanner { Graph = null };
        var group = CreateGroup(planner, new FakeOrchestrator(), out var store, out _, out _);

        var result = await group.PlanTaskAsync("复合任务");

        Assert.IsTrue(result.IsSuccess);
        using var doc = JsonDocument.Parse(result.Data!);
        Assert.IsFalse(doc.RootElement.GetProperty("orchestrate").GetBoolean());
        Assert.AreEqual("single-node", doc.RootElement.GetProperty("reason").GetString());
        Assert.AreEqual(0, store.Count);
    }

    [TestMethod]
    public async Task PlanTask_MultiNode_ReturnsGraphIdAndStoreRetrievable()
    {
        var graph = BuildGraph(3);
        var planner = new FakePlanner { Graph = graph };
        var group = CreateGroup(planner, new FakeOrchestrator(), out var store, out var sink, out _);

        var result = await group.PlanTaskAsync("复合任务");

        Assert.IsTrue(result.IsSuccess);
        using var doc = JsonDocument.Parse(result.Data!);
        var root = doc.RootElement;
        Assert.IsTrue(root.GetProperty("orchestrate").GetBoolean());
        Assert.AreEqual(3, root.GetProperty("nodeCount").GetInt32());
        Assert.AreEqual(3, root.GetProperty("parallelism").GetInt32());

        var graphId = root.GetProperty("graphId").GetString()!;
        Assert.AreSame(graph, store.Get(graphId));

        var nodes = root.GetProperty("nodes");
        Assert.AreEqual(3, nodes.GetArrayLength());
        Assert.AreEqual("n0", nodes[0].GetProperty("id").GetString());
        Assert.AreEqual("节点1", nodes[1].GetProperty("description").GetString());
        Assert.AreEqual(0, nodes[0].GetProperty("dependencies").GetArrayLength());
        Assert.AreEqual("n0", nodes[1].GetProperty("dependencies")[0].GetString());
        Assert.AreEqual(1, nodes[1].GetProperty("toolGroups").GetArrayLength());

        Assert.AreEqual(1, sink.Events.Count);
        StringAssert.Contains(sink.Events[0].Message, "3");
    }

    [TestMethod]
    public async Task PlanTask_InvalidDag_DeclinesInvalidDag()
    {
        var graph = BuildGraph(2);
        graph.Nodes[1].Id = "n0"; // 重复 ID → Validate 失败
        var planner = new FakePlanner { Graph = graph };
        var group = CreateGroup(planner, new FakeOrchestrator(), out var store, out _, out _);

        var result = await group.PlanTaskAsync("复合任务");

        Assert.IsTrue(result.IsSuccess);
        using var doc = JsonDocument.Parse(result.Data!);
        Assert.IsFalse(doc.RootElement.GetProperty("orchestrate").GetBoolean());
        Assert.AreEqual("invalid-dag", doc.RootElement.GetProperty("reason").GetString());
        Assert.AreEqual(0, store.Count);
    }

    [TestMethod]
    public async Task PlanTask_ZeroParallelism_FallsBackToNodeCount()
    {
        var planner = new FakePlanner { Graph = BuildGraph(4) };
        var group = CreateGroup(planner, new FakeOrchestrator(), out _, out _, out _, maxParallelism: 0);

        var result = await group.PlanTaskAsync("复合任务");

        using var doc = JsonDocument.Parse(result.Data!);
        Assert.AreEqual(4, doc.RootElement.GetProperty("parallelism").GetInt32());
    }

    [TestMethod]
    public async Task PlanTask_PlanMode_ReturnsPlannedResult()
    {
        var planner = new FakePlanner { Graph = BuildGraph(3) };
        var group = CreateGroup(planner, new FakeOrchestrator(), out var store, out _,
            out _, outcome: EnumConfirmationOutcome.Planned);

        var result = await group.PlanTaskAsync("复合任务");

        Assert.IsTrue(result.Planned);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(0, store.Count);
    }

    [TestMethod]
    public async Task PlanTask_Denied_ReturnsCancelledResult()
    {
        var planner = new FakePlanner { Graph = BuildGraph(3) };
        var group = CreateGroup(planner, new FakeOrchestrator(), out var store, out _,
            out _, outcome: EnumConfirmationOutcome.Denied);

        var result = await group.PlanTaskAsync("复合任务");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.UserCancelled);
        Assert.AreEqual(0, store.Count);
    }

    [TestMethod]
    public async Task RunOrchestration_UnknownGraphId_Fails()
    {
        var group = CreateGroup(new FakePlanner(), new FakeOrchestrator(), out _, out _, out var confirmation);

        var result = await group.RunOrchestrationAsync("missing-graph");

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "图谱不存在或已过期");
        Assert.AreEqual(OrchestrationToolGroup.RunOrchestrationToolName, confirmation.LastTool);
        Assert.AreEqual("missing-graph", confirmation.LastArguments!["graphId"]);
    }

    [TestMethod]
    public async Task RunOrchestration_Success_RemovesFromStore()
    {
        var graph = BuildGraph(3);
        var planner = new FakePlanner { Graph = graph };
        var orchestrator = new FakeOrchestrator();
        var group = CreateGroup(planner, orchestrator, out var store, out _, out _);
        var plan = await group.PlanTaskAsync("复合任务");
        using var doc = JsonDocument.Parse(plan.Data!);
        var graphId = doc.RootElement.GetProperty("graphId").GetString()!;

        var result = await group.RunOrchestrationAsync(graphId);

        Assert.IsTrue(result.IsSuccess);
        StringAssert.Contains(result.Data, "编排状态: completed");
        Assert.IsNull(store.Get(graphId));
    }

    [TestMethod]
    public async Task RunOrchestration_Failure_KeepsGraphForRetry()
    {
        var graph = BuildGraph(3);
        var planner = new FakePlanner { Graph = graph };
        var orchestrator = new FakeOrchestrator { ThrowOnRun = new InvalidOperationException("boom") };
        var group = CreateGroup(planner, orchestrator, out var store, out _, out _);
        var plan = await group.PlanTaskAsync("复合任务");
        using var doc = JsonDocument.Parse(plan.Data!);
        var graphId = doc.RootElement.GetProperty("graphId").GetString()!;

        var result = await group.RunOrchestrationAsync(graphId);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message, "编排失败");
        Assert.AreSame(graph, store.Get(graphId));
    }
}