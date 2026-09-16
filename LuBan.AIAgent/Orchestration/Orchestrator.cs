/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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

        var graph = await _planner.PlanAsync(task, ct)
            ?? throw new TaskPlanningException("规划器返回空图谱");
        if (!graph.Validate(out var errors))
            throw new TaskPlanningException("DAG 校验失败", errors);

        return await ExecuteGraphAsync(graph, task, ct);
    }

    /// <inheritdoc/>
    public async Task<OrchestrationResult> RunAsync(TaskGraph graph, CancellationToken ct = default)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));

        if (!graph.Validate(out var errors))
            throw new TaskPlanningException("DAG 校验失败", errors);

        return await ExecuteGraphAsync(graph, graph.OriginalTask, ct);
    }

    /// <inheritdoc/>
    public async Task<OrchestrationResult> RunAsync(TaskGraph graph, Action<OrchestrationProgress>? onProgress, CancellationToken ct = default)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));

        if (!graph.Validate(out var errors))
            throw new TaskPlanningException("DAG 校验失败", errors);

        return await ExecuteGraphAsync(graph, graph.OriginalTask, ct, onProgress);
    }

    /// <summary>
    /// 执行任务图谱的核心逻辑。
    /// </summary>
    private async Task<OrchestrationResult> ExecuteGraphAsync(
        TaskGraph graph,
        string originalTask,
        CancellationToken ct,
        Action<OrchestrationProgress>? onProgress = null)
    {
        var orchestrationOpts = _options.Value.Orchestration ?? new();
        var maxReplan = orchestrationOpts.MaxReplanAttempts;

        var attempt = 0;
        OrchestrationResult? lastResult = null;
        ReflectionResult? reflection = null;
        Dictionary<string, string>? dependencyOutputsSnapshot = null;
        Dictionary<string, string>? succeededOutputsSnapshot = null;

        while (attempt <= maxReplan)
        {
            OrchestrationResult result;
            try
            {
                result = await _scheduler.ExecuteAsync(graph, ct, onProgress);

                if (result.OverallStatus == "failed" && attempt < maxReplan)
                {
                    var failedNodeIds = result.Nodes
                        .Where(n => n.Status == TaskNodeStatus.Failed)
                        .Select(n => n.NodeId)
                        .ToHashSet();

                    dependencyOutputsSnapshot = CaptureDependencyOutputs(graph, failedNodeIds);
                }

                // 在 ContextStore 清空前快照全部成功节点输出：修正图谱只包含新节点，
                // 若其依赖指向已成功的原节点，需要把这些输出内联进 prompt（见 BuildFixGraph）。
                if (attempt < maxReplan)
                    succeededOutputsSnapshot = CaptureSucceededOutputs(graph);
            }
            finally
            {
                _contextStore.Clear(graph.GraphId);
            }

            result.FinalOutput = AggregateFinalOutput(graph, result);

            // 无成功终点节点时（如关键节点超时失败）合成可读摘要，
            // 否则上层会拿到空串并静默结束，界面表现为"输出中断、没有回答"
            if (string.IsNullOrWhiteSpace(result.FinalOutput))
                result.FinalOutput = BuildFallbackOutput(graph, result, orchestrationOpts);

            result.ReplanningAttempts = attempt;
            result.Reflection = reflection;
            lastResult = result;

            Logger.Debug($"[OrchDiag] attempt={attempt} overallStatus={result.OverallStatus} nodes=[{string.Join(", ", result.Nodes.Select(n => $"{n.NodeId}:{n.Status}{(n.Error == null ? "" : $"({n.Error})")}"))}] finalOutputLen={result.FinalOutput.Length}");

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
                onProgress?.Invoke(new OrchestrationProgress
                {
                    EventType = ProgressEventType.ReflectionStarted,
                    Message = $"第 {attempt} 次反思重规划（上次状态: {result.OverallStatus}）"
                });

                reflection = await PerformReflectionAsync(
                    graph, result, originalTask, attempt, dependencyOutputsSnapshot, ct);
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

            graph = BuildFixGraph(graph, reflection, attempt, succeededOutputsSnapshot);
            if (!graph.Validate(out var fixErrors))
            {
                result.ReplanningExhausted = true;
                result.Reflection = new ReflectionResult
                {
                    Analysis = $"修正图谱校验失败: {string.Join("; ", fixErrors)}",
                    ShouldRetry = false,
                    FailedNodeIds = reflection.FailedNodeIds
                };
                return result;
            }
        }

        lastResult!.ReplanningExhausted = true;
        return lastResult;
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
    /// 修正图谱只包含新生成的节点，因此节点依赖分三类处理：
    /// <list type="bullet">
    ///   <item>指向其他新节点：改写成带轮次前缀的节点 Id，保持图内依赖与 <c>{dep:}</c> 占位符可解析。</item>
    ///   <item>指向已成功的原节点：原节点不会进入修正图谱，直接把其输出内联进 prompt 并移除该依赖，
    ///         否则 <see cref="TaskGraph.Validate"/> 会因"依赖的节点不存在"直接判失败（反思重规划几乎必失败）。</item>
    ///   <item>其余（失败/不存在节点）：替换占位符为不可用提示并移除依赖，避免图谱校验失败。</item>
    /// </list>
    /// </summary>
    /// <param name="originalGraph">上一轮执行的任务图谱。</param>
    /// <param name="reflection">反思结果。</param>
    /// <param name="attempt">当前重规划轮次（从 1 开始）。</param>
    /// <param name="succeededOutputs">上一轮成功节点的输出快照（节点 Id → 输出），可为 null。</param>
    /// <returns>修正后的任务图谱。</returns>
    private static TaskGraph BuildFixGraph(
        TaskGraph originalGraph,
        ReflectionResult reflection,
        int attempt,
        IReadOnlyDictionary<string, string>? succeededOutputs)
    {
        var fixGraph = new TaskGraph
        {
            GraphId = originalGraph.GraphId,
            OriginalTask = originalGraph.OriginalTask,
            Source = "replan"
        };

        var prefix = $"fix_{attempt}_";
        var succeededNodeIds = new HashSet<string>(
            originalGraph.Nodes.Where(n => n.Status == TaskNodeStatus.Succeeded).Select(n => n.Id));
        // 新节点统一加上轮次前缀，同时记录"原始 Id → 图谱内 Id"的映射，便于依赖改写
        // （LLM 可能返回重复 Id，这里以最后一次为准，避免 ToDictionary 抛重复键异常）
        var newNodeIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var n in reflection.NewNodes)
            newNodeIdMap[n.Id] = n.Id.StartsWith(prefix, StringComparison.Ordinal) ? n.Id : prefix + n.Id;

        var newNodeIdSet = new HashSet<string>(newNodeIdMap.Values, StringComparer.Ordinal);

        var emitted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var newNode in reflection.NewNodes)
        {
            var prefixedId = newNodeIdMap[newNode.Id];
            if (!emitted.Add(prefixedId))
                continue;

            var resolvedDeps = new HashSet<string>(StringComparer.Ordinal);
            var prompt = newNode.Prompt ?? "";

            foreach (var dep in newNode.Dependencies)
            {
                var mappedDep = dep.StartsWith(prefix, StringComparison.Ordinal) ? dep : prefix + dep;

                if (newNodeIdSet.Contains(mappedDep) && !string.Equals(mappedDep, prefixedId, StringComparison.Ordinal))
                {
                    // 指向同轮新节点：保留为图内依赖，并把 prompt 占位符改写成图谱内 Id
                    resolvedDeps.Add(mappedDep);
                    prompt = prompt.Replace($"{{dep:{dep}}}", $"{{dep:{mappedDep}}}");
                }
                else if (succeededNodeIds.Contains(dep))
                {
                    // 指向已成功原节点：原节点不进入修正图谱，内联其输出后移除依赖
                    var output = succeededOutputs != null && succeededOutputs.TryGetValue(dep, out var o) && !string.IsNullOrWhiteSpace(o)
                        ? o
                        : "[前驱节点无输出]";
                    prompt = prompt.Replace($"{{dep:{dep}}}", output);
                }
                else
                {
                    // 失败/不存在的节点：给出提示并移除依赖，避免图谱校验失败
                    prompt = prompt.Replace($"{{dep:{dep}}}", $"[前驱节点 {dep} 不可用]");
                }
            }

            fixGraph.Nodes.Add(new TaskNode
            {
                Id = prefixedId,
                Description = newNode.Description,
                Role = newNode.Role,
                Prompt = prompt,
                ToolGroups = newNode.ToolGroups,
                ModelName = newNode.ModelName,
                TimeoutSeconds = newNode.TimeoutSeconds,
                IsCritical = newNode.IsCritical,
                Dependencies = resolvedDeps.ToList()
            });
        }

        return fixGraph;
    }

    /// <summary>
    /// 在 ContextStore 清空前，快照全部成功节点的输出，供修正图谱内联使用。
    /// </summary>
    private Dictionary<string, string> CaptureSucceededOutputs(TaskGraph graph)
    {
        var outputs = new Dictionary<string, string>();
        foreach (var node in graph.Nodes.Where(n => n.Status == TaskNodeStatus.Succeeded))
        {
            var output = _contextStore.GetOutput(graph.GraphId, node.Id);
            if (!string.IsNullOrWhiteSpace(output))
                outputs[node.Id] = output;
        }
        return outputs;
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

    /// <summary>
    /// 编排未产出任何成功终点节点输出时，合成一条可读的兜底文本，
    /// 避免上层拿到空串后静默结束（界面表现为"输出中断、没有回答"）。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="result">编排结果。</param>
    /// <param name="options">编排配置。</param>
    /// <returns>兜底输出文本。</returns>
    private static string BuildFallbackOutput(TaskGraph graph, OrchestrationResult result, OrchestrationOptions options)
    {
        var descriptionMap = graph.Nodes.ToDictionary(n => n.Id, n => n.Description);
        string Describe(NodeResult n)
            => descriptionMap.TryGetValue(n.NodeId, out var d) && !string.IsNullOrWhiteSpace(d) ? d : n.NodeId;

        var failed = result.Nodes.Where(n => n.Status == TaskNodeStatus.Failed).ToList();
        var skipped = result.Nodes.Where(n => n.Status == TaskNodeStatus.Skipped).ToList();

        if (failed.Count == 0 && result.Nodes.Any(n => n.Status == TaskNodeStatus.Cancelled))
            return "编排已取消，未产出结果。";

        var sb = new StringBuilder();
        sb.Append(result.OverallStatus == "partial" ? "编排未产出可用结果：" : "编排未能完成：");
        sb.Append(failed.Count == 0
            ? "没有节点成功产出结果"
            : string.Join("；", failed.Select(n => $"{Describe(n)}（{n.Error ?? "未知原因"}）")));

        if (skipped.Count > 0)
            sb.Append($"；{string.Join("、", skipped.Select(Describe))} 因前驱失败被跳过");

        if (failed.Any(n => string.Equals(n.Error, "节点执行超时", StringComparison.Ordinal)) && options.DefaultNodeTimeoutSeconds > 0)
            sb.Append($"；单节点超时阈值为 {options.DefaultNodeTimeoutSeconds} 秒（节点可单独覆盖 TimeoutSeconds），可在 Orchestration.DefaultNodeTimeoutSeconds 调大");

        if (result.ReplanningExhausted)
            sb.Append("；重规划已耗尽");

        sb.Append("。建议把任务拆得更小后重试。");
        return sb.ToString();
    }
}
