/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： TaskNode
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：任务节点，表示 DAG 中的一个执行单元
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 任务节点，表示 DAG 中的一个执行单元。
/// </summary>
public class TaskNode
{
    /// <summary>
    /// 获取或设置节点标识（图谱内唯一，如 "research"、"analyze"）。
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// 获取或设置节点描述，供 LLM 和用户理解节点用途。
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 获取或设置节点角色（如 "analyst", "coder"）。null 表示使用通用 SubAgent。
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// 获取或设置节点执行 prompt。支持 {dep:xxx} 占位符，运行时由 ContextStore 替换为前驱节点输出。
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// 获取或设置依赖的节点 ID 列表。
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// 获取或设置该节点 SubAgent 启用的工具组。
    /// 当 Role 非空时，null 表示使用角色的 DefaultToolGroups；
    /// 当 Role 为空时，null 不再允许（planner 必须显式指定）。
    /// 空数组表示无工具。
    /// </summary>
    public List<string>? ToolGroups { get; set; }

    /// <summary>
    /// 获取或设置该节点使用的模型（格式 "provider:model"）。null 表示继承主 Agent 模型。
    /// 经 IProviderRouter 路由，Provider 不存在时回退默认模型。
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 获取或设置节点超时时间（秒）。null 表示使用默认值。
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// 获取或设置是否为关键节点。关键节点失败时阻止后继节点执行，默认为 false。
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// 获取或设置运行时状态。
    /// </summary>
    public TaskNodeStatus Status { get; set; } = TaskNodeStatus.Pending;

    /// <summary>
    /// 获取或设置运行时节点输出。
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// 获取或设置运行时错误信息。
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 获取或设置运行时开始时间（UTC）。
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 获取或设置运行时结束时间（UTC）。
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// 获取或设置运行时 SubAgent 的 SessionId。
    /// </summary>
    public string? SessionId { get; set; }
}
