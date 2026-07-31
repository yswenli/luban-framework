/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Planner
*文件名： CompositeTaskPlanner
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：组合式任务规划器，模板优先匹配，未命中时回退到 LLM 动态规划
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration.Planner;

/// <summary>
/// 组合式任务规划器，模板优先匹配，未命中时回退到 LLM 动态规划。
/// </summary>
public class CompositeTaskPlanner : ITaskPlanner
{
    private readonly TemplateTaskPlanner _templatePlanner;
    private readonly LlmTaskPlanner _llmPlanner;

    /// <summary>
    /// 创建 CompositeTaskPlanner 实例。
    /// </summary>
    /// <param name="templatePlanner">模板规划器。</param>
    /// <param name="llmPlanner">LLM 规划器。</param>
    public CompositeTaskPlanner(TemplateTaskPlanner templatePlanner, LlmTaskPlanner llmPlanner)
    {
        _templatePlanner = templatePlanner;
        _llmPlanner = llmPlanner;
    }

    /// <inheritdoc/>
    public async Task<TaskGraph?> PlanAsync(string task, CancellationToken ct = default)
    {
        try
        {
            var graph = await _templatePlanner.PlanAsync(task, ct);
            if (graph != null) return graph;
        }
        catch (Exception ex)
        {
            Logger.Warn("模板匹配失败，回退到 LLM 规划", ex);
        }

        return await _llmPlanner.PlanAsync(task, ct);
    }
}
