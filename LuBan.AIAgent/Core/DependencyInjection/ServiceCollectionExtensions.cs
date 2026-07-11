using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Core;

namespace LuBan.AIAgent.DependencyInjection;

/// <summary>
/// 服务集合扩展类，提供依赖注入相关的扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Agile AI服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAgileAI(this IServiceCollection services)
    {
        services.AddSingleton<ChatClient>(sp =>
        {
            var chatClient = new ChatClient(sp.GetService<Microsoft.Extensions.Logging.ILogger<ChatClient>>());
            var providers = sp.GetServices<IChatModelProvider>();
            foreach (var provider in providers)
            {
                chatClient.RegisterProvider(provider);
            }
            return chatClient;
        });
        services.AddSingleton<IChatClient>(sp => sp.GetRequiredService<ChatClient>());
        services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
        services.AddSingleton<ISkillRegistry, InMemorySkillRegistry>();
        services.AddSingleton<ISkillPlanner, RuleBasedSkillPlanner>();
        services.AddSingleton<ISkillContinuationPolicy, DefaultSkillContinuationPolicy>();
        services.AddSingleton<ISessionStore, InMemorySessionStore>();
        services.AddSingleton<ISkillExecutor, PromptSkillExecutor>();
        services.AddSingleton<ILocalSkillLoader, LocalFileSkillLoader>();
        services.AddSingleton<IAgentRuntime, DefaultAgentRuntime>();

        return services;
    }

    /// <summary>
    /// 添加聊天客户端提供者
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="provider">聊天模型提供者</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddChatClientProvider(this IServiceCollection services, IChatModelProvider provider)
    {
        services.AddSingleton(provider);
        return services;
    }

    /// <summary>
    /// 添加代理执行中间件
    /// </summary>
    /// <typeparam name="TMiddleware">中间件类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAgentExecutionMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IAgentExecutionMiddleware
    {
        services.AddSingleton<IAgentExecutionMiddleware, TMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加聊天回合中间件
    /// </summary>
    /// <typeparam name="TMiddleware">中间件类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddChatTurnMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IChatTurnMiddleware
    {
        services.AddSingleton<IChatTurnMiddleware, TMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加流式聊天回合中间件
    /// </summary>
    /// <typeparam name="TMiddleware">中间件类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddStreamingChatTurnMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IStreamingChatTurnMiddleware
    {
        services.AddSingleton<IStreamingChatTurnMiddleware, TMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加工具执行中间件
    /// </summary>
    /// <typeparam name="TMiddleware">中间件类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddToolExecutionMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IToolExecutionMiddleware
    {
        services.AddSingleton<IToolExecutionMiddleware, TMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加日志聊天回合中间件
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLoggingChatTurnMiddleware(this IServiceCollection services, Action<LoggingMiddlewareOptions>? configure = null)
    {
        var options = new LoggingMiddlewareOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IChatTurnMiddleware, LoggingChatTurnMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加日志流式聊天回合中间件
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLoggingStreamingChatTurnMiddleware(this IServiceCollection services, Action<LoggingMiddlewareOptions>? configure = null)
    {
        var options = new LoggingMiddlewareOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IStreamingChatTurnMiddleware, LoggingStreamingChatTurnMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加日志工具执行中间件
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLoggingToolExecutionMiddleware(this IServiceCollection services, Action<LoggingMiddlewareOptions>? configure = null)
    {
        var options = new LoggingMiddlewareOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IToolExecutionMiddleware, LoggingToolExecutionMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加工具策略中间件
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddToolPolicyMiddleware(this IServiceCollection services, Action<ToolPolicyOptions>? configure)
    {
        var options = new ToolPolicyOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IToolExecutionMiddleware, ToolPolicyMiddleware>();
        return services;
    }

    /// <summary>
    /// 添加本地技能
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLocalSkills(this IServiceCollection services, Action<LocalSkillsOptions>? configure = null)
    {
        var options = new LocalSkillsOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton<ISkillRegistry>(sp =>
        {
            var registry = new InMemorySkillRegistry();
            var loader = new LocalFileSkillLoader(options);
            var executor = sp.GetRequiredService<ISkillExecutor>();
            var manifests = loader.LoadFromDirectoryAsync(options.RootDirectory).GetAwaiter().GetResult();
            registry.Register(manifests.Select(m => (ISkill)new LocalFileSkill(m, executor)));
            return registry;
        });

        return services;
    }

    /// <summary>
    /// 添加文件会话存储
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddFileSessionStore(this IServiceCollection services, Action<FileSessionStoreOptions>? configure = null)
    {
        var options = new FileSessionStoreOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.RemoveAll<ISessionStore>();
        services.AddSingleton<ISessionStore>(sp => new FileSessionStore(
            sp.GetRequiredService<FileSessionStoreOptions>(),
            sp.GetService<Microsoft.Extensions.Logging.ILogger<FileSessionStore>>()));

        return services;
    }
}
