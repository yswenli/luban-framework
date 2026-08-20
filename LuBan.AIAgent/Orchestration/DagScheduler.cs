/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： DagScheduler
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：DAG 调度器，基于拓扑分层实现同层节点并行执行、跨层节点串行执行
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// DAG 调度器，基于拓扑分层实现同层节点并行执行、跨层节点串行执行。
/// </summary>
public class DagScheduler
{
    private readonly SubAgentFactory _subAgentFactory;
    private readonly ContextStore _contextStore;
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建 DagScheduler 实例。
    /// </summary>
    /// <param name="subAgentFactory">SubAgent 工厂。</param>
    /// <param name="contextStore">跨节点上下文存储。</param>
    /// <param name="options">配置选项。</param>
    public DagScheduler(
        SubAgentFactory subAgentFactory,
        ContextStore contextStore,
        IOptions<LuBanAgentOptions> options)
    {
        _subAgentFactory = subAgentFactory;
        _contextStore = contextStore;
        _options = options;
    }

    /// <summary>
    /// 执行任务图谱，返回编排结果。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>编排结果。</returns>
    public async Task<OrchestrationResult> ExecuteAsync(TaskGraph graph, CancellationToken ct = default)
    {
        var result = new OrchestrationResult { GraphId = graph.GraphId, OriginalTask = graph.OriginalTask };
        var sw = Stopwatch.StartNew();

        using var semaphore = CreateParallelismSemaphore();
        var layers = graph.GetTopologicalLayers();
        for (int layerIdx = 0; layerIdx < layers.Count; layerIdx++)
        {
            var layer = layers[layerIdx];
            var tasks = layer.Select(n => ExecuteNodeAsync(graph, n, result, semaphore, ct)).ToList();
            await Task.WhenAll(tasks);

            if (layer.Any(n => n.Status == TaskNodeStatus.Failed && n.IsCritical))
            {
                MarkRemainingAsSkipped(layers, layerIdx, result);
                break;
            }
        }

        sw.Stop();
        result.TotalElapsed = sw.Elapsed;
        result.OverallStatus = DetermineOverallStatus(graph, result);
        return result;
    }

    /// <summary>
    /// 流式执行任务图谱，逐层推送进度事件。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>进度事件流。</returns>
    public async IAsyncEnumerable<OrchestrationProgress> ExecuteStreamingAsync(
        TaskGraph graph,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var result = new OrchestrationResult { GraphId = graph.GraphId, OriginalTask = graph.OriginalTask };
        var sw = Stopwatch.StartNew();

        using var semaphore = CreateParallelismSemaphore();
        var layers = graph.GetTopologicalLayers();
        for (int layerIdx = 0; layerIdx < layers.Count; layerIdx++)
        {
            var layer = layers[layerIdx];
            var layerTasks = new List<Task>();
            foreach (var node in layer)
            {
                layerTasks.Add(ExecuteNodeAsync(graph, node, result, semaphore, ct));
            }
            await Task.WhenAll(layerTasks);

            yield return new OrchestrationProgress
            {
                EventType = ProgressEventType.LayerCompleted,
                Message = $"第 {layerIdx + 1} 层执行完成"
            };

            if (layer.Any(n => n.Status == TaskNodeStatus.Failed && n.IsCritical))
            {
                MarkRemainingAsSkipped(layers, layerIdx, result);
                break;
            }
        }

        sw.Stop();
        result.TotalElapsed = sw.Elapsed;
        result.OverallStatus = DetermineOverallStatus(graph, result);

        yield return new OrchestrationProgress
        {
            EventType = ProgressEventType.OrchestratingCompleted,
            Message = $"编排完成: {result.OverallStatus}"
        };
    }

    /// <summary>
    /// 根据配置创建并行度信号量。MaxParallelism 为 0 时返回 null，表示不限制。
    /// </summary>
    /// <returns>信号量实例或 null。</returns>
    private SemaphoreSlim? CreateParallelismSemaphore()
    {
        var orchestrationOpts = _options.Value.Orchestration ?? new();
        return orchestrationOpts.MaxParallelism > 0
            ? new SemaphoreSlim(orchestrationOpts.MaxParallelism)
            : null;
    }

    /// <summary>
    /// 执行单个节点。整方法体包裹 try-catch，任何异常都标记节点失败，不向上传播。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="node">当前节点。</param>
    /// <param name="result">编排结果（用于追加节点结果）。</param>
    /// <param name="semaphore">并行度信号量，null 表示不限制。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>异步任务。</returns>
    private async Task ExecuteNodeAsync(
        TaskGraph graph, TaskNode node, OrchestrationResult result,
        SemaphoreSlim? semaphore, CancellationToken ct)
    {
        bool semaphoreAcquired = false;
        try
        {
            if (ShouldSkip(graph, node))
            {
                node.Status = TaskNodeStatus.Skipped;
                return;
            }

            if (semaphore != null)
            {
                await semaphore.WaitAsync(ct);
                semaphoreAcquired = true;
            }

            var resolvedPrompt = _contextStore.ResolvePlaceholders(node.Prompt, graph, node);

            node.Status = TaskNodeStatus.Running;
            node.StartedAt = DateTime.UtcNow;

            var spec = new SubAgentSpec
            {
                NodeId = node.Id,
                Prompt = resolvedPrompt,
                ToolGroups = node.ToolGroups,
                ModelName = node.ModelName,
                ParentSessionId = graph.GraphId
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var orchestrationOpts = _options.Value.Orchestration ?? new();
            if (node.TimeoutSeconds.HasValue)
                cts.CancelAfter(TimeSpan.FromSeconds(node.TimeoutSeconds.Value));
            else if (orchestrationOpts.DefaultNodeTimeoutSeconds > 0)
                cts.CancelAfter(TimeSpan.FromSeconds(orchestrationOpts.DefaultNodeTimeoutSeconds));

            var agent = await _subAgentFactory.CreateAsync(spec, ct);
            var response = await agent.RunAsync(resolvedPrompt, cts.Token);
            node.Output = response.Text ?? "";
            node.Status = TaskNodeStatus.Succeeded;
            node.SessionId = spec.SessionId;

            _contextStore.SetOutput(graph.GraphId, node.Id, node.Output);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            node.Status = TaskNodeStatus.Cancelled;
            node.Error = "编排被取消";
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            node.Status = TaskNodeStatus.Failed;
            node.Error = "节点执行超时";
        }
        catch (Exception ex)
        {
            node.Status = TaskNodeStatus.Failed;
            node.Error = ex.Message;
            Logger.Error($"节点 {node.Id} 执行失败", ex);
        }
        finally
        {
            if (semaphoreAcquired)
                semaphore?.Release();
            node.FinishedAt = DateTime.UtcNow;
            result.Nodes.Add(ToNodeResult(node));
        }
    }

    /// <summary>
    /// 检查任一关键前驱节点是否失败，若是则跳过当前节点。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="node">当前节点。</param>
    /// <returns>是否应跳过。</returns>
    private bool ShouldSkip(TaskGraph graph, TaskNode node)
        => node.Dependencies.Any(dep =>
            graph.Nodes.First(n => n.Id == dep).Status == TaskNodeStatus.Failed
            && graph.Nodes.First(n => n.Id == dep).IsCritical);

    /// <summary>
    /// 标记后续层的所有节点为 <see cref="TaskNodeStatus.Skipped"/>。
    /// </summary>
    /// <param name="layers">拓扑分层列表。</param>
    /// <param name="currentIndex">当前层索引。</param>
    /// <param name="result">编排结果。</param>
    private void MarkRemainingAsSkipped(
        List<List<TaskNode>> layers, int currentIndex, OrchestrationResult result)
    {
        for (int i = currentIndex + 1; i < layers.Count; i++)
        {
            foreach (var node in layers[i])
            {
                node.Status = TaskNodeStatus.Skipped;
                result.Nodes.Add(ToNodeResult(node));
            }
        }
    }

    private static NodeResult ToNodeResult(TaskNode node) => new()
    {
        NodeId = node.Id,
        Status = node.Status,
        Output = node.Output,
        Error = node.Error,
        Elapsed = node.FinishedAt.HasValue && node.StartedAt.HasValue
            ? node.FinishedAt.Value - node.StartedAt.Value
            : TimeSpan.Zero
    };

    /// <summary>
    /// 根据节点执行结果判定整体状态（completed / partial / failed / cancelled）。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="result">编排结果。</param>
    /// <returns>整体状态字符串。</returns>
    private static string DetermineOverallStatus(TaskGraph graph, OrchestrationResult result)
    {
        if (result.Nodes.Count == 0) return "failed";

        if (result.Nodes.Any(n => n.Status == TaskNodeStatus.Cancelled))
            return "cancelled";

        if (result.Nodes.All(n => n.Status == TaskNodeStatus.Succeeded)) return "completed";

        var failedIds = result.Nodes
            .Where(n => n.Status == TaskNodeStatus.Failed)
            .Select(n => n.NodeId)
            .ToHashSet();

        var anyCritical = graph.Nodes.Any(n => failedIds.Contains(n.Id) && n.IsCritical);
        return anyCritical ? "failed" : "partial";
    }
}
