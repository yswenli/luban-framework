/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： ContextStore
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：跨节点上下文存储，按图谱 ID 隔离，线程安全
*
*****************************************************************************/
using System.Collections.Concurrent;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 跨节点上下文存储，按图谱 ID 隔离，线程安全。
/// </summary>
public class ContextStore
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _outputs = new();

    /// <summary>
    /// 存储节点输出。
    /// </summary>
    /// <param name="graphId">图谱标识。</param>
    /// <param name="nodeId">节点标识。</param>
    /// <param name="output">节点输出内容。</param>
    public void SetOutput(string graphId, string nodeId, string output)
    {
        var dict = _outputs.GetOrAdd(graphId, _ => new());
        lock (dict) dict[nodeId] = output;
    }

    /// <summary>
    /// 获取节点输出。
    /// </summary>
    /// <param name="graphId">图谱标识。</param>
    /// <param name="nodeId">节点标识。</param>
    /// <returns>节点输出内容，不存在时返回 null。</returns>
    public string? GetOutput(string graphId, string nodeId)
    {
        if (_outputs.TryGetValue(graphId, out var dict))
            lock (dict) return dict.GetValueOrDefault(nodeId);
        return null;
    }

    /// <summary>
    /// 解析 prompt 中的 {dep:xxx} 占位符，替换为前驱节点输出。
    /// </summary>
    /// <param name="prompt">原始 prompt。</param>
    /// <param name="graph">任务图谱。</param>
    /// <param name="node">当前节点。</param>
    /// <returns>替换后的 prompt。</returns>
    public string ResolvePlaceholders(string prompt, TaskGraph graph, TaskNode node)
    {
        var resolved = prompt;
        foreach (var depId in node.Dependencies)
        {
            var depNode = graph.Nodes.FirstOrDefault(n => n.Id == depId);
            var depOutput = GetOutput(graph.GraphId, depId);

            if (depNode?.Status == TaskNodeStatus.Failed)
                resolved = resolved.Replace($"{{dep:{depId}}}",
                    $"[前驱节点 {depId} 执行失败: {depNode.Error}]");
            else if (depOutput == null)
                resolved = resolved.Replace($"{{dep:{depId}}}", "[前驱节点无输出]");
            else
                resolved = resolved.Replace($"{{dep:{depId}}}", depOutput);
        }
        return resolved;
    }

    /// <summary>
    /// 清理指定图谱的上下文。
    /// </summary>
    /// <param name="graphId">图谱标识。</param>
    public void Clear(string graphId) => _outputs.TryRemove(graphId, out _);
}
