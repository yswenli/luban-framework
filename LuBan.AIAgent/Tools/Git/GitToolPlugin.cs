/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.Git
*文件名： GitToolPlugin
*创建时间：2026/10/9
*描述：Git 版本控制工具插件与工具组
*****************************************************************************/
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Tools.Git;

/// <summary>
/// Git 工具插件（GroupName="git"，默认启用）。
/// </summary>
public class GitToolPlugin : ILuBanToolPlugin
{
    private readonly GitToolOptions _options;

    /// <summary>
    /// 创建 GitToolPlugin 实例。
    /// </summary>
    /// <param name="options">Agent 配置。</param>
    public GitToolPlugin(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.Git;
    }

    /// <inheritdoc />
    public string GroupName => "git";

    /// <inheritdoc />
    public string? Description => "Git 版本控制工具，支持状态、差异、提交、分支、合并、变基、暂存等操作";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var opts = toolsOptions?.Git ?? _options;
        var runner = sp.GetRequiredService<ProcessRunner>();
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var agentOptions = sp.GetService<IOptions<LuBanAgentOptions>>();
        Func<string?> workspaceRootProvider = () => agentOptions?.Value.WorkspaceRoot;

        var group = new GitToolGroup(opts, runner, confirmationService, workspaceRootProvider);

        return new List<AIFunction>
        {
            // 只读
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitStatusAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitDiffAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitLogAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitShowAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitBranchListAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitRemoteListAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitBlameAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitStashListAsync)),
            // 写
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitAddAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitCommitAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitCheckoutAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitSwitchAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitBranchCreateAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitCherryPickAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitRevertAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitRestoreAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitMergeAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitStashAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitInitAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitCloneAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitFetchAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitPullAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitPushAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitResetAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitRebaseAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(GitToolGroup.GitCleanAsync)),
        };
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Git.Enabled;
}

/// <summary>
/// Git 工具组：以系统 git 为可执行文件，复用 <see cref="ProcessRunner"/> 执行。
/// 确认语义：以实际仓库工作目录作为确认路径（工作区内免确认、越界逐次确认）。
/// </summary>
public class GitToolGroup
{
    private readonly GitToolOptions _options;
    private readonly ProcessRunner _processRunner;
    private readonly IToolConfirmationService _confirmationService;
    private readonly Func<string?> _workspaceRootProvider;

    /// <summary>
    /// 创建 GitToolGroup 实例。
    /// </summary>
    /// <param name="options">Git 工具配置。</param>
    /// <param name="processRunner">进程执行器。</param>
    /// <param name="confirmationService">工具调用确认服务。</param>
    /// <param name="workspaceRootProvider">工作区根目录提供者（缺省工作目录）。</param>
    public GitToolGroup(
        GitToolOptions options,
        ProcessRunner processRunner,
        IToolConfirmationService confirmationService,
        Func<string?> workspaceRootProvider)
    {
        _options = options;
        _processRunner = processRunner;
        _confirmationService = confirmationService;
        _workspaceRootProvider = workspaceRootProvider;
    }

    // ─────────────── 执行底座 ───────────────

    private string? ResolveWorkingDirectory(string? workingDirectory)
    {
        if (!string.IsNullOrWhiteSpace(workingDirectory)) return workingDirectory;
        var root = _workspaceRootProvider();
        return string.IsNullOrWhiteSpace(root) ? null : root;
    }

    private async Task<ToolResult<string>?> ConfirmOrBlockAsync(
        string toolName, string? workingDirectory, Dictionary<string, object?> arguments)
    {
        var path = ResolveWorkingDirectory(workingDirectory);
        var outcome = await _confirmationService.EvaluateAsync(toolName, path, arguments).ConfigureAwait(false);
        return outcome switch
        {
            EnumConfirmationOutcome.Allowed => null,
            EnumConfirmationOutcome.Planned => ToolResult.Plan<string>(),
            _ => ToolResult.Denied<string>()
        };
    }

    private async Task<ToolResult<string>> RunGitAsync(
        string toolName,
        IReadOnlyList<string> args,
        string? workingDirectory,
        int? timeoutMs,
        bool detectConflict,
        Dictionary<string, object?> arguments)
    {
        if (await ConfirmOrBlockAsync(toolName, workingDirectory, arguments) is { } blocked)
            return blocked;

        var dir = ResolveWorkingDirectory(workingDirectory);
        var timeout = timeoutMs ?? _options.DefaultTimeout;
        var commandLine = "git " + string.Join(" ", args);

        var result = await _processRunner.RunAsync(
            "git", string.Empty, dir,
            stdin: null, timeoutMs: timeout,
            argumentList: args, outputEncoding: Encoding.UTF8);

        if (result.ExitCode == -1 &&
            (result.StandardError.Contains("可执行文件不存在") || result.StandardError.Contains("无法启动")))
        {
            return ToolResult.Fail<string>(
                "Git 工具不可用：未找到 git 可执行文件。请先安装 Git 并确保其在 PATH 中。",
                new { exitCode = -1, stdout = "", stderr = result.StandardError, commandLine, workingDirectory = dir }.ToJson());
        }

        if (detectConflict && result.ExitCode != 0)
        {
            var conflicts = await DetectConflictsAsync(dir, timeout);
            if (conflicts.Count > 0)
            {
                return ToolResult.Fail<string>(
                    "检测到合并冲突，请手动解决以下文件后告知我继续：\n" + string.Join("\n", conflicts),
                    new
                    {
                        exitCode = result.ExitCode,
                        stdout = result.StandardOutput,
                        stderr = result.StandardError,
                        commandLine,
                        workingDirectory = dir,
                        conflicts
                    }.ToJson());
            }
        }

        var payload = new
        {
            exitCode = result.ExitCode,
            stdout = result.StandardOutput,
            stderr = result.StandardError,
            durationMs = result.DurationMs,
            timedOut = result.TimedOut,
            commandLine,
            workingDirectory = dir
        }.ToJson();

        return result.ExitCode == 0
            ? ToolResult.Ok<string>(payload, "exitCode=0")
            : ToolResult.Fail<string>($"git 命令执行失败（exitCode={result.ExitCode}）：{result.StandardError}", payload);
    }

    private async Task<List<string>> DetectConflictsAsync(string? dir, int timeout)
    {
        var conflicts = new List<string>();

        var ls = await _processRunner.RunAsync("git", string.Empty, dir, null, timeout,
            argumentList: new[] { "ls-files", "-u" }, outputEncoding: Encoding.UTF8);
        if (!string.IsNullOrWhiteSpace(ls.StandardOutput))
            conflicts.AddRange(ls.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries));

        var status = await _processRunner.RunAsync("git", string.Empty, dir, null, timeout,
            argumentList: new[] { "status", "--porcelain" }, outputEncoding: Encoding.UTF8);
        var markers = new[] { "UU", "AA", "DD", "AU", "UA", "DU", "UD" };
        foreach (var line in status.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length >= 2 && markers.Contains(line[..2]))
                conflicts.Add(line);
        }

        return conflicts.Distinct().ToList();
    }

    // ─────────────── 只读工具 ───────────────

    /// <summary>显示工作区 git 状态。</summary>
    /// <param name="workingDirectory">仓库工作目录（缺省工作区根）。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("显示 git 工作区状态（git status --porcelain -b）。")]
    public Task<ToolResult<string>> GitStatusAsync(string? workingDirectory = null, int? timeoutMs = null)
        => RunGitAsync(nameof(GitStatusAsync), new[] { "status", "--porcelain=v1", "-b" }, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["workingDirectory"] = workingDirectory });

    /// <summary>显示差异。</summary>
    /// <param name="path">限定路径。</param>
    /// <param name="staged">是否查看已暂存差异。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("显示 git 差异（git diff）。staged=true 查看已暂存差异。")]
    public Task<ToolResult<string>> GitDiffAsync(string? path = null, bool? staged = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "diff" };
        if (staged == true) args.Add("--staged");
        if (!string.IsNullOrWhiteSpace(path)) { args.Add("--"); args.Add(path!); }
        return RunGitAsync(nameof(GitDiffAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["path"] = path, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>显示提交日志。</summary>
    /// <param name="maxCount">最大条数。</param>
    /// <param name="path">限定路径。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("显示 git 提交日志（git log --oneline）。")]
    public Task<ToolResult<string>> GitLogAsync(int? maxCount = null, string? path = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "log", "--oneline", "-n", (maxCount ?? 20).ToString() };
        if (!string.IsNullOrWhiteSpace(path)) { args.Add("--"); args.Add(path!); }
        return RunGitAsync(nameof(GitLogAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["path"] = path, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>显示对象内容。</summary>
    /// <param name="reference">提交/标签/分支引用。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("显示 git 对象内容（git show <reference>）。")]
    public Task<ToolResult<string>> GitShowAsync(string reference, string? workingDirectory = null, int? timeoutMs = null)
        => RunGitAsync(nameof(GitShowAsync), new[] { "show", reference }, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["reference"] = reference, ["workingDirectory"] = workingDirectory });

    /// <summary>列出分支。</summary>
    /// <param name="includeRemote">是否包含远端分支。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("列出 git 分支（git branch）。includeRemote=true 时包含远端分支。")]
    public Task<ToolResult<string>> GitBranchListAsync(bool? includeRemote = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "branch" };
        if (includeRemote == true) args.Add("-a");
        return RunGitAsync(nameof(GitBranchListAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["workingDirectory"] = workingDirectory });
    }

    /// <summary>列出远端。</summary>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("列出 git 远端仓库（git remote -v）。")]
    public Task<ToolResult<string>> GitRemoteListAsync(string? workingDirectory = null, int? timeoutMs = null)
        => RunGitAsync(nameof(GitRemoteListAsync), new[] { "remote", "-v" }, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["workingDirectory"] = workingDirectory });

    /// <summary>追溯文件逐行提交。</summary>
    /// <param name="path">文件路径。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("追溯 git 文件逐行提交信息（git blame -- <path>）。")]
    public Task<ToolResult<string>> GitBlameAsync(string path, string? workingDirectory = null, int? timeoutMs = null)
        => RunGitAsync(nameof(GitBlameAsync), new[] { "blame", "--", path }, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["path"] = path, ["workingDirectory"] = workingDirectory });

    /// <summary>列出暂存。</summary>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("列出 git 暂存列表（git stash list）。")]
    public Task<ToolResult<string>> GitStashListAsync(string? workingDirectory = null, int? timeoutMs = null)
        => RunGitAsync(nameof(GitStashListAsync), new[] { "stash", "list" }, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["workingDirectory"] = workingDirectory });

    // ─────────────── 写工具 ───────────────

    /// <summary>暂存文件。</summary>
    /// <param name="paths">要暂存的路径列表。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git add 暂存指定路径。paths 为路径数组，传 [\".\"] 暂存全部。")]
    public Task<ToolResult<string>> GitAddAsync(string[] paths, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "add", "--" };
        args.AddRange(paths);
        return RunGitAsync(nameof(GitAddAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["paths"] = paths, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>提交。</summary>
    /// <param name="message">提交信息。</param>
    /// <param name="all">是否自动暂存已跟踪文件（-a）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git commit 提交。all=true 时自动暂存已跟踪文件。")]
    public Task<ToolResult<string>> GitCommitAsync(string message, bool? all = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "commit", "-m", message };
        if (all == true) args.Add("--all");
        return RunGitAsync(nameof(GitCommitAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["message"] = message, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>检出。</summary>
    /// <param name="target">目标分支/提交。</param>
    /// <param name="createBranch">是否创建并切换（-b）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git checkout 检出目标。createBranch=true 时创建并切换（-b）。")]
    public Task<ToolResult<string>> GitCheckoutAsync(string target, bool? createBranch = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "checkout" };
        if (createBranch == true) args.Add("-b");
        args.Add(target);
        return RunGitAsync(nameof(GitCheckoutAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["target"] = target, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>切换。</summary>
    /// <param name="target">目标分支。</param>
    /// <param name="createBranch">是否创建并切换（-c）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git switch 切换分支。createBranch=true 时创建并切换（-c）。")]
    public Task<ToolResult<string>> GitSwitchAsync(string target, bool? createBranch = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "switch" };
        if (createBranch == true) args.Add("-c");
        args.Add(target);
        return RunGitAsync(nameof(GitSwitchAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["target"] = target, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>创建分支。</summary>
    /// <param name="name">分支名。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git branch 创建分支（不切换）。")]
    public Task<ToolResult<string>> GitBranchCreateAsync(string name, string? workingDirectory = null, int? timeoutMs = null)
        => RunGitAsync(nameof(GitBranchCreateAsync), new[] { "branch", name }, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["name"] = name, ["workingDirectory"] = workingDirectory });

    /// <summary>拣选提交。</summary>
    /// <param name="commits">提交引用列表。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git cherry-pick 拣选提交。commits 为提交引用数组。")]
    public Task<ToolResult<string>> GitCherryPickAsync(string[] commits, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "cherry-pick" };
        args.AddRange(commits);
        return RunGitAsync(nameof(GitCherryPickAsync), args, workingDirectory, timeoutMs, true,
            new Dictionary<string, object?> { ["commits"] = commits, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>回滚提交。</summary>
    /// <param name="commits">提交引用列表。</param>
    /// <param name="noCommit">是否不自动提交（--no-commit）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git revert 回滚提交。commits 为提交引用数组。")]
    public Task<ToolResult<string>> GitRevertAsync(string[] commits, bool? noCommit = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "revert" };
        if (noCommit == true) args.Add("--no-commit");
        args.AddRange(commits);
        return RunGitAsync(nameof(GitRevertAsync), args, workingDirectory, timeoutMs, true,
            new Dictionary<string, object?> { ["commits"] = commits, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>恢复文件。</summary>
    /// <param name="paths">要恢复的路径列表。</param>
    /// <param name="staged">是否从暂存恢复（--staged）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git restore 恢复文件。staged=true 时从暂存恢复（--staged）。")]
    public Task<ToolResult<string>> GitRestoreAsync(string[] paths, bool? staged = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "restore" };
        if (staged == true) args.Add("--staged");
        args.Add("--");
        args.AddRange(paths);
        return RunGitAsync(nameof(GitRestoreAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["paths"] = paths, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>合并。</summary>
    /// <param name="reference">要合并的引用。</param>
    /// <param name="noCommit">是否不自动提交（--no-commit）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git merge 合并引用。冲突时返回需人工解决的文件清单。")]
    public Task<ToolResult<string>> GitMergeAsync(string reference, bool? noCommit = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "merge" };
        if (noCommit == true) args.Add("--no-commit");
        args.Add(reference);
        return RunGitAsync(nameof(GitMergeAsync), args, workingDirectory, timeoutMs, true,
            new Dictionary<string, object?> { ["reference"] = reference, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>暂存操作。</summary>
    /// <param name="action">动作：push/pop/apply/list/drop。</param>
    /// <param name="message">push 时的说明。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git stash 管理暂存。action 取 push/pop/apply/list/drop，缺省 push。")]
    public Task<ToolResult<string>> GitStashAsync(string? action = null, string? message = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var verb = string.IsNullOrWhiteSpace(action) ? "push" : action!.Trim().ToLowerInvariant();
        var args = new List<string> { "stash", verb };
        if (verb == "push" && !string.IsNullOrWhiteSpace(message)) { args.Add("-m"); args.Add(message!); }
        var detect = verb is "pop" or "apply";
        return RunGitAsync(nameof(GitStashAsync), args, workingDirectory, timeoutMs, detect,
            new Dictionary<string, object?> { ["action"] = action, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>初始化仓库。</summary>
    /// <param name="defaultBranch">初始分支名。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git init 初始化仓库。defaultBranch 指定初始分支名。")]
    public Task<ToolResult<string>> GitInitAsync(string? defaultBranch = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "init" };
        if (!string.IsNullOrWhiteSpace(defaultBranch)) { args.Add("-b"); args.Add(defaultBranch!); }
        return RunGitAsync(nameof(GitInitAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["workingDirectory"] = workingDirectory });
    }

    /// <summary>克隆仓库。</summary>
    /// <param name="url">远端地址。</param>
    /// <param name="directory">目标目录。</param>
    /// <param name="workingDirectory">执行目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git clone 克隆远端仓库。")]
    public Task<ToolResult<string>> GitCloneAsync(string url, string? directory = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "clone", url };
        if (!string.IsNullOrWhiteSpace(directory)) args.Add(directory!);
        return RunGitAsync(nameof(GitCloneAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["url"] = url, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>抓取远端。</summary>
    /// <param name="remote">远端名。</param>
    /// <param name="prune">是否清理已删除的远端分支（--prune）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git fetch 抓取远端更新。")]
    public Task<ToolResult<string>> GitFetchAsync(string? remote = null, bool? prune = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "fetch" };
        if (!string.IsNullOrWhiteSpace(remote)) args.Add(remote!);
        if (prune == true) args.Add("--prune");
        return RunGitAsync(nameof(GitFetchAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["remote"] = remote, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>拉取。</summary>
    /// <param name="remote">远端名。</param>
    /// <param name="branch">分支名。</param>
    /// <param name="rebase">是否使用 rebase（--rebase）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git pull 拉取远端更新。冲突时返回需人工解决的文件清单。")]
    public Task<ToolResult<string>> GitPullAsync(string? remote = null, string? branch = null, bool? rebase = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "pull" };
        if (rebase == true) args.Add("--rebase");
        if (!string.IsNullOrWhiteSpace(remote)) args.Add(remote!);
        if (!string.IsNullOrWhiteSpace(branch)) args.Add(branch!);
        return RunGitAsync(nameof(GitPullAsync), args, workingDirectory, timeoutMs, true,
            new Dictionary<string, object?> { ["remote"] = remote, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>推送。</summary>
    /// <param name="remote">远端名。</param>
    /// <param name="branch">分支名。</param>
    /// <param name="setUpstream">是否设置上游（-u）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git push 推送。setUpstream=true 时设置上游（-u）。")]
    public Task<ToolResult<string>> GitPushAsync(string? remote = null, string? branch = null, bool? setUpstream = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "push" };
        if (setUpstream == true) args.Add("-u");
        if (!string.IsNullOrWhiteSpace(remote)) args.Add(remote!);
        if (!string.IsNullOrWhiteSpace(branch)) args.Add(branch!);
        return RunGitAsync(nameof(GitPushAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["remote"] = remote, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>重置。</summary>
    /// <param name="mode">模式：soft/mixed/hard。</param>
    /// <param name="target">目标引用。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git reset 重置。mode 取 soft/mixed/hard，缺省 mixed。")]
    public Task<ToolResult<string>> GitResetAsync(string? mode = null, string? target = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "reset" };
        if (!string.IsNullOrWhiteSpace(mode)) args.Add("--" + mode!.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(target)) args.Add(target!);
        return RunGitAsync(nameof(GitResetAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["mode"] = mode, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>变基。</summary>
    /// <param name="upstream">上游引用。</param>
    /// <param name="interactive">是否交互式（-i）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git rebase 变基到 upstream。冲突时返回需人工解决的文件清单。注意 -i 交互式在非交互进程下不可用。")]
    public Task<ToolResult<string>> GitRebaseAsync(string upstream, bool? interactive = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "rebase" };
        if (interactive == true) args.Add("-i");
        args.Add(upstream);
        return RunGitAsync(nameof(GitRebaseAsync), args, workingDirectory, timeoutMs, true,
            new Dictionary<string, object?> { ["upstream"] = upstream, ["workingDirectory"] = workingDirectory });
    }

    /// <summary>清理未跟踪文件。</summary>
    /// <param name="dryRun">预演（-n），缺省 true 更安全。</param>
    /// <param name="directories">是否包含目录（-d）。</param>
    /// <param name="force">是否强制（-f）。</param>
    /// <param name="workingDirectory">仓库工作目录。</param>
    /// <param name="timeoutMs">超时毫秒。</param>
    [Description("git clean 清理未跟踪文件。dryRun 缺省 true（仅预演）；真正删除需 dryRun=false 且 force=true。")]
    public Task<ToolResult<string>> GitCleanAsync(bool? dryRun = null, bool? directories = null, bool? force = null, string? workingDirectory = null, int? timeoutMs = null)
    {
        var args = new List<string> { "clean" };
        var isDryRun = dryRun ?? true;
        args.Add(isDryRun ? "-n" : "-f");
        if (directories == true) args.Add("-d");
        if (force == true && isDryRun) args.Add("-f");
        return RunGitAsync(nameof(GitCleanAsync), args, workingDirectory, timeoutMs, false,
            new Dictionary<string, object?> { ["dryRun"] = dryRun, ["workingDirectory"] = workingDirectory });
    }
}