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
    private readonly ISubAgentExecutor _subAgentExecutor;
    private readonly ContextStore _contextStore;
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 测试接缝：非 null 时替代真实子代理执行（返回节点输出），用于在不创建 LLM 子代理的
    /// 前提下驱动调度算法（并行度、状态传播、传递跳过）。仅测试使用。
    /// </summary>
    internal Func<TaskGraph, TaskNode, string, CancellationToken, Task<string>>? NodeRunnerOverride { get; set; }

    /// <summary>
    /// 创建 DagScheduler 实例。
    /// </summary>
    /// <param name="subAgentExecutor">子代理执行器（默认 <see cref="SubAgentFactory"/>；启用 A2A 后为进程内传输实现）。</param>
    /// <param name="contextStore">跨节点上下文存储。</param>
    /// <param name="options">配置选项。</param>
    public DagScheduler(
        ISubAgentExecutor subAgentExecutor,
        ContextStore contextStore,
        IOptions<LuBanAgentOptions> options)
    {
        _subAgentExecutor = subAgentExecutor;
        _contextStore = contextStore;
        _options = options;
    }

    /// <summary>
    /// 测试专用构造：不提供 <c>ISubAgentExecutor</c>，仅当设置了 <see cref="NodeRunnerOverride"/> 时可用。
    /// 仅测试使用。
    /// </summary>
    /// <param name="contextStore">跨节点上下文存储。</param>
    /// <param name="options">配置选项。</param>
    internal DagScheduler(ContextStore contextStore, IOptions<LuBanAgentOptions> options)
    {
        _subAgentExecutor = null!;
        _contextStore = contextStore;
        _options = options;
    }

    /// <summary>
    /// 执行任务图谱，返回编排结果。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>编排结果。</returns>
    public Task<OrchestrationResult> ExecuteAsync(TaskGraph graph, CancellationToken ct = default)
        => ExecuteAsync(graph, ct, null);

    /// <summary>
    /// 执行任务图谱，返回编排结果，并在执行过程中实时上报节点级进度事件。
    /// 采用依赖驱动动态调度：节点在其全部前驱完成后即可启动，不再按拓扑层串行。
    /// 回调由并行节点共同触发，内部已串行化，调用方无需额外加锁。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="ct">取消令牌。</param>
    /// <param name="onProgress">进度回调，可为 null。</param>
    /// <returns>编排结果。</returns>
    public async Task<OrchestrationResult> ExecuteAsync(
        TaskGraph graph,
        CancellationToken ct,
        Action<OrchestrationProgress>? onProgress)
    {
        var result = new OrchestrationResult { GraphId = graph.GraphId, OriginalTask = graph.OriginalTask };
        var sw = Stopwatch.StartNew();

        var orchestrationOpts = _options.Value.Orchestration ?? new();
        var limit = ResolveParallelismLimit(graph, orchestrationOpts);
        using var semaphore = limit > 0 ? new SemaphoreSlim(limit) : null;
        var reportGate = new object();

        var indegree = graph.Nodes.ToDictionary(n => n.Id, n => n.Dependencies.Count);
        var dependents = graph.Nodes.ToDictionary(n => n.Id, _ => new List<TaskNode>());
        foreach (var n in graph.Nodes)
        {
            foreach (var dep in n.Dependencies)
                dependents[dep].Add(n);
        }

        var ready = new Queue<TaskNode>(graph.Nodes.Where(n => indegree[n.Id] == 0));
        var running = new List<(TaskNode Node, Task Task)>();

        while (ready.Count > 0 || running.Count > 0)
        {
            while (ready.Count > 0)
            {
                var node = ready.Dequeue();
                if (node.Status == TaskNodeStatus.Skipped)
                    continue;
                running.Add((node, ExecuteNodeAsync(graph, node, result, semaphore, ct, onProgress, reportGate)));
            }

            if (running.Count == 0)
                break;

            var finishedTask = await Task.WhenAny(running.Select(r => r.Task)).ConfigureAwait(false);
            var idx = running.FindIndex(r => ReferenceEquals(r.Task, finishedTask));
            var finishedNode = running[idx].Node;
            running.RemoveAt(idx);
            await finishedTask.ConfigureAwait(false);

            foreach (var child in dependents[finishedNode.Id])
            {
                if (child.Status == TaskNodeStatus.Skipped)
                    continue;

                if (HasFailedCriticalDependency(graph, child))
                {
                    SkipTransitiveSuccessors(dependents, child, result, onProgress, reportGate);
                }
                else if (--indegree[child.Id] == 0)
                {
                    ready.Enqueue(child);
                }
            }
        }

        sw.Stop();
        result.TotalElapsed = sw.Elapsed;
        result.OverallStatus = DetermineOverallStatus(graph, result);
        return result;
    }

    /// <summary>
    /// 解析本次执行的并行度上限：优先取图级建议 <see cref="TaskGraph.Parallelism"/>（&gt;0 时有效），
    /// 否则取配置 <see cref="OrchestrationOptions.MaxParallelism"/>；当配置 &gt; 0 时再取二者较小值。
    /// 返回 0 表示不限制。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="orchestrationOpts">编排配置。</param>
    /// <returns>并行度上限，0 表示不限。</returns>
    private static int ResolveParallelismLimit(TaskGraph graph, OrchestrationOptions orchestrationOpts)
    {
        var effective = graph.Parallelism is > 0 ? graph.Parallelism.Value : orchestrationOpts.MaxParallelism;
        if (orchestrationOpts.MaxParallelism > 0)
            effective = Math.Min(effective, orchestrationOpts.MaxParallelism);
        return effective > 0 ? effective : 0;
    }

    /// <summary>
    /// 判断节点是否存在「已失败且关键」的直接前驱。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="node">当前节点。</param>
    /// <returns>存在返回 true。</returns>
    private static bool HasFailedCriticalDependency(TaskGraph graph, TaskNode node)
        => node.Dependencies.Any(dep =>
        {
            var depNode = graph.Nodes.FirstOrDefault(n => n.Id == dep);
            return depNode is { Status: TaskNodeStatus.Failed, IsCritical: true };
        });

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
        SemaphoreSlim? semaphore, CancellationToken ct,
        Action<OrchestrationProgress>? onProgress = null,
        object? reportGate = null)
    {
        bool semaphoreAcquired = false;
        try
        {
            if (semaphore != null)
            {
                await semaphore.WaitAsync(ct);
                semaphoreAcquired = true;
            }

            var resolvedPrompt = _contextStore.ResolvePlaceholders(node.Prompt, graph, node);

            node.Status = TaskNodeStatus.Running;
            node.StartedAt = DateTime.UtcNow;
            Report(onProgress, reportGate, new OrchestrationProgress
            {
                EventType = ProgressEventType.NodeStarted,
                NodeId = node.Id,
                Message = node.Description
            });

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var orchestrationOpts = _options.Value.Orchestration ?? new();
            if (node.TimeoutSeconds.HasValue)
            {
                // 节点显式指定：>0 限时；0/负数表示无限期，不再回落到全局默认
                if (node.TimeoutSeconds.Value > 0)
                    cts.CancelAfter(TimeSpan.FromSeconds(node.TimeoutSeconds.Value));
            }
            else if (orchestrationOpts.DefaultNodeTimeoutSeconds > 0)
            {
                cts.CancelAfter(TimeSpan.FromSeconds(orchestrationOpts.DefaultNodeTimeoutSeconds));
            }

            var output = await RunSubAgentAsync(graph, node, resolvedPrompt, cts.Token, onProgress, reportGate);

            node.Output = output;
            node.Status = TaskNodeStatus.Succeeded;

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
            var nodeResult = ToNodeResult(node);

            Logger.Info($"[OrchDiag] node done: id={node.Id} status={node.Status} elapsed={nodeResult.Elapsed.TotalSeconds:F1}s outLen={(node.Output ?? "").Length} error={node.Error ?? "-"}");

            result.Nodes.Add(nodeResult);

            Report(onProgress, reportGate, new OrchestrationProgress
            {
                EventType = node.Status switch
                {
                    TaskNodeStatus.Failed => ProgressEventType.NodeFailed,
                    TaskNodeStatus.Cancelled => ProgressEventType.NodeFailed,
                    TaskNodeStatus.Skipped => ProgressEventType.NodeSkipped,
                    TaskNodeStatus.Succeeded => ProgressEventType.NodeCompleted,
                    _ => ProgressEventType.NodeCompleted
                },
                NodeId = node.Id,
                Message = node.Description,
                NodeResult = nodeResult,
                ElapsedMs = (long)nodeResult.Elapsed.TotalMilliseconds
            });
        }
    }

    /// <summary>
    /// 执行单个节点：优先走测试接缝 <see cref="NodeRunnerOverride"/>，否则创建真实子代理并流式执行。
    /// 流式执行把子 Agent 的思考/正文/工具调用/工具结果按时间轴上报，使上层 UI 能实时看到子代理在做什么。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <param name="node">当前节点。</param>
    /// <param name="resolvedPrompt">已解析占位符的节点 prompt。</param>
    /// <param name="ct">节点级取消令牌（已含超时）。</param>
    /// <param name="onProgress">进度回调，可为 null。</param>
    /// <param name="reportGate">进度上报串行化锁对象。</param>
    /// <returns>节点输出文本。</returns>
    private async Task<string> RunSubAgentAsync(
        TaskGraph graph, TaskNode node, string resolvedPrompt, CancellationToken ct,
        Action<OrchestrationProgress>? onProgress, object? reportGate)
    {
        if (NodeRunnerOverride is { } runner)
        {
            return await runner(graph, node, resolvedPrompt, ct) ?? string.Empty;
        }

        var spec = new SubAgentSpec
        {
            NodeId = node.Id,
            Prompt = resolvedPrompt,
            Role = node.Role,
            ToolGroups = node.ToolGroups,
            ModelName = node.ModelName,
            ParentSessionId = graph.GraphId,
            WorkspaceRoot = _options.Value.WorkspaceRoot,
            SharedContext = graph.SharedContext
        };

        Logger.Info($"[OrchDiag] node exec: id={node.Id} node.role={(node.Role ?? "null")} spec.role={(spec.Role ?? "null")} node.tools={(node.ToolGroups == null ? "null" : $"[{string.Join(",", node.ToolGroups)}]")} promptLen={resolvedPrompt.Length}");

        var reporter = new NodeActivityReporter(node.Id, onProgress, reportGate);
        var swRun = Stopwatch.StartNew();
        var output = string.Empty;
        var failed = false;
        string? failureMessage = null;

        // 子代理执行（含创建）委托给 ISubAgentExecutor；确认作用域由执行器内部管理。
        await foreach (var evt in _subAgentExecutor.ExecuteAsync(spec, ct).WithCancellation(ct).ConfigureAwait(false))
        {
            switch (evt.Kind)
            {
                case SubAgentEventKind.Started:
                    if (!string.IsNullOrEmpty(evt.SessionId))
                        node.SessionId = evt.SessionId;
                    break;

                case SubAgentEventKind.Thinking:
                    if (!string.IsNullOrEmpty(evt.Text))
                        reporter.Thinking(evt.Text);
                    break;

                case SubAgentEventKind.Text:
                    if (!string.IsNullOrEmpty(evt.Text))
                        reporter.Text(evt.Text);
                    break;

                case SubAgentEventKind.ToolCall:
                    reporter.ToolCall(evt.Text ?? string.Empty, evt.Detail);
                    break;

                case SubAgentEventKind.ToolResult:
                    reporter.ToolResult(evt.Text);
                    break;

                case SubAgentEventKind.Completed:
                    output = evt.Text ?? string.Empty;
                    break;

                case SubAgentEventKind.Failed:
                    failed = true;
                    failureMessage = evt.Text;
                    break;
            }
        }

        reporter.Flush();
        swRun.Stop();

        node.SessionId ??= spec.SessionId;

        Logger.Info($"[OrchDiag] node timing: id={node.Id} run={swRun.Elapsed.TotalSeconds:F1}s");

        if (failed)
            throw new InvalidOperationException(failureMessage ?? $"节点 '{node.Id}' 子代理执行失败");

        return output;
    }

    /// <summary>
    /// 串行化上报进度事件：同层并行节点共用回调，避免并发调用破坏订阅方线程假设。
    /// </summary>
    private static void Report(
        Action<OrchestrationProgress>? onProgress,
        object? gate,
        OrchestrationProgress progress)
    {
        if (onProgress == null) return;
        try
        {
            if (gate == null)
            {
                onProgress(progress);
                return;
            }
            lock (gate) onProgress(progress);
        }
        catch (Exception ex)
        {
            Logger.Warn($"编排进度上报失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 节点内部活动上报器：把子 Agent 的流式内容转成 <see cref="ProgressEventType.NodeActivity"/> 事件。
    /// 文本类增量按时间窗合并（避免逐 token 上报压垮 UI 线程），工具调用/结果即时上报；
    /// 沿用 <c>reportGate</c> 与节点级事件共用同一把锁，保证订阅方看到的顺序稳定。
    /// </summary>
    private sealed class NodeActivityReporter
    {
        /// <summary>文本增量合并时间窗（毫秒）。</summary>
        private const int ThrottleMs = 150;

        private readonly string _nodeId;
        private readonly Action<OrchestrationProgress>? _onProgress;
        private readonly object? _gate;
        private readonly StringBuilder _pendingText = new();
        private readonly StringBuilder _pendingThinking = new();
        private readonly Stopwatch _sinceLastReport = Stopwatch.StartNew();
        private NodeActivityKind _pendingKind = NodeActivityKind.Text;

        /// <summary>
        /// 创建节点活动上报器。
        /// </summary>
        /// <param name="nodeId">节点标识。</param>
        /// <param name="onProgress">进度回调。</param>
        /// <param name="gate">串行化锁对象。</param>
        public NodeActivityReporter(string nodeId, Action<OrchestrationProgress>? onProgress, object? gate)
        {
            _nodeId = nodeId;
            _onProgress = onProgress;
            _gate = gate;
        }

        /// <summary>上报思考增量。</summary>
        /// <param name="text">思考文本增量。</param>
        public void Thinking(string text)
        {
            if (_pendingKind != NodeActivityKind.Thinking)
            {
                Flush();
                _pendingKind = NodeActivityKind.Thinking;
            }
            _pendingThinking.Append(text);
            MaybeFlush();
        }

        /// <summary>上报正文增量。</summary>
        /// <param name="text">正文文本增量。</param>
        public void Text(string text)
        {
            if (_pendingKind != NodeActivityKind.Text)
            {
                Flush();
                _pendingKind = NodeActivityKind.Text;
            }
            _pendingText.Append(text);
            MaybeFlush();
        }

        /// <summary>即时上报工具调用（先冲刷累积文本，保持时序）。</summary>
        /// <param name="toolName">工具名。</param>
        /// <param name="arguments">参数摘要。</param>
        public void ToolCall(string toolName, string? arguments)
        {
            Flush();
            Emit(NodeActivityItem.ToolCall(toolName, arguments, null));
        }

        /// <summary>即时上报工具结果（先冲刷累积文本，保持时序）。</summary>
        /// <param name="text">结果摘要（失败时为错误信息）。</param>
        public void ToolResult(string? text)
        {
            Flush();
            Emit(NodeActivityItem.ToolResult(text, null));
        }

        /// <summary>冲刷所有累积文本（节点结束时必须调用，否则尾部内容丢失）。</summary>
        public void Flush()
        {
            if (_pendingThinking.Length > 0)
            {
                Emit(NodeActivityItem.Thinking(_pendingThinking.ToString()));
                _pendingThinking.Clear();
            }
            if (_pendingText.Length > 0)
            {
                Emit(NodeActivityItem.Text(_pendingText.ToString()));
                _pendingText.Clear();
            }
            _sinceLastReport.Restart();
        }

        /// <summary>达到时间窗才冲刷，避免逐 token 上报。</summary>
        private void MaybeFlush()
        {
            if (_sinceLastReport.ElapsedMilliseconds >= ThrottleMs)
            {
                Flush();
            }
        }

        private void Emit(NodeActivityItem activity)
        {
            Report(_onProgress, _gate, new OrchestrationProgress
            {
                EventType = ProgressEventType.NodeActivity,
                NodeId = _nodeId,
                Activity = activity
            });
            _sinceLastReport.Restart();
        }
    }

    /// <summary>
    /// 关键前驱失败时，沿依赖图把该节点及其全部（传递）后继中仍处于
    /// <see cref="TaskNodeStatus.Pending"/> 的节点标记为 <see cref="TaskNodeStatus.Skipped"/>，
    /// 并逐节点上报 <see cref="ProgressEventType.NodeSkipped"/>，避免界面上这些节点凭空消失。
    /// 已在运行/已完成的节点不会被改动。
    /// </summary>
    /// <param name="dependents">节点 id → 其直接后继列表。</param>
    /// <param name="root">起始节点（关键前驱失败的直接后继）。</param>
    /// <param name="result">编排结果。</param>
    /// <param name="onProgress">进度回调，可为 null。</param>
    /// <param name="reportGate">进度上报串行化锁对象。</param>
    private static void SkipTransitiveSuccessors(
        Dictionary<string, List<TaskNode>> dependents, TaskNode root, OrchestrationResult result,
        Action<OrchestrationProgress>? onProgress = null, object? reportGate = null)
    {
        var stack = new Stack<TaskNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Status != TaskNodeStatus.Pending)
                continue;

            node.Status = TaskNodeStatus.Skipped;
            var nodeResult = ToNodeResult(node);
            result.Nodes.Add(nodeResult);

            Report(onProgress, reportGate, new OrchestrationProgress
            {
                EventType = ProgressEventType.NodeSkipped,
                NodeId = node.Id,
                Message = node.Description,
                NodeResult = nodeResult
            });

            if (dependents.TryGetValue(node.Id, out var children))
            {
                foreach (var child in children)
                    stack.Push(child);
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
