namespace LuBan.AIAgent.Hosting;

/// <summary>解析「某会话属于哪个工作区」。</summary>
public interface IAgentWorkspaceResolver
{
    /// <summary>解析会话绑定的工作区；实现应保证返回非 null（无工作区时回退默认）。</summary>
    Task<WorkspaceDescriptor> ResolveAsync(string sessionId, CancellationToken ct = default);
}