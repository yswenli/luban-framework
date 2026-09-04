/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： TransferRequest.cs
*版本号： V1.0.0.0
*唯一标识：a478791e-2630-418e-b20f-a82606559095
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：TransferRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：TransferRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 转办请求，将当前任务转给其他人处理。
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    /// <summary>
    /// 目标用户ID。
    /// </summary>
    public long TargetUserId { get; set; }
    /// <summary>
    /// 目标用户名称。
    /// </summary>
    public string TargetUserName { get; set; } = string.Empty;
    /// <summary>
    /// 转办原因/意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
}