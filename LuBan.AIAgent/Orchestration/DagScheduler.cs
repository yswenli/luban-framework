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
    public Task<OrchestrationResult> ExecuteAsync(TaskGraph graph, CancellationToken ct = default)
        => ExecuteAsync(graph, ct, null);

    /// <summary>
    /// 执行任务图谱，返回编排结果，并在执行过程中实时上报节点级进度事件。
    /// 回调由同层并行节点共同触发，内部已串行化，调用方无需额外加锁。
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

        using var semaphore = CreateParallelismSemaphore();
        var reportGate = new object();
        var layers = graph.GetTopologicalLayers();
        for (int layerIdx = 0; layerIdx < layers.Count; layerIdx++)
        {
            var layer = layers[layerIdx];
            var tasks = layer.Select(n => ExecuteNodeAsync(graph, n, result, semaphore, ct, onProgress, reportGate)).ToList();
            await Task.WhenAll(tasks);

            if (layer.Any(n => n.Status == TaskNodeStatus.Failed && n.IsCritical))
            {
                MarkRemainingAsSkipped(layers, layerIdx, result, onProgress, reportGate);
                break;
            }
        }

        sw.Stop();
        result.TotalElapsed = sw.Elapsed;
        result.OverallStatus = DetermineOverallStatus(graph, result);
        return result;
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
        SemaphoreSlim? semaphore, CancellationToken ct,
        Action<OrchestrationProgress>? onProgress = null,
        object? reportGate = null)
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
            Report(onProgress, reportGate, new OrchestrationProgress
            {
                EventType = ProgressEventType.NodeStarted,
                NodeId = node.Id,
                Message = node.Description
            });

            var spec = new SubAgentSpec
            {
                NodeId = node.Id,
                Prompt = resolvedPrompt,
                Role = node.Role,
                ToolGroups = node.ToolGroups,
                ModelName = node.ModelName,
                ParentSessionId = graph.GraphId,
                WorkspaceRoot = _options.Value.WorkspaceRoot
            };

            Logger.Debug($"[OrchDiag] node exec: id={node.Id} node.role={(node.Role ?? "null")} spec.role={(spec.Role ?? "null")} node.tools={(node.ToolGroups == null ? "null" : $"[{string.Join(",", node.ToolGroups)}]")} promptLen={resolvedPrompt.Length}");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var orchestrationOpts = _options.Value.Orchestration ?? new();
            if (node.TimeoutSeconds.HasValue)
                cts.CancelAfter(TimeSpan.FromSeconds(node.TimeoutSeconds.Value));
            else if (orchestrationOpts.DefaultNodeTimeoutSeconds > 0)
                cts.CancelAfter(TimeSpan.FromSeconds(orchestrationOpts.DefaultNodeTimeoutSeconds));

            var swCreate = Stopwatch.StartNew();
            var agent = await _subAgentFactory.CreateAsync(spec, ct);
            swCreate.Stop();

            var swRun = Stopwatch.StartNew();
            // 改为流式执行：把子 Agent 的思考/正文/工具调用/工具结果按时间轴上报，
            // 使上层 UI 能像主对话一样实时看到子代理在做什么（节点执行期最长可达超时阈值）。
            var reporter = new NodeActivityReporter(node.Id, onProgress, reportGate);
            var output = new StringBuilder();
            await foreach (var update in agent.RunStreamingAsync(resolvedPrompt, cts.Token))
            {
                if (update.Contents is null) continue;

                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        case TextReasoningContent reasoning when !string.IsNullOrEmpty(reasoning.Text):
                            reporter.Thinking(reasoning.Text);
                            break;

                        case FunctionCallContent functionCall:
                            reporter.ToolCall(functionCall.Name, SummarizeArguments(functionCall.Arguments), functionCall.CallId);
                            break;

                        case FunctionResultContent functionResult:
                            reporter.ToolResult(functionResult, functionResult.CallId);
                            break;

                        case TextContent text when !string.IsNullOrEmpty(text.Text):
                            output.Append(text.Text);
                            reporter.Text(text.Text);
                            break;
                    }
                }
            }
            reporter.Flush();
            swRun.Stop();

            node.Output = output.ToString();
            node.Status = TaskNodeStatus.Succeeded;
            node.SessionId = spec.SessionId;

            Logger.Debug($"[OrchDiag] node timing: id={node.Id} create={swCreate.Elapsed.TotalSeconds:F1}s run={swRun.Elapsed.TotalSeconds:F1}s");

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

            Logger.Debug($"[OrchDiag] node done: id={node.Id} status={node.Status} elapsed={nodeResult.Elapsed.TotalSeconds:F1}s outLen={(node.Output ?? "").Length} error={node.Error ?? "-"}");

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
                NodeResult = nodeResult
            });
        }
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
    /// 把工具调用参数压成单行摘要（过长截断），避免参数 JSON 撑爆 UI 行宽。
    /// </summary>
    /// <param name="arguments">工具参数集合。</param>
    /// <returns>参数摘要文本；无参数时返回 null。</returns>
    private static string? SummarizeArguments(IDictionary<string, object?>? arguments)
    {
        if (arguments is null || arguments.Count == 0) return null;

        var sb = new StringBuilder();
        foreach (var kv in arguments)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(kv.Key).Append('=').Append(FormatArgumentValue(kv.Value));
            if (sb.Length > 200)
            {
                sb.Length = 200;
                sb.Append('…');
                break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 格式化单个参数值：字符串去换行，复杂对象转 JSON。
    /// </summary>
    private static string FormatArgumentValue(object? value) => value switch
    {
        null => "null",
        string s => s.Replace("\r", " ").Replace("\n", " "),
        _ => value.ToString() ?? "null"
    };

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
        /// <param name="callId">调用标识。</param>
        public void ToolCall(string toolName, string? arguments, string? callId)
        {
            Flush();
            Emit(NodeActivityItem.ToolCall(toolName, arguments, callId));
        }

        /// <summary>即时上报工具结果（先冲刷累积文本，保持时序）。</summary>
        /// <param name="result">工具结果内容。</param>
        /// <param name="callId">调用标识。</param>
        public void ToolResult(FunctionResultContent result, string? callId)
        {
            Flush();

            var text = result.Exception is not null
                ? $"❌ {result.Exception.Message}"
                : SummarizeResult(result.Result);
            Emit(NodeActivityItem.ToolResult(text, callId));
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

        /// <summary>
        /// 把工具返回内容压成单行摘要（过长截断），避免大段文件内容灌进 UI。
        /// </summary>
        private static string? SummarizeResult(object? result)
        {
            if (result is null) return null;

            var text = result switch
            {
                string s => s,
                _ => result.ToString()
            };
            if (string.IsNullOrWhiteSpace(text)) return null;

            text = text.Replace("\r", " ").Replace("\n", " ").Trim();
            return text.Length > 200 ? text[..200] + "…" : text;
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
    /// 标记后续层的所有节点为 <see cref="TaskNodeStatus.Skipped"/>，并逐节点上报
    /// <see cref="ProgressEventType.NodeSkipped"/>，避免界面上这些节点凭空消失。
    /// </summary>
    /// <param name="layers">拓扑分层列表。</param>
    /// <param name="currentIndex">当前层索引。</param>
    /// <param name="result">编排结果。</param>
    /// <param name="onProgress">进度回调，可为 null。</param>
    /// <param name="reportGate">进度上报串行化锁对象。</param>
    private static void MarkRemainingAsSkipped(
        List<List<TaskNode>> layers, int currentIndex, OrchestrationResult result,
        Action<OrchestrationProgress>? onProgress = null, object? reportGate = null)
    {
        for (int i = currentIndex + 1; i < layers.Count; i++)
        {
            foreach (var node in layers[i])
            {
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
