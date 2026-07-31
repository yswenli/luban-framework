/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： Orchestrator
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排器默认实现，串联规划、调度与结果聚合
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 编排器默认实现，串联规划、调度与结果聚合。
/// </summary>
public class Orchestrator : IOrchestrator
{
    private readonly ITaskPlanner _planner;
    private readonly DagScheduler _scheduler;
    private readonly ContextStore _contextStore;

    /// <summary>
    /// 创建 Orchestrator 实例。
    /// </summary>
    /// <param name="planner">任务规划器。</param>
    /// <param name="scheduler">DAG 调度器。</param>
    /// <param name="contextStore">跨节点上下文存储。</param>
    public Orchestrator(ITaskPlanner planner, DagScheduler scheduler, ContextStore contextStore)
    {
        _planner = planner;
        _scheduler = scheduler;
        _contextStore = contextStore;
    }

    /// <inheritdoc/>
    public async Task<OrchestrationResult> RunAsync(string task, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(task))
            throw new ArgumentException("任务描述不能为空", nameof(task));

        var graph = await _planner.PlanAsync(task, ct)
            ?? throw new TaskPlanningException("规划器返回空图谱");
        if (!graph.Validate(out var errors))
            throw new TaskPlanningException("DAG 校验失败", errors);

        OrchestrationResult result;
        try
        {
            result = await _scheduler.ExecuteAsync(graph, ct);
        }
        finally
        {
            _contextStore.Clear(graph.GraphId);
        }

        result.FinalOutput = AggregateFinalOutput(graph, result);
        return result;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<OrchestrationProgress> RunStreamingAsync(
        string task,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(task))
            throw new ArgumentException("任务描述不能为空", nameof(task));

        yield return new OrchestrationProgress { EventType = ProgressEventType.PlanningStarted };

        var graph = await _planner.PlanAsync(task, ct)
            ?? throw new TaskPlanningException("规划器返回空图谱");
        if (!graph.Validate(out var errors))
            throw new TaskPlanningException("DAG 校验失败", errors);

        yield return new OrchestrationProgress
        {
            EventType = ProgressEventType.PlanningCompleted,
            Message = $"已生成 {graph.Nodes.Count} 个节点的任务图谱"
        };

        try
        {
            await foreach (var p in _scheduler.ExecuteStreamingAsync(graph, ct))
                yield return p;
        }
        finally
        {
            _contextStore.Clear(graph.GraphId);
        }
    }

    /// <summary>
    /// 聚合终点节点（无后继的节点）输出。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="result">编排结果。</param>
    /// <returns>聚合后的输出字符串。</returns>
    private static string AggregateFinalOutput(TaskGraph graph, OrchestrationResult result)
    {
        var nodeMap = result.Nodes.ToDictionary(n => n.NodeId);
        var hasSuccessor = new HashSet<string>();
        foreach (var n in graph.Nodes)
        {
            foreach (var dep in n.Dependencies)
                hasSuccessor.Add(dep);
        }

        var terminals = graph.Nodes
            .Where(n => !hasSuccessor.Contains(n.Id) && nodeMap[n.Id].Status == TaskNodeStatus.Succeeded)
            .Select(n => $"## {n.Description}\n{nodeMap[n.Id].Output}")
            .ToList();

        return string.Join("\n\n", terminals);
    }
}
