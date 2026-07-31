/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： SubAgentSpec
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：SubAgent 创建规格，由 TaskNode 映射而来
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// SubAgent 创建规格，由 TaskNode 映射而来。
/// </summary>
public class SubAgentSpec
{
    /// <summary>
    /// 获取或设置节点标识。
    /// </summary>
    public string NodeId { get; set; } = "";

    /// <summary>
    /// 获取或设置执行 prompt（已注入前驱输出）。
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// 获取或设置启用的工具组。null 表示全部启用。
    /// </summary>
    public List<string>? ToolGroups { get; set; }

    /// <summary>
    /// 获取或设置使用的模型。
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// 获取或设置编排会话 ID（用于关联）。
    /// </summary>
    public string ParentSessionId { get; set; } = "";

    /// <summary>
    /// 获取或设置运行时填充的 SessionId。
    /// </summary>
    public string? SessionId { get; set; }
}
