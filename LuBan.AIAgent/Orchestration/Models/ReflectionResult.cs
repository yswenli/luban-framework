/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： ReflectionResult
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：反思结果、反思上下文、失败节点信息，用于 DAG 动态重规划
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 反思结果，包含 LLM 对失败节点的分析和修正建议。
/// </summary>
public class ReflectionResult
{
    /// <summary>
    /// 获取或设置被分析的失败节点 ID 列表。
    /// </summary>
    public IReadOnlyList<string> FailedNodeIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 获取或设置 LLM 对失败原因的分析。
    /// </summary>
    public string Analysis { get; set; } = "";

    /// <summary>
    /// 获取或设置 LLM 建议的修复方案。
    /// </summary>
    public string FixApproach { get; set; } = "";

    /// <summary>
    /// 获取或设置是否建议重试。false 时直接返回原始失败结果。
    /// </summary>
    public bool ShouldRetry { get; set; }

    /// <summary>
    /// 获取或设置修正节点列表（由 LLM 生成，用于修复失败分支）。
    /// </summary>
    public IReadOnlyList<TaskNode> NewNodes { get; set; } = Array.Empty<TaskNode>();
}

/// <summary>
/// 重规划上下文，传递给反思阶段的输入数据。
/// </summary>
public class ReplanContext
{
    /// <summary>
    /// 获取或设置原始用户任务。
    /// </summary>
    public string UserGoal { get; set; } = "";

    /// <summary>
    /// 获取或设置所有失败关键节点的信息。
    /// </summary>
    public IReadOnlyList<FailedNodeInfo> FailedNodes { get; set; } = Array.Empty<FailedNodeInfo>();

    /// <summary>
    /// 获取或设置原始任务图谱。
    /// </summary>
    public TaskGraph OriginalGraph { get; set; } = new();

    /// <summary>
    /// 获取或设置当前重规划尝试次数（从 1 开始）。
    /// </summary>
    public int Attempt { get; set; } = 1;
}

/// <summary>
/// 失败节点信息，包含节点输出及其直接依赖节点的输出。
/// </summary>
public class FailedNodeInfo
{
    /// <summary>
    /// 获取或设置节点标识。
    /// </summary>
    public string NodeId { get; set; } = "";

    /// <summary>
    /// 获取或设置节点描述。
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 获取或设置节点工具组。
    /// </summary>
    public List<string>? ToolGroups { get; set; }

    /// <summary>
    /// 获取或设置节点错误信息。
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 获取或设置节点输出。
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// 获取或设置直接依赖节点的输出字典（key 为依赖节点 ID，value 为输出内容）。
    /// </summary>
    public IReadOnlyDictionary<string, string> DependencyOutputs { get; set; }
        = new Dictionary<string, string>();
}
