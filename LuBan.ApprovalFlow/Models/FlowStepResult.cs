/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowStepResult
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：单步结果
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：单步结果
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;


/// <summary>
/// 单步结果
/// </summary>
public class FlowStepResult
{
    /// <summary>
    /// 执行的节点Key。
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    /// <summary>
    /// 节点显示名称。
    /// </summary>
    public string NodeName { get; set; } = string.Empty;
    /// <summary>
    /// 步骤结果：approve/reject/auto。
    /// </summary>
    public string Outcome { get; set; } = string.Empty; // approve/reject/auto
    /// <summary>
    /// 发生时间。
    /// </summary>
    public DateTime Time { get; set; } = DateTime.Now;
    /// <summary>
    /// 执行人用户ID（可选）。
    /// </summary>
    public long? ActorUserId { get; set; }
    /// <summary>
    /// 备注或审批意见（可选）。
    /// </summary>
    public string? Comment { get; set; }
}