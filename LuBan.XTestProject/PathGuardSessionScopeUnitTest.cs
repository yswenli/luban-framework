/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PathGuardSessionScopeUnitTest
*创建时间：2026/10/10
*描述：验证 PathGuard 按会话允许根校验，不再依赖进程级全局配置
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Abstractions.Models;
using LuBan.AIAgent.Infrastructure;

namespace LuBan.XTestProject;

internal sealed class FakeSessionContext : ISessionContext
{
    public string? SessionId { get; init; }
    public string EffectiveRoot { get; init; } = @"C:\ws";
    public WorktreeSession? ActiveWorktree => null;
    public IReadOnlyList<string> AllowedRoots { get; init; } = [];
    public string ResolveTempDirectory() => @"C:\ws\.tmp";
    public bool IsWithinEffectiveRoot(string path) => path.StartsWith(@"C:\ws", StringComparison.OrdinalIgnoreCase);
}

[TestClass]
public class PathGuardSessionScopeUnitTest
{
    [TestMethod]
    public void IsAllowed_Uses_SessionAllowedRoots()
    {
        var ctx = new FakeSessionContext { AllowedRoots = [@"C:\ws\extra"] };
        var guard = new PathGuard(ctx, null);
        Assert.IsTrue(guard.IsAllowed(@"C:\ws\extra\a.txt"));
        Assert.IsFalse(guard.IsAllowed(@"C:\other\a.txt"));
    }

    [TestMethod]
    public void IsAllowed_Denies_WhenNoRoots()
    {
        var guard = new PathGuard(new FakeSessionContext(), null);
        Assert.IsFalse(guard.IsAllowed(@"C:\ws\a.txt"));
    }
}