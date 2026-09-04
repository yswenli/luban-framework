/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： StepInfo.cs
*版本号： V1.0.0.0
*唯一标识：a59a4a30-e92b-492f-ada1-7c21d051ca4a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：StepInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：StepInfo 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 步骤信息，描述单次审批操作的详情。
/// </summary>
public class StepInfo
{
    /// <summary>
    /// 节点名称。
    /// </summary>
    public string? NodeName { get; set; }
    /// <summary>
    /// 操作人名称。
    /// </summary>
    public string ActorName { get; set; } = string.Empty;
    /// <summary>
    /// 操作人角色。
    /// </summary>
    public string? ActorRole { get; set; }
    /// <summary>
    /// 操作动作：approve/reject/return/cancel等。
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// 审批意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? Payload { get; set; }
    /// <summary>
    /// 操作时间。
    /// </summary>
    public DateTime ActionTime { get; set; }
    /// <summary>
    /// 是否系统自动操作。
    /// </summary>
    public bool IsSystemAction { get; set; }
}