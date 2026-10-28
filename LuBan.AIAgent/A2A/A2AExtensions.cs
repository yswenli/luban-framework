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
    /// <summary>
    /// 登记智能体描述及其处理器；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="descriptor">智能体描述信息。</param>
    /// <param name="handler">处理该智能体任务的处理器。</param>
    public void Register(A2AAgentDescriptor descriptor, IA2AHandler handler)
        => throw A2AExtensions.NotProvided(nameof(IA2AAgentRegistry));

    /// <summary>
    /// 按名称解析已登记智能体；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="name">待查找的智能体名称。</param>
    /// <param name="descriptor">解析成功时的智能体描述。</param>
    /// <param name="handler">解析成功时的任务处理器。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public bool TryResolve(string name, out A2AAgentDescriptor descriptor, out IA2AHandler handler)
        => throw A2AExtensions.NotProvided(nameof(IA2AAgentRegistry));

    /// <summary>
    /// 枚举全部已登记智能体；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public IReadOnlyCollection<A2AAgentDescriptor> GetAll()
        => throw A2AExtensions.NotProvided(nameof(IA2AAgentRegistry));
}

/// <summary>默认传输实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2ATransport : IA2ATransport
{
    /// <summary>
    /// 向指定智能体发送消息并等待任务完成；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="agentName">目标智能体名称。</param>
    /// <param name="message">待发送的消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2ATask> SendAsync(string agentName, A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    /// <summary>
    /// 以流式方式向指定智能体发送消息；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="agentName">目标智能体名称。</param>
    /// <param name="message">待发送的消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回事件序列，调用即抛出未实现异常。</returns>
    public IAsyncEnumerable<A2AStreamEvent> SendStreamingAsync(string agentName, A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    /// <summary>
    /// 查询指定智能体上的任务状态；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="agentName">目标智能体名称。</param>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2ATask?> GetTaskAsync(string agentName, string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    /// <summary>
    /// 列出指定智能体上的任务；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="agentName">目标智能体名称。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<IReadOnlyList<A2ATask>> ListTasksAsync(string agentName, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    /// <summary>
    /// 请求取消指定任务；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="agentName">目标智能体名称。</param>
    /// <param name="taskId">待取消的任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2ATask?> CancelAsync(string agentName, string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));

    /// <summary>
    /// 订阅指定任务的后续事件流；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="agentName">目标智能体名称。</param>
    /// <param name="taskId">要订阅的任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回事件序列，调用即抛出未实现异常。</returns>
    public IAsyncEnumerable<A2AStreamEvent> SubscribeAsync(string agentName, string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2ATransport));
}

/// <summary>默认服务端宿主实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2AServerHost : IA2AServerHost
{
    /// <summary>
    /// 服务端宿主是否正在运行；此默认实现恒为 false。
    /// </summary>
    public bool IsRunning => false;

    /// <summary>
    /// 服务端对外暴露的监听地址；此默认实现恒为 null。
    /// </summary>
    public string? Endpoint => null;

    /// <summary>
    /// 取得服务端对外发布的智能体描述；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public IReadOnlyCollection<A2AAgentDescriptor> GetDescriptors()
        => throw A2AExtensions.NotProvided(nameof(IA2AServerHost));

    /// <summary>
    /// 启动 A2A 服务端宿主；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public Task StartAsync(CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AServerHost));

    /// <summary>
    /// 停止 A2A 服务端宿主；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public Task StopAsync(CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AServerHost));
}

/// <summary>默认远端客户端实现（未提供真实实现）。</summary>
internal sealed class NotImplementedA2AClient : IA2AClient
{
    /// <summary>
    /// 远端客户端标识名称；此默认实现返回空字符串。
    /// </summary>
    public string Name => "";

    /// <summary>
    /// 远端客户端的服务地址；此默认实现返回空字符串。
    /// </summary>
    public string Endpoint => "";

    /// <summary>
    /// 拉取远端智能体卡片；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2AAgentDescriptor?> ResolveCardAsync(CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    /// <summary>
    /// 向远端智能体发送消息并等待任务完成；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="message">待发送的消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2ATask> SendAsync(A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    /// <summary>
    /// 以流式方式向远端智能体发送消息；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="message">待发送的消息。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回事件序列，调用即抛出未实现异常。</returns>
    public IAsyncEnumerable<A2AStreamEvent> SendStreamingAsync(A2AMessage message, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    /// <summary>
    /// 查询远端任务状态；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="taskId">任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2ATask?> GetTaskAsync(string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));

    /// <summary>
    /// 请求取消远端任务；此默认实现无条件抛出未实现异常。
    /// </summary>
    /// <param name="taskId">待取消的任务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>不返回结果，调用即抛出未实现异常。</returns>
    public Task<A2ATask?> CancelAsync(string taskId, CancellationToken ct = default)
        => throw A2AExtensions.NotProvided(nameof(IA2AClient));
}