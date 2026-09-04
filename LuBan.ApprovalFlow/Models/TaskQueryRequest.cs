/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： TaskQueryRequest.cs
*版本号： V1.0.0.0
*唯一标识：28bd9532-5164-4eab-a0f9-7c4bf5da399b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：TaskQueryRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：TaskQueryRequest 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 任务查询请求，用于查询待办或已办任务列表。
/// </summary>
public class TaskQueryRequest
{
    /// <summary>
    /// 页码，默认1。
    /// </summary>
    public int Page { get; set; } = 1;
    /// <summary>
    /// 每页数量，默认20。
    /// </summary>
    public int PageSize { get; set; } = 20;
    /// <summary>
    /// 流程名称筛选。
    /// </summary>
    public string? FlowName { get; set; }
    /// <summary>
    /// 节点状态筛选。
    /// </summary>
    public string? NodeStatus { get; set; }
    /// <summary>
    /// 开始时间筛选。
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束时间筛选。
    /// </summary>
    public DateTime? EndTime { get; set; }
}