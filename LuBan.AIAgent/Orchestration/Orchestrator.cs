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
using LuBan.AIAgent.Configuration;
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
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建 Orchestrator 实例。
    /// </summary>
    /// <param name="planner">任务规划器。</param>
    /// <param name="scheduler">DAG 调度器。</param>
    /// <param name="contextStore">跨节点上下文存储。</param>
    /// <param name="options">配置选项。</param>
    public Orchestrator(
        ITaskPlanner planner,
        DagScheduler scheduler,
        ContextStore contextStore,
        IOptions<LuBanAgentOptions> options)
    {
        _planner = planner;
        _scheduler = scheduler;
        _contextStore = contextStore;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<OrchestrationResult> RunAsync(string task, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(task))
            throw new ArgumentException("任务描述不能为空", nameof(task));

        var orchestrationOpts = _options.Value.Orchestration ?? new();
        var maxReplan = orchestrationOpts.MaxReplanAttempts;

        var graph = await _planner.PlanAsync(task, ct)
            ?? throw new TaskPlanningException("规划器返回空图谱");
        if (!graph.Validate(out var errors))
            throw new TaskPlanningException("DAG 校验失败", errors);

        var attempt = 0;
        OrchestrationResult? lastResult = null;
        ReflectionResult? reflection = null;
        Dictionary<string, string>? dependencyOutputsSnapshot = null;

        while (attempt <= maxReplan)
        {
            OrchestrationResult result;
            try
            {
                result = await _scheduler.ExecuteAsync(graph, ct);

                if (result.OverallStatus == "failed" && attempt < maxReplan)
                {
                    var failedNodeIds = result.Nodes
                        .Where(n => n.Status == TaskNodeStatus.Failed)
                        .Select(n => n.NodeId)
                        .ToHashSet();

                    dependencyOutputsSnapshot = CaptureDependencyOutputs(graph, failedNodeIds);
                }
            }
            finally
            {
                _contextStore.Clear(graph.GraphId);
            }

            result.FinalOutput = AggregateFinalOutput(graph, result);
            result.ReplanningAttempts = attempt;
            result.Reflection = reflection;
            lastResult = result;

            if (result.OverallStatus != "failed")
                return result;

            if (attempt >= maxReplan)
            {
                result.ReplanningExhausted = true;
                return result;
            }

            attempt++;
            try
            {
                reflection = await PerformReflectionAsync(
                    graph, result, task, attempt, dependencyOutputsSnapshot, ct);
            }
            catch (Exception ex)
            {
                Logger.Warn($"反思阶段失败: {ex.Message}", ex);
                result.ReplanningExhausted = true;
                result.Reflection = new ReflectionResult
                {
                    Analysis = $"反思失败: {ex.Message}",
                    ShouldRetry = false,
                    FailedNodeIds = result.Nodes
                        .Where(n => n.Status == TaskNodeStatus.Failed)
                        .Select(n => n.NodeId).ToList()
                };
                return result;
            }

            result.Reflection = reflection;

            if (!reflection.ShouldRetry || reflection.NewNodes.Count == 0)
            {
                result.ReplanningExhausted = true;
                return result;
            }

            graph = BuildFixGraph(graph, reflection, attempt);
            if (!graph.Validate(out errors))
            {
                result.ReplanningExhausted = true;
                result.Reflection = new ReflectionResult
                {
                    Analysis = $"修正图谱校验失败: {string.Join("; ", errors)}",
                    ShouldRetry = false,
                    FailedNodeIds = reflection.FailedNodeIds
                };
                return result;
            }
        }

        lastResult!.ReplanningExhausted = true;
        return lastResult;
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
    /// 执行反思阶段，分析失败节点并生成修正建议。
    /// </summary>
    private async Task<ReflectionResult> PerformReflectionAsync(
        TaskGraph graph,
        OrchestrationResult result,
        string task,
        int attempt,
        Dictionary<string, string>? dependencyOutputsSnapshot,
        CancellationToken ct)
    {
        var orchestrationOpts = _options.Value.Orchestration ?? new();
        var failedNodeIds = result.Nodes
            .Where(n => n.Status == TaskNodeStatus.Failed)
            .Select(n => n.NodeId)
            .ToHashSet();

        var failedNodes = graph.Nodes
            .Where(n => failedNodeIds.Contains(n.Id) && n.IsCritical)
            .Select(n => new FailedNodeInfo
            {
                NodeId = n.Id,
                Description = n.Description,
                ToolGroups = n.ToolGroups,
                Error = n.Error,
                Output = n.Output,
                DependencyOutputs = GetDependencyOutputsFromSnapshot(n, dependencyOutputsSnapshot)
            })
            .ToList();

        var context = new ReplanContext
        {
            UserGoal = task,
            FailedNodes = failedNodes,
            OriginalGraph = graph,
            Attempt = attempt
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(orchestrationOpts.ReflectionTimeoutSeconds));

        return await _planner.ReflectAsync(context, cts.Token);
    }

    /// <summary>
    /// 在 ContextStore 清空前，捕获失败节点的直接依赖输出。
    /// </summary>
    private Dictionary<string, string> CaptureDependencyOutputs(
        TaskGraph graph,
        HashSet<string> failedNodeIds)
    {
        var outputs = new Dictionary<string, string>();
        foreach (var node in graph.Nodes.Where(n => failedNodeIds.Contains(n.Id)))
        {
            foreach (var depId in node.Dependencies)
            {
                var key = $"{node.Id}:{depId}";
                var output = _contextStore.GetOutput(graph.GraphId, depId);
                if (output != null)
                    outputs[key] = output;
            }
        }
        return outputs;
    }

    /// <summary>
    /// 从快照中获取节点的直接依赖输出。
    /// </summary>
    private static Dictionary<string, string> GetDependencyOutputsFromSnapshot(
        TaskNode node,
        Dictionary<string, string>? snapshot)
    {
        var outputs = new Dictionary<string, string>();
        if (snapshot == null) return outputs;

        foreach (var depId in node.Dependencies)
        {
            var key = $"{node.Id}:{depId}";
            if (snapshot.TryGetValue(key, out var output))
                outputs[depId] = output;
        }
        return outputs;
    }

    /// <summary>
    /// 根据反思结果构建修正图谱。
    /// </summary>
    private static TaskGraph BuildFixGraph(
        TaskGraph originalGraph,
        ReflectionResult reflection,
        int attempt)
    {
        var fixGraph = new TaskGraph
        {
            GraphId = originalGraph.GraphId,
            OriginalTask = originalGraph.OriginalTask,
            Source = "replan"
        };

        var succeededNodeIds = new HashSet<string>(
            originalGraph.Nodes.Where(n => n.Status == TaskNodeStatus.Succeeded).Select(n => n.Id));
        var newNodeIds = new HashSet<string>(
            reflection.NewNodes.Select(n => $"fix_{attempt}_{n.Id}"));

        foreach (var newNode in reflection.NewNodes)
        {
            var prefixedId = $"fix_{attempt}_{newNode.Id}";
            var resolvedDeps = new List<string>();

            foreach (var dep in newNode.Dependencies)
            {
                if (succeededNodeIds.Contains(dep))
                {
                    resolvedDeps.Add(dep);
                }
                else if (newNodeIds.Contains($"fix_{attempt}_{dep}"))
                {
                    resolvedDeps.Add($"fix_{attempt}_{dep}");
                }
            }

            var prefixed = new TaskNode
            {
                Id = prefixedId,
                Description = newNode.Description,
                Prompt = newNode.Prompt,
                ToolGroups = newNode.ToolGroups,
                ModelName = newNode.ModelName,
                TimeoutSeconds = newNode.TimeoutSeconds,
                IsCritical = newNode.IsCritical,
                Dependencies = resolvedDeps
            };
            fixGraph.Nodes.Add(prefixed);
        }

        return fixGraph;
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
