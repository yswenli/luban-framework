/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： StartFlowRequest.cs
*版本号： V1.0.0.0
*唯一标识：90d3ae76-6ca1-4f5a-b76f-b8407ff3a959
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：StartFlowRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：StartFlowRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 启动流程请求，包含启动审批流所需的所有参数。
/// </summary>
public class StartFlowRequest
{
    /// <summary>
    /// 流程定义ID（与FlowCode二选一）。
    /// </summary>
    public long? FlowId { get; set; }
    /// <summary>
    /// 流程编码（与FlowId二选一）。
    /// </summary>
    public string? FlowCode { get; set; }
    /// <summary>
    /// 业务主键，用于关联业务数据。
    /// </summary>
    public string? BusinessKey { get; set; }
    /// <summary>
    /// 表单数据载荷。
    /// </summary>
    public object? FormPayload { get; set; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }
    /// <summary>
    /// 发起人用户ID。
    /// </summary>
    public long InitiatorUserId { get; set; }
    /// <summary>
    /// 发起人名称。
    /// </summary>
    public string InitiatorName { get; set; } = string.Empty;
}