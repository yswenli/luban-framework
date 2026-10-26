namespace LuBan.AIAgent.Hosting;

/// <summary>按工作区选择 Agent Profile。</summary>
public interface IAgentProfileResolver
{
    /// <summary>选 Profile。</summary>
    IAgentProfile Resolve(WorkspaceDescriptor workspace);
}