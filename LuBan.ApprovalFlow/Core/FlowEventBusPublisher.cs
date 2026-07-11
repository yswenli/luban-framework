/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*命名空间：LuBan.ApprovalFlow.Core
*文件名： FlowEventBusPublisher
*描述：审批流 EventBus 事件发布器
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 审批流 EventBus 事件发布器。
/// 封装 LuBan.EventBus 的发布逻辑，在流程关键节点发布事件。
/// </summary>
public class FlowEventBusPublisher
{
    private readonly IEventBus? _eventBus;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="eventBus">事件总线实例（可选）。</param>
    public FlowEventBusPublisher(IEventBus? eventBus = null)
    {
        _eventBus = eventBus ?? ServiceProviderUtil.GetService<IEventBus>();
    }

    /// <summary>
    /// 发布流程初始化事件。
    /// </summary>
    public async Task PublishInitializedAsync(long recordId, string? flowCode, Dictionary<string, object>? variables = null, string? businessKey = null)
    {
        if (_eventBus == null) return;

        var eventData = new FlowInitializedBusEvent(recordId, flowCode)
        {
            Variables = variables,
            BusinessKey = businessKey
        };
        await _eventBus.PublishAsync(eventData);
    }

    /// <summary>
    /// 发布流程完成事件。
    /// </summary>
    public async Task PublishCompletedAsync(long recordId, string? flowCode, Dictionary<string, object>? variables = null)
    {
        if (_eventBus == null) return;

        var eventData = new FlowCompletedBusEvent(recordId, flowCode)
        {
            Variables = variables
        };
        await _eventBus.PublishAsync(eventData);
    }

    /// <summary>
    /// 发布流程被拒绝事件。
    /// </summary>
    public async Task PublishRejectedAsync(long recordId, string? flowCode, Dictionary<string, object>? variables = null)
    {
        if (_eventBus == null) return;

        var eventData = new FlowRejectedBusEvent(recordId, flowCode)
        {
            Variables = variables
        };
        await _eventBus.PublishAsync(eventData);
    }

    /// <summary>
    /// 发布节点进入事件。
    /// </summary>
    public async Task PublishNodeEnterAsync(long recordId, string? flowCode, GraphNode node, Dictionary<string, object>? variables = null)
    {
        if (_eventBus == null) return;

        var eventData = new NodeEnterBusEvent(recordId, flowCode)
        {
            NodeId = node.Id,
            NodeName = node.Text?.Value,
            NodeType = node.Type,
            Variables = variables,
            NodeProperties = node.Properties?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value)
        };
        await _eventBus.PublishAsync(eventData);
    }

    /// <summary>
    /// 发布节点离开事件。
    /// </summary>
    public async Task PublishNodeLeaveAsync(long recordId, string? flowCode, GraphNode node, string? action = null, long? actorUserId = null, string? actorName = null, Dictionary<string, object>? variables = null)
    {
        if (_eventBus == null) return;

        var eventData = new NodeLeaveBusEvent(recordId, flowCode)
        {
            NodeId = node.Id,
            NodeName = node.Text?.Value,
            NodeType = node.Type,
            Action = action,
            ActorUserId = actorUserId,
            ActorName = actorName,
            Variables = variables,
            NodeProperties = node.Properties?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value)
        };
        await _eventBus.PublishAsync(eventData);
    }

    /// <summary>
    /// 发布审批操作事件。
    /// </summary>
    public async Task PublishApprovalActionAsync(long recordId, string? flowCode, GraphNode node, string action, long? actorUserId, string? actorName, Dictionary<string, object>? variables = null)
    {
        if (_eventBus == null) return;

        var eventData = new ApprovalActionBusEvent(recordId, flowCode)
        {
            NodeId = node.Id,
            NodeName = node.Text?.Value,
            NodeType = node.Type,
            Action = action,
            ActorUserId = actorUserId,
            ActorName = actorName,
            Variables = variables,
            NodeProperties = node.Properties?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value)
        };
        await _eventBus.PublishAsync(eventData);
    }
}
