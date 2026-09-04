/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： ApprovalResponse.cs
*版本号： V1.0.0.0
*唯一标识：34aad7d9-0f4d-4cfb-9369-595fd7b343ba
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ApprovalResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ApprovalResponse 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批响应，返回审批操作后的结果信息。
/// </summary>
public class ApprovalResponse
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程状态：pending/running/finished/rejected。
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// 当前节点ID。
    /// </summary>
    public string? CurrentNodeId { get; set; }
    /// <summary>
    /// 当前节点名称。
    /// </summary>
    public string? CurrentNodeName { get; set; }
    /// <summary>
    /// 聚合结果，用于多实例节点：pending/approved/rejected/returned/cancelled。
    /// </summary>
    public string? AggregationResult { get; set; }
}