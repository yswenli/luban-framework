/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowCancelledEventArgs.cs
*版本号： V1.0.0.0
*唯一标识：9b9fb692-3e91-4d61-bf8c-c73b8f8e5c9e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：FlowCancelledEventArgs 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FlowCancelledEventArgs 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程取消事件参数，当流程被取消时触发。
/// </summary>
public class FlowCancelledEventArgs : EventArgs
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
    /// 取消原因/意见。
    /// </summary>
    public string? CancelReason { get; }
    /// <summary>
    /// 取消操作人用户ID。
    /// </summary>
    public long? CancelledByUserId { get; }
    /// <summary>
    /// 取消操作人名称。
    /// </summary>
    public string? CancelledByName { get; }
    /// <summary>
    /// 取消时间。
    /// </summary>
    public DateTime CancelledAt { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }

    /// <summary>
    /// 构造流程取消事件参数。
    /// </summary>
    public FlowCancelledEventArgs(long recordId, string? flowKey, string flowName, string? cancelReason, long? cancelledByUserId, string? cancelledByName, DateTime cancelledAt, string? businessKey)
    {
        RecordId = recordId;
        FlowKey = flowKey;
        FlowName = flowName;
        CancelReason = cancelReason;
        CancelledByUserId = cancelledByUserId;
        CancelledByName = cancelledByName;
        CancelledAt = cancelledAt;
        BusinessKey = businessKey;
    }
}