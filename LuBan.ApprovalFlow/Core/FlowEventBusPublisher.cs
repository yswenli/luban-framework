/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.ApprovalFlow.Core
*文件名： FlowEventBusPublisher
*版本号： V1.0.0.0
*唯一标识：6888534d-e744-4918-88ba-9c51abce2948
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/11
*描述：审批流 EventBus 事件发布器
*
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
        _eventBus = eventBus ?? DiContainer.Instance.GetService<IEventBus>();
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
