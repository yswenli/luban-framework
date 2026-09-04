/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： CancelRequest.cs
*版本号： V1.0.0.0
*唯一标识：a1e07afd-d4de-4d0f-8d01-6aed2fa9257d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：CancelRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CancelRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 取消请求，取消整个审批流程。
/// </summary>
public class CancelRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 取消原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
    /// <summary>
    /// 操作人角色列表。
    /// </summary>
    public List<string>? ActorRoles { get; set; }
}