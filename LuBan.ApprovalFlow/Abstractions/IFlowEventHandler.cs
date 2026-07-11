/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*命名空间：LuBan.ApprovalFlow.Abstractions
*文件名： IFlowEventHandler
*描述：审批流 EventBus 事件处理器接口
*****************************************************************************/

namespace LuBan.ApprovalFlow.Abstractions;

/// <summary>
/// 审批流 EventBus 事件处理器接口。
/// 业务层可实现此接口来处理审批流中的各类事件。
/// </summary>
public interface IFlowEventHandler
{
    /// <summary>
    /// 处理事件。
    /// </summary>
    /// <param name="eventData">事件数据。</param>
    /// <returns>异步任务。</returns>
    Task HandleAsync(FlowEventBusData eventData);

    /// <summary>
    /// 判断是否处理指定流程的事件。
    /// </summary>
    /// <param name="flowCode">流程编码。</param>
    /// <returns>是否处理，默认 true。</returns>
    bool ShouldHandle(string flowCode) => true;
}

/// <summary>
/// 审批流事件处理器基类，提供默认实现。
/// </summary>
public abstract class FlowEventHandlerBase : IFlowEventHandler
{
    /// <summary>
    /// 处理事件。
    /// </summary>
    public abstract Task HandleAsync(FlowEventBusData eventData);

    /// <summary>
    /// 判断是否处理指定流程的事件。
    /// </summary>
    public virtual bool ShouldHandle(string flowCode) => true;
}
