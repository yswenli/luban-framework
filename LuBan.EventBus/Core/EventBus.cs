/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Core
*文件名： EventBus
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件总线主实现
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件总线主实现
*
*****************************************************************************/
using LuBan.Common.EventBus;
using LuBan.EventBus.Models;

namespace LuBan.EventBus.Core;

/// <summary>
/// 事件总线实现，使用 System.Threading.Channels
/// </summary>
public class EventBus : IEventBus, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusOptions _options;
    private readonly EventPersistence _persistence;

    private readonly ConcurrentDictionary<Type, IEventChannel> _channels = new();
    private readonly ConcurrentDictionary<Type, SubscriptionInfo> _subscriptions = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="options">配置选项</param>
    /// <param name="persistence">持久化服务</param>
    public EventBus(
        IServiceProvider serviceProvider,
        IOptions<EventBusOptions> options,
        EventPersistence persistence)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _persistence = persistence;

        ConsoleUtil.SetEventBus(this);

        Logger.Debug("EventBus: 初始化完成");
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    /// <returns>任务</returns>
    public async Task PublishAsync<TEvent>(TEvent eventData) where TEvent : IEventData
    {
        var channel = GetOrCreateChannel<TEvent>();
        await channel.Writer.WriteAsync(eventData);

        Logger.Debug($"EventBus: 发布事件 {typeof(TEvent).Name} 已写入Channel");

        if (_options.EnablePersistence)
        {
            await _persistence.SaveAsync(eventData);
        }

        Logger.Debug($"EventBus: 发布事件 {typeof(TEvent).Name} 完成");
    }

    /// <summary>
    /// 订阅事件（永久订阅）
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <typeparam name="THandler">处理器类型</typeparam>
    public void Subscribe<TEvent, THandler>()
        where TEvent : IEventData
        where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        _subscriptions[eventType] = new SubscriptionInfo(typeof(THandler), false);
        GetOrCreateChannel<TEvent>();

        Logger.Debug($"EventBus: 订阅事件 {typeof(TEvent).Name} -> {typeof(THandler).Name}");
    }

    /// <summary>
    /// 订阅事件（一次性订阅）
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <typeparam name="THandler">处理器类型</typeparam>
    public void SubscribeOnce<TEvent, THandler>()
        where TEvent : IEventData
        where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        _subscriptions[eventType] = new SubscriptionInfo(typeof(THandler), true);
        GetOrCreateChannel<TEvent>();

        Logger.Debug($"EventBus: 一次性订阅事件 {typeof(TEvent).Name} -> {typeof(THandler).Name}");
    }

    /// <summary>
    /// 取消订阅
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    public void Unsubscribe<TEvent>()
    {
        var eventType = typeof(TEvent);
        _subscriptions.TryRemove(eventType, out _);

        if (_channels.TryRemove(eventType, out var channel))
        {
            channel.Dispose();
        }

        Logger.Debug($"EventBus: 取消订阅事件 {typeof(TEvent).Name}");
    }

    /// <summary>
    /// 获取订阅数量
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <returns>订阅数量</returns>
    public int GetSubscriberCount<TEvent>()
    {
        return _subscriptions.Count(s => s.Key == typeof(TEvent));
    }

    /// <summary>
    /// 获取或创建事件 Channel
    /// </summary>
    private EventChannel<TEvent> GetOrCreateChannel<TEvent>() where TEvent : IEventData
    {
        return (EventChannel<TEvent>)_channels.GetOrAdd(
            typeof(TEvent),
            _ => new EventChannel<TEvent>(_serviceProvider, _options));
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        foreach (var channel in _channels.Values)
        {
            channel.Dispose();
        }
        _channels.Clear();
        _subscriptions.Clear();

        Logger.Debug("EventBus: 已释放所有资源");
    }
}
