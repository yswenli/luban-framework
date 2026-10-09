/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A
*文件名： A2AExtensions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 框架侧注册扩展，仅注册抽象默认实现（默认抛 NotSupportedException）+ 绑定配置
*
*****************************************************************************/
using LuBan.AIAgent.A2A.Abstractions;
using LuBan.AIAgent.A2A.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LuBan.AIAgent.A2A;

/// <summary>
/// A2A 框架侧注册扩展。
/// <para>
/// <b>注意</b>：框架 <c>LuBan.AIAgent</c> 只提供 A2A 抽象与原生 DTO，不引用 A2A SDK。
/// 本方法注册的均为「默认实现」，所有成员统一抛 <see cref="NotSupportedException"/>（非静默 no-op）。
/// 真实实现由 agent 侧 <c>LubanAgentA2A</c> 的 <c>AddLubanAgentA2A()</c> 覆盖。
/// </para>
/// </summary>
public static class A2AExtensions
{
    /// <summary>
    /// 注册 A2A 框架侧抽象默认实现并绑定 <see cref="A2AOptions"/>。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置源。</param>
    /// <returns>服务集合，便于链式调用。</returns>
    public static IServiceCollection AddLubanA2A(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<A2AOptions>(configuration.GetSection(A2AOptions.SectionName));

        services.TryAddSingleton<IA2AAgentRegistry, NotImplementedA2AAgentRegistry>();
        services.TryAddSingleton<IA2ATransport, NotImplementedA2ATransport>();
        services.TryAddSingleton<IA2AServerHost, NotImplementedA2AServerHost>();
        services.TryAddSingleton<IA2AClient, NotImplementedA2AClient>();

        return services;
    }

    /// <summary>构造统一的「未提供实现」异常。</summary>
    internal static NotSupportedException NotProvided(string member)
        => new($"A2A 功能未启用或未安装实现：{member}。" +
               "请在 agent 侧引用 LubanAgentA2A 并调用 AddLubanAgentA2A()，或在配置中启用 LuBanAgent:A2A。");
}

/// <summary>默认 Agent 注册表实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2AAgentRegistry : IA2AAgentRegistry
{
    public void Register(A2AAgentDescriptor descriptor, IA2AHandler handler)
        => throw A2AExtensions.NotProvided(nameof(IA2AAgentRegistry));

    public bool TryResolve(string name, out A2AAgentDescriptor descriptor, out IA2AHandler handler)
        => throw A2AExtensions.NotProvided(nameof(IA2AAgentRegistry));

    public IReadOnlyCollection<A2AAgentDescriptor> GetAll()
        => throw A2AExtensions.NotProvided(nameof(IA2AAgentRegistry));
}

/// <summary>默认传输实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2ATransport : IA2ATransport
{
    public Task<A2ATask> SendAsync(string agentName, A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    public IAsyncEnumerable<A2AStreamEvent> SendStreamingAsync(string agentName, A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    public Task<A2ATask?> GetTaskAsync(string agentName, string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    public Task<IReadOnlyList<A2ATask>> ListTasksAsync(string agentName, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    public Task<A2ATask?> CancelAsync(string agentName, string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    public IAsyncEnumerable<A2AStreamEvent> SubscribeAsync(string agentName, string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));
}

/// <summary>默认服务端宿主实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2AServerHost : IA2AServerHost
{
    public bool IsRunning => false;

    public string? Endpoint => null;

    public IReadOnlyCollection<A2AAgentDescriptor> GetDescriptors()
        => throw A2AExtensions.NotProvided(nameof(IA2AServerHost));

    public Task StartAsync(CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AServerHost));

    public Task StopAsync(CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AServerHost));
}

/// <summary>默认远端客户端实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2AClient : IA2AClient
{
    public string Name => "";

    public string Endpoint => "";

    public Task<A2AAgentDescriptor?> ResolveCardAsync(CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    public Task<A2ATask> SendAsync(A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    public IAsyncEnumerable<A2AStreamEvent> SendStreamingAsync(A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    public Task<A2ATask?> GetTaskAsync(string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    public Task<A2ATask?> CancelAsync(string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));
}