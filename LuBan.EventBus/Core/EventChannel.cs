/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Core
*文件名： EventChannel
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件 Channel 实现
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件 Channel 实现
*
*****************************************************************************/
using LuBan.Common.EventBus;
using LuBan.EventBus.Models;

namespace LuBan.EventBus.Core;

/// <summary>
/// 事件 Channel 实现
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public class EventChannel<TEvent> : IEventChannel<TEvent> where TEvent : IEventData
{
    private readonly Channel<TEvent> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="options">配置选项</param>
    public EventChannel(IServiceProvider serviceProvider, EventBusOptions options)
    {
        var fullMode = options.FullMode switch
        {
            1 => BoundedChannelFullMode.DropNewest,
            2 => BoundedChannelFullMode.DropOldest,
            _ => BoundedChannelFullMode.Wait
        };

        var channelOptions = new BoundedChannelOptions(options.MaxQueueCapacity)
        {
            FullMode = fullMode,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<TEvent>(channelOptions);

        _processingTask = Task.Run(() => ProcessEventsAsync(
            serviceProvider, _cts.Token));

        Logger.Debug($"EventChannel: 创建 {typeof(TEvent).Name} 的 Channel");
    }

    /// <summary>
    /// Channel 写入器
    /// </summary>
    public ChannelWriter<TEvent> Writer => _channel.Writer;

    /// <summary>
    /// 处理事件
    /// </summary>
    private async Task ProcessEventsAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        Logger.Debug($"EventChannel: {typeof(TEvent).Name} 后台处理任务已启动");

        try
        {
            await foreach (var eventData in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                List<IEventHandler<TEvent>> handlers;
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    handlers = scope.ServiceProvider
                        .GetServices<IEventHandler<TEvent>>()
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error($"EventChannel: {typeof(TEvent).Name} 解析事件处理器失败", ex);
                    continue;
                }

                if (handlers.Count == 0)
                {
                    Logger.Error($"EventChannel: {typeof(TEvent).Name} 未找到任何事件处理器，事件将被丢弃");
                    continue;
                }

                Logger.Debug($"EventChannel: {typeof(TEvent).Name} 找到 {handlers.Count} 个处理器，开始处理");

                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler.HandleAsync(eventData);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"EventChannel: 处理事件 {typeof(TEvent).Name} 失败", ex);
                        try
                        {
                            await handler.OnErrorAsync(ex);
                        }
                        catch (Exception onErrorEx)
                        {
                            Logger.Error($"EventChannel: {typeof(TEvent).Name} OnErrorAsync 执行失败", onErrorEx);
                        }
                    }
                }

                Logger.Debug($"EventChannel: 处理事件 {typeof(TEvent).Name} 完成");
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Debug($"EventChannel: {typeof(TEvent).Name} 后台处理任务已取消");
        }
        catch (Exception ex)
        {
            Logger.Error($"EventChannel: {typeof(TEvent).Name} 后台处理任务异常退出", ex);
        }

        Logger.Debug($"EventChannel: {typeof(TEvent).Name} 后台处理任务已终止");
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _channel.Writer.Complete();

        try
        {
            _processingTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
        }

        _cts.Dispose();
        Logger.Debug($"EventChannel: 释放 {typeof(TEvent).Name} 的 Channel");
    }
}
