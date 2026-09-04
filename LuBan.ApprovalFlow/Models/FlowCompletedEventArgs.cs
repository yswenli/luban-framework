/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowCompletedEventArgs.cs
*版本号： V1.0.0.0
*唯一标识：ee85b4bd-f97e-4495-9df1-279d6d9c6b96
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：FlowCompletedEventArgs 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FlowCompletedEventArgs 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程完成事件参数，当流程正常完成时触发。
/// </summary>
public class FlowCompletedEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 流程编码。
    /// </summary>
    public string? FlowKey { get; }
    /// <summary>
    /// 流程名称。
    /// </summary>
    public string FlowName { get; }
    /// <summary>
    /// 发起人用户ID。
    /// </summary>
    public long InitiatorUserId { get; }
    /// <summary>
    /// 发起人名称。
    /// </summary>
    public string InitiatorName { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; }
    /// <summary>
    /// 完成时间。
    /// </summary>
    public DateTime CompletedAt { get; }
    /// <summary>
    /// 流程总时长（秒）。
    /// </summary>
    public int? Duration { get; }
    /// <summary>
    /// 流程开始时间。
    /// </summary>
    public DateTime StartedAt { get; }

    /// <summary>
    /// 构造流程完成事件参数。
    /// </summary>
    public FlowCompletedEventArgs(long recordId, string? flowKey, string flowName, long initiatorUserId, string initiatorName, string? businessKey, Dictionary<string, object>? variables, DateTime completedAt, int? duration, DateTime startedAt)
    {
        RecordId = recordId;
        FlowKey = flowKey;
        FlowName = flowName;
        InitiatorUserId = initiatorUserId;
        InitiatorName = initiatorName;
        BusinessKey = businessKey;
        Variables = variables;
        CompletedAt = completedAt;
        Duration = duration;
        StartedAt = startedAt;
    }
}