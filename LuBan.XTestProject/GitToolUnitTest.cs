/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： GitToolUnitTest
*创建时间：2026/10/9
*描述：验证 Git 工具组执行底座、只读工具与冲突检测
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.Tools.Git;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class GitToolUnitTest
{
    private static string _repo = null!;
    private static GitToolGroup _group = null!;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        _repo = Path.Combine(Path.GetTempPath(), "luban-git-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_repo);

        var opts = Options.Create(new LuBanAgentOptions
        {
            WorkspaceRoot = _repo,
            Tools = new ToolGroupOptions { Git = new GitToolOptions { Enabled = true, DefaultTimeout = 60000 } }
        });
        var ctx = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(ConfirmResult.Allow),
            WorkspacePathChecker = _ => true
        };
        _group = new GitToolGroup(
            opts.Value.Tools.Git,
            new ProcessRunner(),
            new ToolConfirmationService(ctx, opts),
            () => _repo);
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        try { if (Directory.Exists(_repo)) Directory.Delete(_repo, true); } catch { }
    }

    [TestMethod]
    public async Task GitInit_Then_Status_Works()
    {
        var init = await _group.GitInitAsync(workingDirectory: _repo);
        Assert.IsTrue(init.IsSuccess, init.Message);

        File.WriteAllText(Path.Combine(_repo, "f.txt"), "hi");
        var status = await _group.GitStatusAsync(workingDirectory: _repo);
        Assert.IsTrue(status.IsSuccess);
        StringAssert.Contains(status.Data!, "f.txt");
    }

    [TestMethod]
    public async Task GitCommit_RequiresAuthor_ReturnsActionableFailureOrSuccess()
    {
        await _group.GitInitAsync(workingDirectory: _repo);
        File.WriteAllText(Path.Combine(_repo, "g.txt"), "hi");
        await _group.GitAddAsync(paths: new[] { "g.txt" }, workingDirectory: _repo);

        var result = await _group.GitCommitAsync("test commit", workingDirectory: _repo);

        // 未配置 user.name/email 时 commit 会失败，工具应返回失败态（非异常）
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Message);
    }

    [TestMethod]
    public void DefaultReadOnlyTools_ContainsGitReadOnlyTools()
    {
        var ctx = new ToolConfirmationContext();
        var svc = new ToolConfirmationService(ctx);
        Assert.IsTrue(svc.ReadOnlyTools.Contains("GitStatusAsync"));
        Assert.IsTrue(svc.ReadOnlyTools.Contains("GitDiffAsync"));
        Assert.IsFalse(svc.AlwaysConfirmTools.Contains("GitCommitAsync"), "Git 工具不得进入 AlwaysConfirmTools");
    }

    [TestMethod]
    public async Task GitClean_DefaultDryRun_DoesNotDelete_And_RequiresForce()
    {
        await _group.GitInitAsync(workingDirectory: _repo);
        var junk = Path.Combine(_repo, "junk_" + Guid.NewGuid().ToString("N") + ".tmp");
        File.WriteAllText(junk, "x");

        var dry = await _group.GitCleanAsync(workingDirectory: _repo);
        Assert.IsTrue(dry.IsSuccess);
        Assert.IsTrue(File.Exists(junk), "默认预演不应删除文件");

        var guard = await _group.GitCleanAsync(dryRun: false, force: false, workingDirectory: _repo);
        Assert.IsFalse(guard.IsSuccess);
        Assert.IsTrue(File.Exists(junk));

        var real = await _group.GitCleanAsync(dryRun: false, force: true, workingDirectory: _repo);
        Assert.IsTrue(real.IsSuccess);
        Assert.IsFalse(File.Exists(junk));
    }

    [TestMethod]
    public async Task GitShow_RejectsOptionLikeReference()
    {
        await _group.GitInitAsync(workingDirectory: _repo);
        var result = await _group.GitShowAsync("-s", workingDirectory: _repo);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GitMerge_Conflict_ReturnsConflictList()
    {
        var repo = Path.Combine(Path.GetTempPath(), "luban-git-merge-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repo);
        try
        {
            var group = CreateGroup(repo);
            await group.GitInitAsync(defaultBranch: "main", workingDirectory: repo);
            var f = Path.Combine(repo, "conflict.txt");
            File.WriteAllText(f, "base");
            await group.GitAddAsync(paths: new[] { "conflict.txt" }, workingDirectory: repo);
            await CommitRaw(repo, "init");

            await group.GitCheckoutAsync("side", createBranch: true, workingDirectory: repo);
            File.WriteAllText(f, "side-change");
            await group.GitAddAsync(paths: new[] { "conflict.txt" }, workingDirectory: repo);
            await CommitRaw(repo, "side");

            await group.GitCheckoutAsync("main", workingDirectory: repo);
            File.WriteAllText(f, "main-change");
            await group.GitAddAsync(paths: new[] { "conflict.txt" }, workingDirectory: repo);
            await CommitRaw(repo, "main");

            var merge = await group.GitMergeAsync("side", workingDirectory: repo);
            Assert.IsFalse(merge.IsSuccess);
            StringAssert.Contains(merge.Message!, "冲突");
            StringAssert.Contains(merge.Message!, "conflict.txt");
        }
        finally
        {
            try { if (Directory.Exists(repo)) Directory.Delete(repo, true); } catch { }
        }
    }

    private static GitToolGroup CreateGroup(string repo)
    {
        var opts = Options.Create(new LuBanAgentOptions
        {
            WorkspaceRoot = repo,
            Tools = new ToolGroupOptions { Git = new GitToolOptions { Enabled = true, DefaultTimeout = 60000 } }
        });
        var ctx = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(ConfirmResult.Allow),
            WorkspacePathChecker = _ => true
        };
        return new GitToolGroup(
            opts.Value.Tools.Git,
            new ProcessRunner(),
            new ToolConfirmationService(ctx, opts),
            () => repo);
    }

    private static async Task CommitRaw(string repo, string message)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("git", $"commit -m \"{message}\"")
        {
            WorkingDirectory = repo,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        psi.Environment["GIT_AUTHOR_NAME"] = "t";
        psi.Environment["GIT_AUTHOR_EMAIL"] = "t@t";
        psi.Environment["GIT_COMMITTER_NAME"] = "t";
        psi.Environment["GIT_COMMITTER_EMAIL"] = "t@t";
        using var p = System.Diagnostics.Process.Start(psi)!;
        await p.WaitForExitAsync();
    }
}
