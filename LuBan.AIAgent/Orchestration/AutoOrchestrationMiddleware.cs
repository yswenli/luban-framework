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
        var opts = _options.Value.Orchestration;
        if (opts == null || !opts.Enabled || !opts.AutoDetect)
            return false;

        if (opts.HeuristicFilter.ShouldSkipPlanning(input))
            return false;

        TaskGraph? graph;
        try
        {
            graph = await _planner.PlanAsync(input, cancellationToken);
        }
        catch
        {
            return false;
        }

        if (graph == null || graph.Nodes.Count <= 1)
            return false;

        if (!graph.Validate(out _))
            return false;

        _pendingGraph = graph;
        return true;
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