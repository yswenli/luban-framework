/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Planner
*文件名： TemplateTaskPlanner
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：基于模板匹配的任务规划器，先尝试匹配预定义模板，未命中时返回 null
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration.Planner;

/// <summary>
/// 基于模板匹配的任务规划器，先尝试匹配预定义模板，未命中时返回 null。
/// </summary>
public class TemplateTaskPlanner : ITaskPlanner
{
    private readonly List<TaskGraphTemplate> _templates;

    /// <summary>
    /// 创建 TemplateTaskPlanner 实例。
    /// </summary>
    /// <param name="templates">模板列表。</param>
    public TemplateTaskPlanner(IEnumerable<TaskGraphTemplate> templates)
    {
        _templates = templates?.ToList() ?? new();
    }

    /// <inheritdoc/>
    public Task<TaskGraph?> PlanAsync(string task, CancellationToken ct = default)
    {
        var matched = MatchTemplate(task);
        if (matched == null)
            return Task.FromResult<TaskGraph?>(null);

        var parameters = ExtractParameters(task, matched);
        var graph = matched.Instantiate(parameters);
        graph.OriginalTask = task;
        return Task.FromResult<TaskGraph?>(graph);
    }

    /// <summary>
    /// 通过关键词匹配模板。
    /// </summary>
    /// <param name="task">用户任务描述。</param>
    /// <returns>匹配的模板，未命中返回 null。</returns>
    private TaskGraphTemplate? MatchTemplate(string task)
    {
        var lowerTask = task.ToLowerInvariant();
        foreach (var t in _templates)
        {
            foreach (var kw in t.Keywords)
            {
                if (lowerTask.Contains(kw.ToLowerInvariant()))
                    return t;
            }
        }
        return null;
    }

    /// <summary>
    /// 从任务描述中提取参数。当前为简单实现，返回空字典。
    /// 未来可扩展为 LLM 提取。
    /// </summary>
    /// <param name="task">用户任务描述。</param>
    /// <param name="template">匹配的模板。</param>
    /// <returns>参数字典。</returns>
    private static Dictionary<string, string> ExtractParameters(string task, TaskGraphTemplate template)
    {
        var parameters = new Dictionary<string, string>();
        foreach (var p in template.Parameters)
        {
            if (!p.Required)
                continue;
            parameters[p.Name] = "";
        }
        return parameters;
    }
}
