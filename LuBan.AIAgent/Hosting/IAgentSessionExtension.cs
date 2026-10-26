namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 会话级扩展点：建 Agent 前由宿主逐个调用（插件贡献、连接器启用等）。
/// </summary>
public interface IAgentSessionExtension
{
    /// <summary>在会话 scope 上应用扩展。</summary>
    Task ApplyAsync(IServiceProvider sessionServices, WorkspaceDescriptor workspace, CancellationToken ct = default);
}