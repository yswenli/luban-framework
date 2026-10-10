/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： GitWorktreeToolUnitTest
*创建时间：2026/10/10
*描述：验证 Git 工具组 worktree 工具委托给 IWorktreeCoordinator
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Abstractions.Models;
using LuBan.AIAgent.Tools.Git;

namespace LuBan.XTestProject;

internal sealed class FakeWorktreeCoordinator : IWorktreeCoordinator
{
    public WorktreeSession? Started;
    public bool MergeConflict;
    public bool Abandoned;

    public Task<WorktreeSession> BeginAsync(string reason, CancellationToken ct = default)
    {
        Started = new WorktreeSession
        {
            SessionId = "s1", WorkspaceId = "w1",
            WorktreePath = @"C:\repo\.worktrees\x", Branch = "luban-worktree/x",
            BaseBranch = "main", BaseCommit = "abc", CreatedAt = default
        };
        return Task.FromResult(Started);
    }

    public Task<WorktreeMergeResult> EndAsync(CancellationToken ct = default)
        => Task.FromResult(MergeConflict
            ? new WorktreeMergeResult(false, true, "conflict", ["a.txt"])
            : new WorktreeMergeResult(true, false, "ok"));

    public Task AbandonAsync(CancellationToken ct = default)
    {
        Abandoned = true;
        return Task.CompletedTask;
    }

    public WorktreeSession? GetActive(string sessionId) => Started;
}

[TestClass]
public class GitWorktreeToolUnitTest
{
    [TestMethod]
    public async Task EnterWorktree_Delegates_To_Coordinator()
    {
        var fake = new FakeWorktreeCoordinator();
        var group = new GitToolGroup(fake);
        var s = await group.EnterWorktreeAsync("build feature");
        Assert.IsNotNull(fake.Started);
        Assert.AreEqual("s1", s.SessionId);
    }

    [TestMethod]
    public async Task FinishWorktree_Delegates_To_Coordinator()
    {
        var fake = new FakeWorktreeCoordinator { MergeConflict = true };
        var group = new GitToolGroup(fake);
        var r = await group.FinishWorktreeAsync();
        Assert.IsTrue(r.Conflict);
    }

    [TestMethod]
    public async Task AbandonWorktree_Delegates_To_Coordinator()
    {
        var fake = new FakeWorktreeCoordinator();
        var group = new GitToolGroup(fake);
        await group.AbandonWorktreeAsync();
        Assert.IsTrue(fake.Abandoned);
    }
}