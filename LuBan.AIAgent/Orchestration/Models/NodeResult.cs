/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： NodeResult
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：单节点执行结果
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 单节点执行结果。
/// </summary>
public class NodeResult
{
    /// <summary>
    /// 获取或设置节点标识。
    /// </summary>
    public string NodeId { get; set; } = "";

    /// <summary>
    /// 获取或设置节点最终状态。
    /// </summary>
    public TaskNodeStatus Status { get; set; }

    /// <summary>
    /// 获取或设置节点输出内容。
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// 获取或设置错误信息。
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 获取或设置执行耗时。
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// 获取或设置 Token 消耗量。当前未实现，预留字段。
    /// </summary>
    public int TokensUsed { get; set; }
}
