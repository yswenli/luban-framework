/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： ReturnRequest.cs
*版本号： V1.0.0.0
*唯一标识：6dfdbc6d-1481-4951-8b53-e10dbef2cb0c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ReturnRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReturnRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 退回请求，将流程退回到指定节点。
/// </summary>
public class ReturnRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 退回目标节点ID，为空则退回到发起节点。
    /// </summary>
    public string? ReturnToNodeId { get; set; }
    /// <summary>
    /// 退回原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? Payload { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
}