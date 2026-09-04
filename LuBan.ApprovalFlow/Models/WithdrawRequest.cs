/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： WithdrawRequest.cs
*版本号： V1.0.0.0
*唯一标识：3558acbf-7bfc-4e8c-be45-370fb2b9c4bf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：WithdrawRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：WithdrawRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 撤回请求，发起人撤回已提交的流程。
/// </summary>
public class WithdrawRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 撤回原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作人用户ID（通常为发起人）。
    /// </summary>
    public long ActorUserId { get; set; }
}