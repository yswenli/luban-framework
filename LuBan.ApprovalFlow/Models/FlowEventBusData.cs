/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowEventBusData
*描述：审批流 EventBus 事件数据基类
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批流 EventBus 事件数据基类，继承自 LuBan.EventBus 的 BaseEventData。
/// </summary>
[Serializable]
public class FlowEventBusData : LuBan.EventBus.Models.BaseEventData
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 流程编码。
    /// </summary>
    public string? FlowCode { get; set; }

    /// <summary>
    /// 节点ID。
    /// </summary>
    public string? NodeId { get; set; }

    /// <summary>
    /// 节点名称。
    /// </summary>
    public string? NodeName { get; set; }

    /// <summary>
    /// 节点类型。
    /// </summary>
    public string? NodeType { get; set; }

    /// <summary>
    /// 操作类型（如 approve、reject、return 等）。
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// 操作人ID。
    /// </summary>
    public long? ActorUserId { get; set; }

    /// <summary>
    /// 操作人姓名。
    /// </summary>
    public string? ActorName { get; set; }

    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }

    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; set; }

    /// <summary>
    /// 节点属性快照（包含 events 等配置）。
    /// </summary>
    public Dictionary<string, object>? NodeProperties { get; set; }

    /// <summary>
    /// 构造函数。
    /// </summary>
    public FlowEventBusData() : base() { }

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="name">事件名称。</param>
    public FlowEventBusData(string name) : base(name) { }

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="name">事件名称。</param>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="flowCode">流程编码。</param>
    public FlowEventBusData(string name, long recordId, string? flowCode = null) : base(name)
    {
        RecordId = recordId;
        FlowCode = flowCode;
    }
}

/// <summary>
/// 流程初始化事件。
/// </summary>
[Serializable]
public class FlowInitializedBusEvent : FlowEventBusData
{
    public FlowInitializedBusEvent() : base("FlowInitialized") { }
    public FlowInitializedBusEvent(long recordId, string? flowCode = null) : base("FlowInitialized", recordId, flowCode) { }
}

/// <summary>
/// 流程完成事件。
/// </summary>
[Serializable]
public class FlowCompletedBusEvent : FlowEventBusData
{
    public FlowCompletedBusEvent() : base("FlowCompleted") { }
    public FlowCompletedBusEvent(long recordId, string? flowCode = null) : base("FlowCompleted", recordId, flowCode) { }
}

/// <summary>
/// 流程被拒绝事件。
/// </summary>
[Serializable]
public class FlowRejectedBusEvent : FlowEventBusData
{
    public FlowRejectedBusEvent() : base("FlowRejected") { }
    public FlowRejectedBusEvent(long recordId, string? flowCode = null) : base("FlowRejected", recordId, flowCode) { }
}

/// <summary>
/// 节点进入事件。
/// </summary>
[Serializable]
public class NodeEnterBusEvent : FlowEventBusData
{
    public NodeEnterBusEvent() : base("NodeEnter") { }
    public NodeEnterBusEvent(long recordId, string? flowCode = null) : base("NodeEnter", recordId, flowCode) { }
}

/// <summary>
/// 节点离开事件。
/// </summary>
[Serializable]
public class NodeLeaveBusEvent : FlowEventBusData
{
    public NodeLeaveBusEvent() : base("NodeLeave") { }
    public NodeLeaveBusEvent(long recordId, string? flowCode = null) : base("NodeLeave", recordId, flowCode) { }
}

/// <summary>
/// 审批操作事件。
/// </summary>
[Serializable]
public class ApprovalActionBusEvent : FlowEventBusData
{
    public ApprovalActionBusEvent() : base("ApprovalAction") { }
    public ApprovalActionBusEvent(long recordId, string? flowCode = null) : base("ApprovalAction", recordId, flowCode) { }
}
