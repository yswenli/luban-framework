/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： StartFlowResponse.cs
*版本号： V1.0.0.0
*唯一标识：38107617-4765-4ef8-87d4-7c2aa467f65f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：StartFlowResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：StartFlowResponse 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 启动流程响应，返回流程启动后的结果信息。
/// </summary>
public class StartFlowResponse
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程状态：pending/running/finished。
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
    /// 待处理任务列表。
    /// </summary>
    public List<PendingTaskInfo>? PendingTasks { get; set; }
}