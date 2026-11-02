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
*命名空间：LuBan.ApprovalFlow.Abstractions
*文件名： IFlowEventHandler
*版本号： V1.0.0.0
*唯一标识：b6f31485-d3b7-43b0-8b42-c13dc4c4998c
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/11
*描述：审批流 EventBus 事件处理器接口
*
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
