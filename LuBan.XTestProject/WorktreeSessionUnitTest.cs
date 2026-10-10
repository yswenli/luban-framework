/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： WorktreeSessionUnitTest
*创建时间：2026/10/10
*描述：验证 worktree 会话记录为不可变 record，支持 with 派生
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions.Models;

namespace LuBan.XTestProject;

[TestClass]
public class WorktreeSessionUnitTest
{
    [TestMethod]
    public void WorktreeSession_ShouldBe_Immutable_Record()
    {
        var s = new WorktreeSession
        {
            SessionId = "s1",
            WorkspaceId = "w1",
            WorktreePath = @"C:\repo\.worktrees\20261010-120000",
            Branch = "luban-worktree/20261010-120000",
            BaseBranch = "main",
            BaseCommit = "abc123",
            CreatedAt = new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.Zero)
        };
        var s2 = s with { Branch = "other" };
        Assert.AreEqual("luban-worktree/20261010-120000", s.Branch);
        Assert.AreEqual("other", s2.Branch);
    }
}