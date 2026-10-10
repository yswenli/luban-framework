/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Abstractions
*文件名： IWorktreeCoordinator
*创建时间：2026/10/10
*描述：worktree 会话生命周期协调契约（宿主实现）
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions.Models;

namespace LuBan.AIAgent.Abstractions;

/// <summary>worktree 会话生命周期协调（宿主实现）。</summary>
public interface IWorktreeCoordinator
{
    /// <summary>进入 worktree 隔离：创建 worktree 与临时分支。</summary>
    Task<WorktreeSession> BeginAsync(string reason, CancellationToken cancellationToken = default);

    /// <summary>结束并合并回主分支；冲突时返回 Conflict 结果且不清理。</summary>
    Task<WorktreeMergeResult> EndAsync(CancellationToken cancellationToken = default);

    /// <summary>放弃当前 worktree：强制移除并丢弃改动。</summary>
    Task AbandonAsync(CancellationToken cancellationToken = default);

    /// <summary>查询指定会话的活动 worktree。</summary>
    WorktreeSession? GetActive(string sessionId);
}

/// <summary>merge 结果。</summary>
public sealed record WorktreeMergeResult(
    bool Merged,
    bool Conflict,
    string Message,
    IReadOnlyList<string>? ConflictFiles = null);