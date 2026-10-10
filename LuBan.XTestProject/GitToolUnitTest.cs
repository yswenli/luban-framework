/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： GitToolUnitTest
*创建时间：2026/10/9
*描述：验证 Git 工具组执行底座、只读工具、冲突检测与参数防护（每个用例独立仓库）
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
    private string _repo = null!;
    private GitToolGroup _group = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = Path.Combine(Path.GetTempPath(), "luban-git-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_repo);
        _group = CreateGroup(_repo);
    }

    [TestCleanup]
    public void Cleanup()
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
    public async Task GitClone_RejectsOptionLikeDirectory()
    {
        var result = await _group.GitCloneAsync(
            "https://example.com/repo.git", directory: "--upload-pack=xxx", workingDirectory: _repo);

        Assert.IsFalse(result.IsSuccess, "以 '-' 开头的 directory 应被拒绝");
        StringAssert.Contains(result.Message!, "directory");
        Assert.IsNull(result.Data, "守卫应在调用 git 之前返回，不带命令执行载荷");
    }

    [TestMethod]
    public async Task GitReset_InvalidMode_ReturnsFail()
    {
        var result = await _group.GitResetAsync(mode: "frobnicate", workingDirectory: _repo);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "不支持的 reset mode");
    }

    [TestMethod]
    public async Task GitRebase_Interactive_ReturnsFail()
    {
        var result = await _group.GitRebaseAsync("main", interactive: true, workingDirectory: _repo);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "交互式");
    }

    [TestMethod]
    public async Task GitMerge_Conflict_ReturnsConflictList()
    {
        await _group.GitInitAsync(defaultBranch: "main", workingDirectory: _repo);
        var f = Path.Combine(_repo, "conflict.txt");
        File.WriteAllText(f, "base");
        await _group.GitAddAsync(paths: new[] { "conflict.txt" }, workingDirectory: _repo);
        await CommitRaw(_repo, "init");

        await _group.GitCheckoutAsync("side", createBranch: true, workingDirectory: _repo);
        File.WriteAllText(f, "side-change");
        await _group.GitAddAsync(paths: new[] { "conflict.txt" }, workingDirectory: _repo);
        await CommitRaw(_repo, "side");

        await _group.GitCheckoutAsync("main", workingDirectory: _repo);
        File.WriteAllText(f, "main-change");
        await _group.GitAddAsync(paths: new[] { "conflict.txt" }, workingDirectory: _repo);
        await CommitRaw(_repo, "main");

        var merge = await _group.GitMergeAsync("side", workingDirectory: _repo);
        Assert.IsFalse(merge.IsSuccess);
        StringAssert.Contains(merge.Message!, "冲突");
        StringAssert.Contains(merge.Message!, "conflict.txt");
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