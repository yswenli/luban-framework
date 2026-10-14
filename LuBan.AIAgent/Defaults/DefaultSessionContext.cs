/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Defaults
*文件名： DefaultSessionContext
*创建时间：2026/10/14
*描述：默认会话级上下文（Scoped 只读视图），状态来自 DefaultContextState
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions.Models;

namespace LuBan.AIAgent.Defaults;

/// <summary>
/// 默认会话上下文（Scoped 只读视图）。
/// <para>
/// 有效根解析顺序：活动 worktree → <see cref="DefaultContextState.WorkspaceRoot"/> →
/// <c>LuBanAgent:WorkspaceRoot</c> 配置 → 进程当前目录。<b>永不抛异常</b>，
/// 因此不注册任何宿主上下文的容器也能正常构建文件/Git 工具组。
/// </para>
/// </summary>
public sealed class DefaultSessionContext(DefaultContextState state, IOptions<LuBanAgentOptions> options)
    : ISessionContext
{
    /// <inheritdoc />
    public string? SessionId => state.SessionId;

    /// <inheritdoc />
    public string EffectiveRoot
        => state.ActiveWorktree?.WorktreePath
           ?? NonEmpty(state.WorkspaceRoot)
           ?? NonEmpty(options.Value.WorkspaceRoot)
           ?? Directory.GetCurrentDirectory();

    /// <inheritdoc />
    public WorktreeSession? ActiveWorktree => state.ActiveWorktree;

    /// <inheritdoc />
    public IReadOnlyList<string> AllowedRoots => state.AllowedRoots;

    /// <inheritdoc />
    public string ResolveTempDirectory()
        => state.TempDirectory ?? Path.Combine(EffectiveRoot, ".luban-agent", "temp");

    /// <inheritdoc />
    public bool IsWithinEffectiveRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        string full;
        try
        {
            full = Path.GetFullPath(path);
        }
        catch
        {
            return false;
        }

        if (full.StartsWith(Path.GetFullPath(EffectiveRoot), StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var root in state.AllowedRoots)
        {
            if (string.IsNullOrWhiteSpace(root)) continue;
            if (full.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string? NonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}