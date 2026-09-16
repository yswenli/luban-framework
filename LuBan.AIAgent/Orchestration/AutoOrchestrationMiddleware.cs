using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Orchestration;

public class AutoOrchestrationMiddleware
{
    private readonly IOrchestrator _orchestrator;
    private readonly ITaskPlanner _planner;
    private readonly IOptions<LuBanAgentOptions> _options;
    private TaskGraph? _pendingGraph;

    public AutoOrchestrationMiddleware(
        IOrchestrator orchestrator,
        ITaskPlanner planner,
        IOptions<LuBanAgentOptions> options)
    {
        _orchestrator = orchestrator;
        _planner = planner;
        _options = options;
    }

    public async Task<bool> ShouldOrchestrateAsync(string input, CancellationToken cancellationToken = default)
    {
        if (!ShouldAttemptPlanning(input))
            return false;

        return await TryPlanAsync(input, cancellationToken) != null;
    }

    /// <summary>
    /// 廉价前置判定：编排是否可能命中（已启用 + 自动检测 + 启发式过滤），不做 LLM 规划。
    /// 供上层在真正规划（耗时数十秒）之前先给出"正在规划"的实时提示。
    /// </summary>
    /// <param name="input">用户输入。</param>
    /// <returns>是否值得进入规划阶段。</returns>
    public bool ShouldAttemptPlanning(string input)
    {
        var opts = _options.Value.Orchestration;
        if (opts == null || !opts.Enabled || !opts.AutoDetect)
        {
            Logger.Debug($"[OrchDiag] skip: enabled={opts?.Enabled} autoDetect={opts?.AutoDetect}");
            return false;
        }

        if (opts.HeuristicFilter.ShouldSkipPlanning(input))
        {
            Logger.Debug($"[OrchDiag] skip: heuristic filter (len={input.Trim().Length} range=[{opts.HeuristicFilter.MinLength},{opts.HeuristicFilter.MaxLength}] requireKeyword={opts.HeuristicFilter.RequireKeyword})");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 执行规划并缓存图谱；未命中编排（节点数 ≤ 1 或校验失败）时返回 null，调用方走常规对话。
    /// </summary>
    /// <param name="input">用户输入。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>命中的任务图谱，未命中返回 null。</returns>
    public async Task<TaskGraph?> TryPlanAsync(string input, CancellationToken cancellationToken = default)
    {
        TaskGraph? graph;
        try
        {
            graph = await _planner.PlanAsync(input, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Debug($"[OrchDiag] skip: planner threw {ex.GetType().Name}: {ex.Message}");
            return null;
        }

        if (graph == null || graph.Nodes.Count <= 1)
        {
            Logger.Debug($"[OrchDiag] skip: plan nodes={graph?.Nodes.Count ?? -1}");
            return null;
        }

        if (!graph.Validate(out var validateErrors))
        {
            Logger.Debug($"[OrchDiag] skip: validate failed {string.Join("; ", validateErrors)}");
            return null;
        }

        Logger.Debug($"[OrchDiag] orchestrate accepted: source={graph.Source} nodes={graph.Nodes.Count} | " +
            string.Join(" | ", graph.Nodes.Select(n =>
                $"{n.Id}(role={(n.Role ?? "null")},tools={(n.ToolGroups == null ? "null" : $"[{string.Join(",", n.ToolGroups)}]")},deps=[{string.Join(",", n.Dependencies)}],critical={n.IsCritical})")));

        _pendingGraph = graph;
        return graph;
    }

    /// <summary>
    /// 规划 + 执行编排，并在执行过程中实时上报进度；未命中编排时返回 null（调用方走常规对话）。
    /// </summary>
    /// <param name="input">用户输入。</param>
    /// <param name="onProgress">进度回调。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>编排结果；未命中编排返回 null。</returns>
    public async Task<OrchestrationResult?> TryRunOrchestrationAsync(
        string input,
        Action<OrchestrationProgress>? onProgress,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldAttemptPlanning(input))
            return null;

        var graph = await TryPlanAsync(input, cancellationToken);
        _pendingGraph = null;

        if (graph == null)
        {
            onProgress?.Invoke(new OrchestrationProgress
            {
                EventType = ProgressEventType.PlanningCompleted,
                Message = "未命中编排，转为常规对话"
            });
            return null;
        }

        onProgress?.Invoke(new OrchestrationProgress
        {
            EventType = ProgressEventType.PlanningCompleted,
            Message = $"已生成 {graph.Nodes.Count} 个节点的任务图谱"
        });

        return await _orchestrator.RunAsync(graph, onProgress, cancellationToken);
    }

    public async Task<OrchestrationResult> RunAsync(string input, CancellationToken cancellationToken = default)
    {
        // 复用 ShouldOrchestrateAsync 已规划完成的 DAG，避免同一任务二次 LLM 规划
        if (_pendingGraph != null)
        {
            var graph = _pendingGraph;
            _pendingGraph = null;
            return await _orchestrator.RunAsync(graph, cancellationToken);
        }

        return await _orchestrator.RunAsync(input, cancellationToken);
    }
}