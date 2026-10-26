namespace LuBan.AIAgent.Hosting;

/// <summary>会话 scope 初始化数据。</summary>
/// <param name="SessionId">会话标识。</param>
/// <param name="WorkspaceId">工作区标识（可为 null）。</param>
/// <param name="WorkspaceRoot">工作区根目录绝对路径。</param>
/// <param name="AllowedRoots">允许访问的根路径快照。</param>
public sealed record SessionRuntimeSeed(
    string SessionId,
    string? WorkspaceId,
    string WorkspaceRoot,
    IReadOnlyList<string> AllowedRoots);