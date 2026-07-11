namespace LuBan.ApprovalFlow.Extensions;

/// <summary>
/// IServiceCollection 扩展方法集合，用于注册审批流相关的服务。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册审批流核心服务。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="options">审批流配置选项，为空时使用默认配置。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    /// <remarks>
    /// 注册的服务包括：
    /// - 单例服务：配置选项、流程构建器、事件监听管理器、聚合评估器、规则引擎、HTTP回调执行器、审批流引擎
    /// - 瞬态服务：开始节点处理器、结束节点处理器、HTTP节点处理器
    /// </remarks>
    public static IServiceCollection AddApprovalFlow(
        this IServiceCollection services,
        ApprovalFlowOptions? options = null)
    {
        // 使用默认配置如果未提供
        options ??= new ApprovalFlowOptions();

        // 注册配置选项为单例
        services.AddSingleton(options);
        // 注册核心服务为单例
        services.AddSingleton<FlowBuilder>();
        services.AddSingleton<FlowEventListenerManager>();
        services.AddSingleton<AggregationEvaluator>();
        services.AddSingleton<RuleEngine>();
        services.AddSingleton<HttpCallbackExecutor>();
        services.AddSingleton<ApprovalFlowEngine>();
        services.AddSingleton<FlowEventBusPublisher>();

        // 注册 EventBus 事件处理器（支持多个处理器）
        services.AddSingleton<IFlowEventHandler, HttpCallbackEventHandler>();

        // 注册节点处理器为瞬态，每次请求创建新实例
        services.AddTransient<StartNodeHandler>();
        services.AddTransient<EndNodeHandler>();
        services.AddTransient<HttpNodeHandler>();

        return services;
    }

    /// <summary>
    /// 注册审批流事件监听器。
    /// </summary>
    /// <typeparam name="TListener">事件监听器实现类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddApprovalFlowListener<TListener>(
        this IServiceCollection services)
        where TListener : class, IFlowEventListener
    {
        services.AddSingleton<IFlowEventListener, TListener>();
        return services;
    }

    /// <summary>
    /// 注册审批流数据仓储实现。
    /// </summary>
    /// <typeparam name="TRepository">数据仓储实现类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddApprovalRepository<TRepository>(
        this IServiceCollection services)
        where TRepository : class, IApprovalRepository
    {
        services.AddScoped<IApprovalRepository, TRepository>();
        return services;
    }

    /// <summary>
    /// 注册审批流用户服务实现。
    /// </summary>
    /// <typeparam name="TUserService">用户服务实现类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddApprovalUserService<TUserService>(
        this IServiceCollection services)
        where TUserService : class, IUserService
    {
        services.AddScoped<IUserService, TUserService>();
        return services;
    }

    /// <summary>
    /// 注册审批流通知服务实现。
    /// </summary>
    /// <typeparam name="TNotificationService">通知服务实现类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddApprovalNotificationService<TNotificationService>(
        this IServiceCollection services)
        where TNotificationService : class, INotificationService
    {
        services.AddScoped<INotificationService, TNotificationService>();
        return services;
    }

    /// <summary>
    /// 注册审批流统计更新器实现。
    /// </summary>
    /// <typeparam name="TStatisticsUpdater">统计更新器实现类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddStatisticsUpdater<TStatisticsUpdater>(
        this IServiceCollection services)
        where TStatisticsUpdater : class, IStatisticsUpdater
    {
        services.AddScoped<IStatisticsUpdater, TStatisticsUpdater>();
        return services;
    }
}