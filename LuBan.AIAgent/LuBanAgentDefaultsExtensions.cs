/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent
*文件名： LuBanAgentDefaultsExtensions
*创建时间：2026/10/14
*描述：LuBan Agent「开箱即用」注册扩展：AddLuBanAgent + 框架契约的默认实现
*
*****************************************************************************/
using LuBan.AIAgent.Defaults;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LuBan.AIAgent;

/// <summary>
/// LuBan Agent「开箱即用」注册扩展。
/// <para>
/// <see cref="AddLuBanAgent(IServiceCollection, IConfiguration)"/> 只注册框架内部零件，
/// 会话上下文（<see cref="ISessionContext"/>）、本地记忆存储（<see cref="ILocalMemoryStore"/>）
/// 等契约留给宿主实现——直接用会导致工具组构建期抛 <see cref="InvalidOperationException"/>。
/// 本扩展在 <c>AddLuBanAgent</c> 之上补齐这些契约的内存/默认实现，使最小宿主三行即可跑通。
/// </para>
/// <para>
/// 全部默认实现均以 <c>TryAdd</c> 注册：<b>宿主先注册自己的实现即可覆盖</b>，
/// 不会与既有注册冲突。生产环境需持久化 / 多用户隔离时，请在调用本方法前注册
/// 自定义 <see cref="ISessionManager"/>、<see cref="ILocalMemoryStore"/> 及上下文实现。
/// </para>
/// </summary>
public static class LuBanAgentDefaultsExtensions
{
    /// <summary>
    /// 添加 LuBan Agent 服务并补齐默认实现（带自定义 ChatClient 工厂）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置。</param>
    /// <param name="chatClientFactory">ChatClient 工厂方法。</param>
    /// <returns>服务集合。</returns>
    [RequiresUnreferencedCode("AddLuBanAgent loads external plugins via Assembly.Load and GetTypes, which is incompatible with Native AOT trimming.")]
    public static IServiceCollection AddLuBanAgentDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, IChatClient> chatClientFactory)
    {
        ArgumentNullException.ThrowIfNull(chatClientFactory);

        services.TryAddSingleton(chatClientFactory);
        return services.AddLuBanAgentDefaults(configuration);
    }

    /// <summary>
    /// 添加 LuBan Agent 服务并补齐默认实现（ChatClient 需宿主已注册）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置。</param>
    /// <returns>服务集合。</returns>
    [RequiresUnreferencedCode("AddLuBanAgent loads external plugins via Assembly.Load and GetTypes, which is incompatible with Native AOT trimming.")]
    public static IServiceCollection AddLuBanAgentDefaults(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLuBanAgent(configuration);

        // SubAgentFactory 构造依赖具体类型 LuBanAgentFactory（其 CreateSubAgentAsync 非接口成员），
        // 而 AddLuBanAgent 只注册了接口，仅此会导致 ValidateOnBuild 失败；
        // 此处照 AgentHostBuilder 的做法补齐具体类型，并将接口转发到同一实例。
        services.TryAddScoped<LuBanAgentFactory>();
        services.AddScoped<ILuBanAgentFactory>(sp => sp.GetRequiredService<LuBanAgentFactory>());

        // 会话 / 工作区 / 用户上下文默认实现（Scoped，随会话作用域隔离）
        services.TryAddScoped<DefaultContextState>();
        services.TryAddScoped<ISessionContext, DefaultSessionContext>();
        services.TryAddScoped<IWorkspaceContext, DefaultWorkspaceContext>();
        services.TryAddScoped<IUserContext, DefaultUserContext>();

        // 本地长期记忆存储：内存实现（框架默认不注册，缺失会致 localmemory 工具组构建失败）
        services.TryAddSingleton<ILocalMemoryStore, InMemoryLocalMemoryStore>();

        // 会话管理：内存实现（开启 useSessionHistory 时无则无历史，不致命但开发者需自行实现该 17 成员接口）
        services.TryAddSingleton<ISessionManager, InMemorySessionManager>();

        return services;
    }
}