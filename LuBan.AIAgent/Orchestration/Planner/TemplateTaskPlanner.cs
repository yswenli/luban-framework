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
namespace LuBan.AIAgent.Orchestration.Planner;

/// <summary>
/// 基于模板匹配的任务规划器，先尝试匹配预定义模板，未命中时返回 null。
/// </summary>
public class TemplateTaskPlanner : ITaskPlanner
{
    private readonly List<TaskGraphTemplate> _builtInTemplates;
    private List<TaskGraphTemplate> _workspaceTemplates = new();

    /// <summary>
    /// 创建 TemplateTaskPlanner 实例。
    /// </summary>
    /// <param name="templates">内置模板列表（构造时提供，不随工作区变化）。</param>
    public TemplateTaskPlanner(IEnumerable<TaskGraphTemplate> templates)
    {
        _builtInTemplates = templates?.ToList() ?? new();
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

    /// <inheritdoc/>
    public Task<ReflectionResult> ReflectAsync(ReplanContext context, CancellationToken ct = default)
    {
        return Task.FromResult(new ReflectionResult
        {
            Analysis = "模板规划器不支持反思重规划",
            ShouldRetry = false,
            FailedNodeIds = context.FailedNodes.Select(n => n.NodeId).ToList()
        });
    }

    /// <summary>
    /// 从工作区 `.luban-agent/plans/*.json` 加载任务模板。整体替换之前加载的工作区模板，
    /// 避免重复进入或切换工作区时累积残留。单个文件失败不影响其他文件。
    /// </summary>
    /// <param name="workspaceRoot">工作区根路径。</param>
    /// <returns>成功加载的模板数量。</returns>
    [RequiresUnreferencedCode("模板 JSON 反序列化依赖反射")]
    public int LoadFromWorkspace(string workspaceRoot)
    {
        var loaded = new List<TaskGraphTemplate>();
        if (!string.IsNullOrWhiteSpace(workspaceRoot))
        {
            var dir = Path.Combine(workspaceRoot, ".luban-agent", "plans");
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
                {
                    try
                    {
                        var template = TaskGraphTemplate.FromJson(File.ReadAllText(file));
                        if (template == null)
                        {
                            Logger.Warn($"任务模板文件无效（缺少 name 或 graph 无节点），已跳过: {file}");
                            continue;
                        }
                        loaded.Add(template);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"加载任务模板失败: {file}", ex);
                    }
                }
            }
        }

        _workspaceTemplates = loaded;

        if (loaded.Count > 0)
            Logger.Info($"已从工作区加载 {loaded.Count} 个任务模板 ({Path.Combine(workspaceRoot ?? "", ".luban-agent", "plans")})");
        return loaded.Count;
    }

    /// <summary>
    /// 通过关键词匹配模板。工作区模板优先于内置模板。
    /// </summary>
    /// <param name="task">用户任务描述。</param>
    /// <returns>匹配的模板，未命中返回 null。</returns>
    private TaskGraphTemplate? MatchTemplate(string task)
    {
        var lowerTask = task.ToLowerInvariant();
        
        foreach (var t in _workspaceTemplates)
        {
            foreach (var kw in t.Keywords)
            {
                if (lowerTask.Contains(kw.ToLowerInvariant()))
                    return t;
            }
        }
        
        foreach (var t in _builtInTemplates)
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
