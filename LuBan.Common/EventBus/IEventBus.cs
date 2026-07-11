/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.EventBus
*文件名： IEventBus
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件总线核心接口
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件总线核心接口
*
*****************************************************************************/
namespace LuBan.Common.EventBus;

/// <summary>
/// 事件总线核心接口
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    /// <returns>任务</returns>
    Task PublishAsync<TEvent>(TEvent eventData) where TEvent : IEventData;

    /// <summary>
    /// 订阅事件（永久订阅）
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <typeparam name="THandler">处理器类型</typeparam>
    void Subscribe<TEvent, THandler>()
        where TEvent : IEventData
        where THandler : IEventHandler<TEvent>;

    /// <summary>
    /// 订阅事件（一次性订阅，处理后自动取消）
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <typeparam name="THandler">处理器类型</typeparam>
    void SubscribeOnce<TEvent, THandler>()
        where TEvent : IEventData
        where THandler : IEventHandler<TEvent>;

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    void Unsubscribe<TEvent>();

    /// <summary>
    /// 获取订阅数量（用于监控）
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <returns>订阅数量</returns>
    int GetSubscriberCount<TEvent>();
}
