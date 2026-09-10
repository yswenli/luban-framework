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
        if (opts == null || !opts.AutoDetect)
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

        return graph.Validate(out _);
    }

    public async Task<OrchestrationResult> RunAsync(string input, CancellationToken cancellationToken = default)
    {
        return await _orchestrator.RunAsync(input, cancellationToken);
    }
}