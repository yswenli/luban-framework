namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 节点进入事件参数，当流程进入某个节点时触发。
/// </summary>
public class NodeEnterEventArgs : EventArgs
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
    /// 进入时间。
    /// </summary>
    public DateTime EnterTime { get; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; }

    /// <summary>
    /// 构造节点进入事件参数。
    /// </summary>
    public NodeEnterEventArgs(long recordId, string nodeId, string nodeName, string nodeType, string nodeStatus, DateTime enterTime, Dictionary<string, object>? variables, string? businessKey = null)
    {
        RecordId = recordId;
        NodeId = nodeId;
        NodeName = nodeName;
        NodeType = nodeType;
        NodeStatus = nodeStatus;
        EnterTime = enterTime;
        Variables = variables;
        BusinessKey = businessKey;
    }
}