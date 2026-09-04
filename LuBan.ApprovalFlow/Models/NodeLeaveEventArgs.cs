/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： NodeLeaveEventArgs.cs
*版本号： V1.0.0.0
*唯一标识：7040bba7-47a6-4f64-ac6f-be18ca867a86
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：NodeLeaveEventArgs 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：NodeLeaveEventArgs 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 节点离开事件参数，当流程离开某个节点时触发。
/// </summary>
public class NodeLeaveEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; }
    /// <summary>
    /// 节点名称。
    /// </summary>
    public string NodeName { get; }
    /// <summary>
    /// 节点类型。
    /// </summary>
    public string NodeType { get; }
    /// <summary>
    /// 节点状态。
    /// </summary>
    public string NodeStatus { get; }
    /// <summary>
    /// 离开时间。
    /// </summary>
    public DateTime LeaveTime { get; }
    /// <summary>
    /// 节点停留时长（秒）。
    /// </summary>
    public int? Duration { get; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }

    /// <summary>
    /// 构造节点离开事件参数。
    /// </summary>
    public NodeLeaveEventArgs(long recordId, string nodeId, string nodeName, string nodeType, string nodeStatus, DateTime leaveTime, int? duration, Dictionary<string, object>? variables, string? businessKey = null)
    {
        RecordId = recordId;
        NodeId = nodeId;
        NodeName = nodeName;
        NodeType = nodeType;
        NodeStatus = nodeStatus;
        LeaveTime = leaveTime;
        Duration = duration;
        Variables = variables;
        BusinessKey = businessKey;
    }
}