/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： DagSchedulerUnitTest
*唯一标识：DagScheduler 依赖驱动调度单测
*创建时间：2026/10/8
*描述：通过 NodeRunnerOverride 接缝验证依赖驱动调度：并行度、关键失败传递跳过、
*      非关键失败不阻断、硬上限与不限四种语义（纯单元，不创建真实子代理）
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class DagSchedulerUnitTest
{
    private static DagScheduler CreateScheduler(
        OrchestrationOptions orchestration,
        Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>> runner)
    {
        var options = Options.Create(new LuBanAgentOptions { Orchestration = orchestration });
        return new DagScheduler(new ContextStore(), options) { NodeRunnerOverride = runner };
    }

    private static void UpdateMax(ref int max, int value)
    {
        int snapshot;
        while (value > (snapshot = Volatile.Read(ref max)))
        {
            if (Interlocked.CompareExchange(ref max, value, snapshot) == snapshot)
                break;
        }
    }

    private static TaskGraph Diamond() => new()
    {
        GraphId = "g",
        OriginalTask = "t",
        Nodes =
        {
            new TaskNode { Id = "a" },
            new TaskNode { Id = "b", Dependencies = { "a" } },
            new TaskNode { Id = "c", Dependencies = { "a" } },
            new TaskNode { Id = "d", Dependencies = { "b", "c" } },
        }
    };

    [TestMethod]
    public async Task ExecuteAsync_Diamond_RunsIndependentNodesConcurrently()
    {
        int current = 0, max = 0;
        var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>> runner = async (g, n, p, ct) =>
        {
            UpdateMax(ref max, Interlocked.Increment(ref current));
            try
            {
                if (n.Id == "b")
                {
                    bStarted.TrySetResult();
                    await Task.WhenAny(cStarted.Task, Task.Delay(TimeSpan.FromSeconds(10), ct));
                }
                else if (n.Id == "c")
                {
                    cStarted.TrySetResult();
                    await Task.WhenAny(bStarted.Task, Task.Delay(TimeSpan.FromSeconds(10), ct));
                }
                return "out-" + n.Id;
            }
            finally
            {
                Interlocked.Decrement(ref current);
            }
        };

        var graph = Diamond();
        var scheduler = CreateScheduler(new OrchestrationOptions { MaxParallelism = 10, MaxNodes = 10 }, runner);

        var result = await scheduler.ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(4, result.Nodes.Count);
        Assert.AreEqual(2, max, "b 与 c 应并发执行");
        Assert.IsTrue(graph.Nodes.All(n => n.Status == TaskNodeStatus.Succeeded));
    }

    [TestMethod]
    public async Task ExecuteAsync_CriticalFailure_SkipsTransitiveSuccessors()
    {
        Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>> runner = (g, n, p, ct) =>
            n.Id == "a"
                ? throw new InvalidOperationException("boom")
                : Task.FromResult("ok");

        var graph = new TaskGraph
        {
            GraphId = "g",
            OriginalTask = "t",
            Nodes =
            {
                new TaskNode { Id = "a", IsCritical = true },
                new TaskNode { Id = "b", Dependencies = { "a" } },
                new TaskNode { Id = "c", Dependencies = { "b" } },
            }
        };
        var scheduler = CreateScheduler(new OrchestrationOptions { MaxParallelism = 4, MaxNodes = 10 }, runner);

        var result = await scheduler.ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("failed", result.OverallStatus);
        Assert.AreEqual(TaskNodeStatus.Failed, graph.Nodes.First(n => n.Id == "a").Status);
        Assert.AreEqual(TaskNodeStatus.Skipped, graph.Nodes.First(n => n.Id == "b").Status);
        Assert.AreEqual(TaskNodeStatus.Skipped, graph.Nodes.First(n => n.Id == "c").Status);
        Assert.AreEqual(3, result.Nodes.Count);
    }

    [TestMethod]
    public async Task ExecuteAsync_NonCriticalFailure_DoesNotBlockSuccessors()
    {
        Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>> runner = (g, n, p, ct) =>
            n.Id == "a"
                ? throw new InvalidOperationException("boom")
                : Task.FromResult("ok");

        var graph = new TaskGraph
        {
            GraphId = "g",
            OriginalTask = "t",
            Nodes =
            {
                new TaskNode { Id = "a", IsCritical = false },
                new TaskNode { Id = "b", Dependencies = { "a" } },
            }
        };
        var scheduler = CreateScheduler(new OrchestrationOptions { MaxParallelism = 4, MaxNodes = 10 }, runner);

        var result = await scheduler.ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("partial", result.OverallStatus);
        Assert.AreEqual(TaskNodeStatus.Failed, graph.Nodes.First(n => n.Id == "a").Status);
        Assert.AreEqual(TaskNodeStatus.Succeeded, graph.Nodes.First(n => n.Id == "b").Status);
    }

    [TestMethod]
    public async Task ExecuteAsync_HardLimitOne_NeverOverlaps()
    {
        int current = 0, max = 0;
        var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>> runner = async (g, n, p, ct) =>
        {
            UpdateMax(ref max, Interlocked.Increment(ref current));
            try
            {
                if (n.Id == "b")
                {
                    bStarted.TrySetResult();
                    // 若上限失效导致并发，b/c 会在此会合；正确实现下 b 独占，等待超时后继续。
                    await Task.WhenAny(cStarted.Task, Task.Delay(TimeSpan.FromSeconds(2), ct));
                }
                else if (n.Id == "c")
                {
                    cStarted.TrySetResult();
                    await Task.WhenAny(bStarted.Task, Task.Delay(TimeSpan.FromSeconds(2), ct));
                }
                return "out-" + n.Id;
            }
            finally
            {
                Interlocked.Decrement(ref current);
            }
        };

        var graph = Diamond();
        var scheduler = CreateScheduler(new OrchestrationOptions { MaxParallelism = 1, MaxNodes = 10 }, runner);

        var result = await scheduler.ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(1, max, "硬上限=1 时任意时刻至多 1 个节点运行");
    }

    [TestMethod]
    public async Task ExecuteAsync_Unlimited_RunsConcurrently()
    {
        int current = 0, max = 0;
        var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>> runner = async (g, n, p, ct) =>
        {
            UpdateMax(ref max, Interlocked.Increment(ref current));
            try
            {
                if (n.Id == "b")
                {
                    bStarted.TrySetResult();
                    await Task.WhenAny(cStarted.Task, Task.Delay(TimeSpan.FromSeconds(10), ct));
                }
                else if (n.Id == "c")
                {
                    cStarted.TrySetResult();
                    await Task.WhenAny(bStarted.Task, Task.Delay(TimeSpan.FromSeconds(10), ct));
                }
                return "out-" + n.Id;
            }
            finally
            {
                Interlocked.Decrement(ref current);
            }
        };

        var graph = Diamond();
        var scheduler = CreateScheduler(new OrchestrationOptions { MaxParallelism = 0, MaxNodes = 10 }, runner);

        var result = await scheduler.ExecuteAsync(graph, CancellationToken.None, null);

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(2, max, "不限并行度时 b 与 c 应并发执行");
    }
}