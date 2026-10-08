/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tools.Orchestration
*文件名： OrchestrationToolGroup
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排工具组，将编排能力拆为「规划」与「执行」两个工具方法暴露给主 Agent，
*      由 LLM 自行判定是否需要编排（取代旧的自动前哨中间件）
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;

namespace LuBan.AIAgent.Tools.Orchestration;

/// <summary>
/// 编排工具组，将编排能力拆为「规划」与「执行」两个工具方法暴露给主 Agent。
/// 由 LLM 自行判定是否需要编排，判定过程即是工具调用，不再有隐式前哨。
/// </summary>
public class OrchestrationToolGroup
{
    /// <summary>plan_task 工具名（对 LLM 暴露的名称）。</summary>
    public const string PlanTaskToolName = "plan_task";

    /// <summary>run_orchestration 工具名（对 LLM 暴露的名称）。</summary>
    public const string RunOrchestrationToolName = "run_orchestration";

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建 OrchestrationToolGroup 实例。
    /// </summary>
    /// <param name="serviceProvider">服务提供者。</param>
    public OrchestrationToolGroup(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 把复合任务拆解为 DAG 任务图谱并登记，返回 graphId 与节点摘要。
    /// </summary>
    /// <param name="task">复合任务描述。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>规划摘要 JSON（命中编排或未命中均为成功工具结果）。</returns>
    [Description("把复合任务拆解为 DAG 任务图谱并登记，返回 graphId 与节点摘要。" +
                 "适用于多步骤、可并行、需要多种工具组合的任务；" +
                 "单一问答、单一工具调用等简单任务不要调用本工具。")]
    public async Task<ToolResult<string>> PlanTaskAsync(
        [Description("复合任务描述")] string task,
        CancellationToken cancellationToken = default)
    {
        var confirmation = _serviceProvider.GetRequiredService<IToolConfirmationService>();
        var outcome = await confirmation.EvaluateAsync(PlanTaskToolName, null,
            new Dictionary<string, object?> { ["task"] = task });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        var planner = _serviceProvider.GetRequiredService<ITaskPlanner>();
        var store = _serviceProvider.GetRequiredService<GraphPlanStore>();
        var sink = _serviceProvider.GetRequiredService<IOrchestrationProgressSink>();
        var orchestrationOpts = _serviceProvider.GetRequiredService<IOptions<LuBanAgentOptions>>().Value.Orchestration ?? new();

        var sw = Stopwatch.StartNew();
        try
        {
            var graph = await planner.PlanAsync(task, cancellationToken);
            sw.Stop();
            var planMs = sw.ElapsedMilliseconds;

            if (graph == null || graph.Nodes.Count == 0)
                return Decline("single-node", "规划器未返回可用节点", planMs, sink);

            if (graph.Nodes.Count <= 1)
                return Decline("single-node", "单节点任务无需编排，请直接处理", planMs, sink);

            if (!graph.Validate(out var errors))
                return Decline("invalid-dag", string.Join("; ", errors), planMs, sink);

            var graphId = store.Save(graph);
            var parallelism = orchestrationOpts.MaxParallelism > 0
                ? orchestrationOpts.MaxParallelism
                : graph.Nodes.Count;

            Logger.Info($"[OrchDiag] plan_task accepted graphId={graphId} nodes={graph.Nodes.Count} parallelism={parallelism} planMs={planMs}");
            sink.Publish(new OrchestrationProgress
            {
                EventType = ProgressEventType.PlanningCompleted,
                Message = $"已生成 {graph.Nodes.Count} 个节点的任务图谱",
                ElapsedMs = planMs
            });

            return ToolResult.Ok(BuildPlanJson(graphId, graph, parallelism, planMs));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();

            // 工具内异常会被 FunctionInvokingChatClient 捕获并转成结果文本，无法到达 TUI 错误分类链，
            // 故此处自行分类：API 级错误记 Error 并返回 Fail（含分类），其余降级为「不编排」成功结果。
            var info = ApiErrorClassifier.Classify(ex, cancellationToken.IsCancellationRequested);
            if (info.Category is not AgentApiErrorCategory.Unknown and not AgentApiErrorCategory.Canceled)
            {
                Logger.Error("编排规划失败（API 错误）", ex, info.Category.ToString());
                return ToolResult.Fail<string>($"规划失败（{info.Category}）：{info.FriendlyMessage}");
            }

            Logger.Error("编排规划失败", ex);
            return Decline("planning-failed", ex.Message, sw.ElapsedMilliseconds, sink);
        }
    }

    /// <summary>
    /// 执行 plan_task 已登记的任务图谱，调度子代理执行并返回最终结果。
    /// </summary>
    /// <param name="graphId">plan_task 返回的图谱标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>编排结果文本。</returns>
    [Description("执行 plan_task 已登记的任务图谱（graphId 来自 plan_task 返回值），" +
                 "调度子代理执行并返回最终结果。必须先调用 plan_task。")]
    public async Task<ToolResult<string>> RunOrchestrationAsync(
        [Description("plan_task 返回的图谱标识")] string graphId,
        CancellationToken cancellationToken = default)
    {
        var confirmation = _serviceProvider.GetRequiredService<IToolConfirmationService>();
        var outcome = await confirmation.EvaluateAsync(RunOrchestrationToolName, null,
            new Dictionary<string, object?> { ["graphId"] = graphId });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        var store = _serviceProvider.GetRequiredService<GraphPlanStore>();
        var graph = store.Get(graphId);
        if (graph == null)
            return ToolResult.Fail<string>("图谱不存在或已过期，请重新调用 plan_task");

        var orchestrator = _serviceProvider.GetRequiredService<IOrchestrator>();
        var sink = _serviceProvider.GetRequiredService<IOrchestrationProgressSink>();

        try
        {
            var sw = Stopwatch.StartNew();
            var result = await orchestrator.RunAsync(graph, sink.Publish, cancellationToken);
            sw.Stop();

            // 成功后显式移除；失败保留以便重试
            store.Remove(graphId);
            return ToolResult.Ok(FormatResult(result, sw.Elapsed));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error("编排执行失败", ex);
            return ToolResult.Fail<string>($"编排失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 构造「不编排」的成功工具结果，并上报规划完成进度（便于 UI 收尾规划动画）。
    /// </summary>
    private static ToolResult<string> Decline(string reason, string detail, long planMs, IOrchestrationProgressSink sink)
    {
        Logger.Info($"[OrchDiag] plan_task declined reason={reason} planMs={planMs} detail={detail}");
        sink.Publish(new OrchestrationProgress
        {
            EventType = ProgressEventType.PlanningCompleted,
            Message = "未命中编排，转为常规对话",
            ElapsedMs = planMs
        });
        return ToolResult.Ok(BuildDeclineJson(reason, detail));
    }

    /// <summary>
    /// 命中编排时的规划摘要 JSON。
    /// </summary>
    private static string BuildPlanJson(string graphId, TaskGraph graph, int parallelism, long planMs)
    {
        using var ms = new MemoryStream();
        using (var w = new Utf8JsonWriter(ms))
        {
            w.WriteStartObject();
            w.WriteBoolean("orchestrate", true);
            w.WriteString("graphId", graphId);
            w.WriteNumber("nodeCount", graph.Nodes.Count);
            w.WriteNumber("parallelism", parallelism);
            w.WriteNumber("planMs", planMs);
            w.WriteStartArray("nodes");
            foreach (var n in graph.Nodes)
            {
                w.WriteStartObject();
                w.WriteString("id", n.Id);
                if (!string.IsNullOrEmpty(n.Role))
                    w.WriteString("role", n.Role);
                w.WriteString("description", n.Description);
                w.WriteStartArray("dependencies");
                foreach (var dep in n.Dependencies)
                    w.WriteStringValue(dep);
                w.WriteEndArray();
                if (n.ToolGroups != null)
                {
                    w.WriteStartArray("toolGroups");
                    foreach (var tg in n.ToolGroups)
                        w.WriteStringValue(tg);
                    w.WriteEndArray();
                }
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// 未命中编排时的 JSON（仍为成功工具结果，供主 Agent 继续对话）。
    /// </summary>
    private static string BuildDeclineJson(string reason, string detail)
    {
        using var ms = new MemoryStream();
        using (var w = new Utf8JsonWriter(ms))
        {
            w.WriteStartObject();
            w.WriteBoolean("orchestrate", false);
            w.WriteString("reason", reason);
            w.WriteString("detail", detail);
            w.WriteNull("graphId");
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// 格式化编排结果为字符串，供主 Agent 读取。
    /// </summary>
    /// <param name="result">编排结果。</param>
    /// <param name="elapsed">编排总耗时。</param>
    /// <returns>格式化后的字符串。</returns>
    private static string FormatResult(OrchestrationResult result, TimeSpan elapsed)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"编排状态: {result.OverallStatus}");
        sb.AppendLine($"节点执行: {result.Nodes.Count(n => n.Status == TaskNodeStatus.Succeeded)}/{result.Nodes.Count} 成功");
        sb.AppendLine($"总耗时: {elapsed.TotalSeconds:F1}s");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(result.FinalOutput))
        {
            sb.AppendLine("最终结果:");
            sb.AppendLine(result.FinalOutput);
        }

        var failed = result.Nodes.Where(n => n.Status == TaskNodeStatus.Failed).ToList();
        if (failed.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("失败节点:");
            foreach (var f in failed)
                sb.AppendLine($"  - {f.NodeId}: {f.Error}");
        }

        return sb.ToString();
    }
}