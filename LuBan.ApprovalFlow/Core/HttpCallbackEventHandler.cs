/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*命名空间：LuBan.ApprovalFlow.Core
*文件名： HttpCallbackEventHandler
*描述：默认的审批流 HTTP 回调事件处理器
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 默认的审批流 HTTP 回调事件处理器。
/// 读取节点属性中的事件配置，通过 HttpClientProxy 发送回调请求。
/// </summary>
public class HttpCallbackEventHandler : IFlowEventHandler
{
    /// <summary>
    /// 处理事件：根据事件名称匹配节点配置中的回调，执行 HTTP 请求。
    /// </summary>
    public async Task HandleAsync(FlowEventBusData eventData)
    {
        if (eventData?.NodeProperties == null) return;

        // 从节点属性中获取事件配置
        if (!eventData.NodeProperties.TryGetValue("events", out var eventsObj) || eventsObj == null)
            return;

        var eventsJson = SerializeUtil.Serialize(eventsObj);
        var events = SerializeUtil.Deserialize<Dictionary<string, HttpCallbackConfig>>(eventsJson);
        if (events == null) return;

        // 根据事件名称匹配回调配置
        var eventName = eventData.Name;
        if (!events.TryGetValue(eventName, out var config) || config == null)
            return;

        // 构建执行上下文
        var context = BuildContext(eventData);

        // 复用 HttpCallbackExecutor 执行请求
        var executor = new HttpCallbackExecutor();
        await executor.ExecuteAsync(config, context);
    }

    /// <summary>
    /// 构建流程执行上下文。
    /// </summary>
    private FlowExecutionContext BuildContext(FlowEventBusData eventData)
    {
        var variables = eventData.Variables ?? new Dictionary<string, object>();

        // 将事件数据注入变量上下文，供占位符解析使用
        variables["recordId"] = eventData.RecordId;
        if (!string.IsNullOrEmpty(eventData.FlowCode))
            variables["flowCode"] = eventData.FlowCode;
        if (!string.IsNullOrEmpty(eventData.NodeId))
            variables["nodeId"] = eventData.NodeId;
        if (!string.IsNullOrEmpty(eventData.NodeName))
            variables["nodeName"] = eventData.NodeName;
        if (!string.IsNullOrEmpty(eventData.NodeType))
            variables["nodeType"] = eventData.NodeType;
        if (!string.IsNullOrEmpty(eventData.Action))
            variables["action"] = eventData.Action;
        if (eventData.ActorUserId.HasValue)
            variables["actorUserId"] = eventData.ActorUserId.Value;
        if (!string.IsNullOrEmpty(eventData.ActorName))
            variables["actorName"] = eventData.ActorName;
        if (!string.IsNullOrEmpty(eventData.BusinessKey))
            variables["businessKey"] = eventData.BusinessKey;

        return new FlowExecutionContext
        {
            RecordId = eventData.RecordId,
            Variables = variables
        };
    }

    /// <summary>
    /// 默认处理所有流程。
    /// </summary>
    public bool ShouldHandle(string flowCode) => true;
}
