namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 流程事件监听器管理器：集中管理和触发流程生命周期事件。
/// 采用单例模式，支持注册多个监听器并按流程键过滤事件。
/// </summary>
public class FlowEventListenerManager
{
    /// <summary>
    /// 已注册的事件监听器列表。
    /// </summary>
    private readonly List<IFlowEventListener> _listeners = new();

    /// <summary>
    /// 注册事件监听器。
    /// </summary>
    /// <param name="listener">要注册的监听器实例。</param>
    public void Register(IFlowEventListener listener)
    {
        _listeners.Add(listener);
    }

    /// <summary>
    /// 注销事件监听器。
    /// </summary>
    /// <param name="listener">要注销的监听器实例。</param>
    public void Unregister(IFlowEventListener listener)
    {
        _listeners.Remove(listener);
    }

    /// <summary>
    /// 清除所有已注册的监听器。
    /// </summary>
    public void Clear()
    {
        _listeners.Clear();
    }

    /// <summary>
    /// 触发指定事件，通知所有符合条件的监听器。
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型。</typeparam>
    /// <param name="eventName">事件名称。</param>
    /// <param name="args">事件参数。</param>
    /// <param name="flowKey">流程键（用于过滤监听器）。</param>
    public async Task TriggerEventAsync<TArgs>(string eventName, TArgs args, string? flowKey)
    {
        // 并行触发所有符合条件的监听器
        var tasks = _listeners
            .Where(l => l.ShouldListen(flowKey ?? string.Empty))
            .Select(async l =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        // 根据事件名称调用对应的监听器方法
                        switch (eventName)
                        {
                            case "OnInitialized":
                                if (args is FlowInitializedEventArgs initArgs)
                                    l.OnInitialized(initArgs);
                                break;
                            case "OnCompleted":
                                if (args is FlowCompletedEventArgs completedArgs)
                                    l.OnCompleted(completedArgs);
                                break;
                            case "OnRejected":
                                if (args is FlowRejectedEventArgs rejectedArgs)
                                    l.OnRejected(rejectedArgs);
                                break;
                            case "OnCancelled":
                                if (args is FlowCancelledEventArgs cancelledArgs)
                                    l.OnCancelled(cancelledArgs);
                                break;
                            case "OnReturned":
                                if (args is FlowReturnedEventArgs returnedArgs)
                                    l.OnReturned(returnedArgs);
                                break;
                            case "OnError":
                                if (args is FlowErrorEventArgs errorArgs)
                                    l.OnError(errorArgs);
                                break;
                            case "OnNodeEnter":
                                if (args is NodeEnterEventArgs enterArgs)
                                    l.OnNodeEnter(enterArgs);
                                break;
                            case "OnNodeLeave":
                                if (args is NodeLeaveEventArgs leaveArgs)
                                    l.OnNodeLeave(leaveArgs);
                                break;
                            case "OnApprovalAction":
                                if (args is ApprovalActionEventArgs actionArgs)
                                    l.OnApprovalAction(actionArgs);
                                break;
                        }
                    });
                }
                catch (Exception ex)
                {
                    // 监听器执行失败时记录警告日志，不影响其他监听器
                    Logger.Warn($"监听器执行失败 [{l.GetType().Name}]: {ex.Message}");
                }
            });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 获取指定流程键的所有监听器。
    /// </summary>
    /// <param name="flowKey">流程键。</param>
    /// <returns>符合条件的监听器列表。</returns>
    public List<IFlowEventListener> GetListeners(string? flowKey)
    {
        return _listeners.Where(l => l.ShouldListen(flowKey ?? string.Empty)).ToList();
    }

    /// <summary>
    /// 获取管理器单例实例（优先从依赖注入容器获取）。
    /// </summary>
    public static FlowEventListenerManager Instance
    {
        get
        {
            // 尝试从服务提供者获取实例
            var instance = ServiceProviderUtil.GetService<FlowEventListenerManager>();
            if (instance != null) return instance;
            // 容器中不存在则创建新实例
            return new FlowEventListenerManager();
        }
    }
}