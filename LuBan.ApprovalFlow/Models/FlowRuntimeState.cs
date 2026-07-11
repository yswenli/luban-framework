/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowRuntimeState
*版本号： V1.0.0.0
*唯一标识：7e55b76e-11e9-4116-9349-463645303e3a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:19
*描述：运行时状态（持久化到记录FlowResult）
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：运行时状态（持久化到记录FlowResult）
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;


/// <summary>
/// 运行时状态（持久化到记录FlowResult）
/// </summary>
public class FlowRuntimeState
{
    /// <summary>
    /// 当前所在节点Key，结束时为空。
    /// </summary>
    public string? CurrentNodeKey { get; set; }
    /// <summary>
    /// 流程状态：pending/running/finished/rejected。
    /// </summary>
    public string Status { get; set; } = "pending";
    /// <summary>
    /// 步骤历史列表。
    /// </summary>
    public List<FlowStepResult> History { get; set; } = new();
    /// <summary>
    /// 运行时上下文，用于在节点之间传导值（例如上一个节点返回值、边上的动作标签）。
    /// </summary>
    public Dictionary<string, object>? Context { get; set; } = new();
    /// <summary>
    /// 流程变量，用于条件表达式计算和节点间传递业务数据。
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; } = new();
}