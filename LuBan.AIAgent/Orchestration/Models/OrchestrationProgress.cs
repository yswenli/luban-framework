/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： OrchestrationProgress
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排进度事件
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 编排进度事件。
/// </summary>
public class OrchestrationProgress
{
    /// <summary>
    /// 获取或设置事件类型。
    /// </summary>
    public ProgressEventType EventType { get; set; }

    /// <summary>
    /// 获取或设置关联的节点标识。
    /// </summary>
    public string? NodeId { get; set; }

    /// <summary>
    /// 获取或设置事件消息。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 获取或设置关联的节点结果。
    /// </summary>
    public NodeResult? NodeResult { get; set; }
}
