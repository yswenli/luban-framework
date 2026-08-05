namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 当前工作区上下文提供者（供 LocalMemory 按工作区隔离记忆）
/// </summary>
public interface IWorkspaceContextProvider
{
    /// <summary>
    /// 当前工作区 ID，无工作区时为 null
    /// </summary>
    string? CurrentWorkspaceId { get; }
}
