/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： ApprovalRequest.cs
*版本号： V1.0.0.0
*唯一标识：2e19badb-cc71-4ce1-a849-fc6fee313683
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ApprovalRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ApprovalRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批请求，包含审批操作所需的所有参数。
/// </summary>
public class ApprovalRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 审批动作：approve/reject/return等。
    /// </summary>
    public string Action { get; set; } = ConstActionType.Approve;
    /// <summary>
    /// 审批意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? Payload { get; set; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }
    /// <summary>
    /// 附件列表。
    /// </summary>
    public List<AttachmentInfo>? Attachments { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long ActorUserId { get; set; }
    /// <summary>
    /// 操作人角色列表。
    /// </summary>
    public List<string>? ActorRoles { get; set; }
    /// <summary>
    /// 是否系统自动操作。
    /// </summary>
    public bool IsSystemAction { get; set; } = false;
}

/// <summary>
/// 附件信息，描述审批时上传的附件详情。
/// </summary>
public class AttachmentInfo
{
    /// <summary>
    /// 附件名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 附件URL地址。
    /// </summary>
    public string? Url { get; set; }
    /// <summary>
    /// 附件大小（字节）。
    /// </summary>
    public long? Size { get; set; }
    /// <summary>
    /// 附件类型/MIME类型。
    /// </summary>
    public string? Type { get; set; }
}