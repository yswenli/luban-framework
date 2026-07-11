/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： AdvanceRequest
*版本号： V1.0.0.0
*唯一标识：6b62763c-a55a-4987-ab70-dfd421721d4d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:49:58
*描述：流程推进参数
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:49:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：流程推进参数
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程推进参数
/// </summary>
public class AdvanceRequest
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 推进行为：approve/reject等。
    /// </summary>
    public string Action { get; set; } = "approve"; // approve/reject
    /// <summary>
    /// 审批意见或备注（可选）。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 操作者用户ID（可选）。
    /// </summary>
    public long? ActorUserId { get; set; }
    /// <summary>
    /// 操作者角色集合（可选，用于与 user-node 的要求角色匹配）。
    /// </summary>
    public List<string>? ActorRoles { get; set; }
}