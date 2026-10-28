/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentHost
*版本号： V1.0.0.0
*唯一标识：85765570-369c-4389-bf32-1805f421f66b
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：Agent 宿主契约：按会话路由流式对话，输出 UI 无关事件
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

/// <summary>
/// Agent 宿主：按会话路由流式对话，输出 UI 无关的 <see cref="AgentStreamEvent"/> 序列。
/// </summary>
/// <remarks>
/// 必须以 <b>Scoped</b> 解析（它用当前 scope 取 <c>IChatClient</c> / 附件编排器等 Scoped 服务）。
/// 每会话的 Agent 实例在其会话 scope 内构建并缓存，宿主自身不持有会话状态。
/// </remarks>
public interface IAgentHost
{
    /// <summary>当前会话的 Agent 是否已就绪。</summary>
    bool IsInitialized { get; }

    /// <summary>当前 scope 的服务提供者（供宿主读取全局配置/工作区等）。</summary>
    IServiceProvider Services { get; }

    /// <summary>当前活动会话标识（无则 null）。</summary>
    string? CurrentSessionId { get; }

    /// <summary>确保指定会话的 Agent 已构建（首次对话前调用）。</summary>
    Task InitializeAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// 重置指定会话的 Agent 缓存；null 表示当前会话。
    /// </summary>
    /// <remarks>
    /// 若该会话正有回合在运行，则只登记延迟重载（不释放正在使用的 Agent），
    /// 待回合收尾时释放、下一次对话按最新配置重建。
    /// </remarks>
    void Reset(string? sessionId = null);

    /// <summary>运行一轮对话。</summary>
    IAsyncEnumerable<AgentStreamEvent> RunStreamingAsync(AgentRunRequest request, CancellationToken ct = default);
}