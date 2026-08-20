/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： TaskGraph
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：任务图谱，表示一个有向无环图（DAG）的根结构
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 任务图谱，表示一个有向无环图（DAG）的根结构。
/// </summary>
public class TaskGraph
{
    /// <summary>
    /// 获取或设置图谱唯一标识。
    /// </summary>
    public string GraphId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 获取或设置原始用户任务描述。
    /// </summary>
    public string OriginalTask { get; set; } = "";

    /// <summary>
    /// 获取或设置所有节点集合。顺序不保证为拓扑序，调度时由 <see cref="GetTopologicalLayers"/> 排序。
    /// </summary>
    public List<TaskNode> Nodes { get; set; } = new();

    /// <summary>
    /// 获取或设置图谱来源（llm / template / manual）。
    /// </summary>
    public string Source { get; set; } = "llm";

    /// <summary>
    /// 获取或设置创建时间（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 校验 DAG 合法性，包括无环、依赖存在、无重复 ID 三项检查。
    /// </summary>
    /// <param name="errors">校验失败时输出的错误信息列表。</param>
    /// <returns>校验通过返回 true，否则返回 false。</returns>
    public bool Validate(out List<string> errors)
    {
        errors = new();
        if (Nodes.Count == 0)
        {
            errors.Add("图谱无节点");
            return false;
        }

        var idSet = new HashSet<string>();
        foreach (var n in Nodes)
        {
            if (!idSet.Add(n.Id))
                errors.Add($"节点 ID 重复: {n.Id}");
        }
        if (errors.Count > 0) return false;

        foreach (var n in Nodes)
        {
            foreach (var dep in n.Dependencies)
            {
                if (!idSet.Contains(dep))
                    errors.Add($"节点 {n.Id} 依赖不存在的节点 {dep}");
            }
        }
        if (errors.Count > 0) return false;

        if (HasCycle())
            errors.Add("图谱存在环");

        return errors.Count == 0;
    }

    /// <summary>
    /// 基于 Kahn 算法（BFS）对节点进行拓扑分层，同一层节点可并行执行。
    /// </summary>
    /// <returns>按层级排序的节点列表，有环时返回的节点总数小于 <see cref="Nodes"/> 数量。</returns>
    public List<List<TaskNode>> GetTopologicalLayers()
    {
        var indegree = Nodes.ToDictionary(n => n.Id, n => n.Dependencies.Count);
        var dependents = Nodes.ToDictionary(n => n.Id, n => new List<string>());
        foreach (var n in Nodes)
        {
            foreach (var dep in n.Dependencies)
                dependents[dep].Add(n.Id);
        }

        var layers = new List<List<TaskNode>>();
        var queue = Nodes.Where(n => indegree[n.Id] == 0).ToList();
        var nodeMap = Nodes.ToDictionary(n => n.Id);

        while (queue.Count > 0)
        {
            var layer = queue.ToList();
            layers.Add(layer);
            var next = new List<TaskNode>();
            foreach (var n in layer)
            {
                foreach (var childId in dependents[n.Id])
                {
                    if (--indegree[childId] == 0)
                        next.Add(nodeMap[childId]);
                }
            }
            queue = next;
        }
        return layers;
    }

    private bool HasCycle() => GetTopologicalLayers().SelectMany(l => l).Count() != Nodes.Count;
}
