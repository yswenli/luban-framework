/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： ToolCancellationUnitTest
*版本号： V1.0.0.0
*描述：工具组取消能力回归测试（Git / FileSystem / Browser / MCP）
*
*****************************************************************************/
using System.Reflection;
using System.Text.Json;

using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.MCP.BuiltIn;
using LuBan.AIAgent.Tools.Browser;
using LuBan.AIAgent.Tools.FileSystem;
using LuBan.AIAgent.Tools.Git;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

/// <summary>
/// 工具组取消能力回归测试：用户按 Esc 中止回合时，工具必须抛
/// <see cref="OperationCanceledException"/> 而不是被 <c>catch (Exception)</c>
/// 吞成「执行失败」结果（否则模型会把取消当工具错误继续跑，回合无法收尾）。
/// </summary>
[TestClass]
public class ToolCancellationUnitTest
{
    private static IToolConfirmationService BypassPermissions() => new ToolConfirmationService(
        new ToolConfirmationContext { Mode = ToolPermissionMode.BypassPermissions });

    private static CancellationToken CancelledToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        return cts.Token;
    }

    /// <summary>
    /// Git 工具：预取消令牌必须在启动 git 进程前生效，且不能被转成失败结果。
    /// </summary>
    [TestMethod]
    public async Task TestGitTool_PreCancelled_ThrowsCancellation()
    {
        var root = Path.GetTempPath();
        var group = new GitToolGroup(new GitToolOptions(), new ProcessRunner(), BypassPermissions(), () => root);

        OperationCanceledException? caught = null;
        try
        {
            await group.GitStatusAsync(root, null, CancelledToken());
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "Git 工具吞掉了取消：应抛 OperationCanceledException，而不是返回 ToolResult.Fail");
    }

    /// <summary>
    /// 文件系统工具：预取消令牌必须在任何文件操作前生效。
    /// </summary>
    [TestMethod]
    public async Task TestFileSystemTool_PreCancelled_ThrowsCancellation()
    {
        var group = new FileSystemToolGroup(BypassPermissions());

        OperationCanceledException? caught = null;
        try
        {
            await group.ReadFileAsync(Path.Combine(Path.GetTempPath(), "luban-cancel-probe.txt"), CancelledToken());
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "FileSystem 工具吞掉了取消：应抛 OperationCanceledException");
    }

    /// <summary>
    /// 文件系统工具：等待确认期间被取消时，不得再启动重耗时扫描。
    /// </summary>
    [TestMethod]
    public async Task TestFileSystemTool_CancelledWhileConfirming_ThrowsCancellation()
    {
        using var cts = new CancellationTokenSource();
        var group = new FileSystemToolGroup(new CancelOnEvaluateConfirmationService(BypassPermissions(), cts));

        OperationCanceledException? caught = null;
        try
        {
            await group.SearchFilesAsync(Path.GetTempPath(), "*.luban-cancel-probe", 10, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "确认返回后没有再次检查取消：扫描仍会继续执行");
    }

    /// <summary>
    /// 文件系统工具：目录遍历中途取消必须立刻中断（逐项检查），而不是跑完整个树。
    /// </summary>
    [TestMethod]
    public void TestFileSystemTool_CancelDuringEnumeration_ThrowsCancellation()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-cancel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            for (var i = 0; i < 3; i++)
                File.WriteAllText(Path.Combine(root, $"probe-{i}.txt"), "x");

            var method = typeof(FileSystemToolGroup).GetMethod(
                "EnumerateFilesSafe", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "未找到 EnumerateFilesSafe，测试需同步更新");

            using var cts = new CancellationTokenSource();
            var enumerable = (IEnumerable<string>)method.Invoke(null, new object?[] { root, cts.Token })!;
            using var enumerator = enumerable.GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext(), "根目录应至少枚举出一个文件");

            cts.Cancel();

            OperationCanceledException? caught = null;
            try
            {
                enumerator.MoveNext();
            }
            catch (OperationCanceledException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "目录遍历中途取消未中断：逐项检查失效");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { }
        }
    }

    /// <summary>
    /// 内置 FileSystem MCP 客户端：取消必须直通，不能被包装成失败结果。
    /// </summary>
    [TestMethod]
    public async Task TestFileSystemMCPClient_PreCancelled_ThrowsCancellation()
    {
        var client = new FileSystemMCPClient(new[] { Path.GetTempPath() });

        OperationCanceledException? caught = null;
        try
        {
            await client.CallToolAsync("list_directory",
                new Dictionary<string, object?> { ["path"] = Path.GetTempPath() }, CancelledToken());
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "FileSystemMCPClient 把取消吞成了失败结果");
    }

    /// <summary>
    /// HTTP MCP 客户端：取消必须直通，不能被包装成失败结果。
    /// </summary>
    [TestMethod]
    public async Task TestHttpMCPClient_PreCancelled_ThrowsCancellation()
    {
        var config = new McpServerConfig
        {
            Name = "http-cancel-probe",
            Transport = "http",
            Url = "http://127.0.0.1:9/mcp"
        };
        using var client = new HttpMCPClient(config);

        OperationCanceledException? caught = null;
        try
        {
            await client.CallToolAsync("probe", new Dictionary<string, object?>(), CancelledToken());
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "HttpMCPClient 把取消吞成了失败结果");
    }

    /// <summary>
    /// 浏览器工具：Playwright 公开 API 不支持取消，但预取消令牌必须在不启动浏览器的情况下生效。
    /// </summary>
    [TestMethod]
    public async Task TestBrowserTool_PreCancelled_ThrowsWithoutStartingBrowser()
    {
        var session = new PlaywrightSession(Options.Create(new LuBanAgentOptions()));
        var group = new BrowserToolGroup(session, BypassPermissions());

        OperationCanceledException? caught = null;
        try
        {
            await group.NavigateAsync("https://example.com", CancelledToken());
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "浏览器工具吞掉了取消：应抛 OperationCanceledException");
    }

    /// <summary>
    /// Git 工具：确认期间被取消（用户按 Esc 而不是点按钮）时，不得再启动 git 进程。
    /// </summary>
    [TestMethod]
    public async Task TestGitTool_CancelledWhileConfirming_ThrowsCancellation()
    {
        using var cts = new CancellationTokenSource();
        var group = new GitToolGroup(new GitToolOptions(), new ProcessRunner(),
            new CancelOnEvaluateConfirmationService(BypassPermissions(), cts), () => Path.GetTempPath());

        OperationCanceledException? caught = null;
        try
        {
            await group.GitStatusAsync(Path.GetTempPath(), null, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "确认返回后没有再次检查取消：git 子进程仍会启动");
    }

    /// <summary>
    /// 工具 JSON Schema 不得把 <c>cancellationToken</c> 暴露给模型（模型只应看到业务参数）。
    /// </summary>
    [TestMethod]
    public void TestToolSchema_DoesNotExposeCancellationToken()
    {
        var git = new GitToolGroup(new GitToolOptions(), new ProcessRunner(),
            BypassPermissions(), () => Path.GetTempPath());
        var fs = new FileSystemToolGroup(BypassPermissions());

        var functions = new (AIFunction Function, string Label)[]
        {
            (AIFunctionFactoryHelper.Create(git, nameof(GitToolGroup.GitStatusAsync), name: "git_status"), "git_status"),
            (AIFunctionFactoryHelper.Create(git, nameof(GitToolGroup.GitPushAsync), name: "git_push"), "git_push"),
            (AIFunctionFactoryHelper.Create(fs, nameof(FileSystemToolGroup.SearchFilesAsync), name: "search_files"), "search_files"),
            (AIFunctionFactoryHelper.Create(fs, nameof(FileSystemToolGroup.GrepAsync), name: "grep"), "grep"),
            (AIFunctionFactoryHelper.Create(fs, nameof(FileSystemToolGroup.GetWorkspaceOverviewAsync),
                workspaceRootProvider: () => Path.GetTempPath(), name: "workspace_overview"), "workspace_overview")
        };

        foreach (var (function, label) in functions)
        {
            using var document = JsonDocument.Parse(function.JsonSchema.GetRawText());
            var properties = document.RootElement.GetProperty("properties")
                .EnumerateObject().Select(p => p.Name).ToList();

            Assert.IsFalse(
                properties.Any(p => string.Equals(p, "cancellationToken", StringComparison.OrdinalIgnoreCase)),
                $"{label} 的 JSON Schema 暴露了 cancellationToken：{string.Join(", ", properties)}");
        }
    }

    /// <summary>
    /// 取消链路端到端：宿主 <c>InvokeAsync(arguments, ct)</c> 传入的令牌必须穿透 AIFunction
    /// 注入到工具方法（这里是 Git），而不是被丢弃。
    /// </summary>
    [TestMethod]
    public async Task TestTool_CancellationFlowsThroughAIFunction()
    {
        var group = new GitToolGroup(new GitToolOptions(), new ProcessRunner(),
            BypassPermissions(), () => Path.GetTempPath());
        var function = AIFunctionFactoryHelper.Create(
            group, nameof(GitToolGroup.GitStatusAsync), name: "git_status");

        var arguments = new AIFunctionArguments { ["workingDirectory"] = Path.GetTempPath() };

        OperationCanceledException? caught = null;
        try
        {
            await function.InvokeAsync(arguments, CancelledToken());
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        Assert.IsNotNull(caught, "取消令牌未穿透 AIFunction：git 工具仍会启动子进程");
    }

    /// <summary>
    /// 评估确认时立刻取消令牌，模拟「确认弹窗期间用户按 Esc」。
    /// </summary>
    private sealed class CancelOnEvaluateConfirmationService : IToolConfirmationService
    {
        private readonly IToolConfirmationService _inner;
        private readonly CancellationTokenSource _cts;

        public CancelOnEvaluateConfirmationService(IToolConfirmationService inner, CancellationTokenSource cts)
        {
            _inner = inner;
            _cts = cts;
        }

        public Task<EnumConfirmationOutcome> EvaluateAsync(
            string toolName, string? path, IReadOnlyDictionary<string, object?> arguments)
        {
            _cts.Cancel();
            return _inner.EvaluateAsync(toolName, path, arguments);
        }

        public Task<bool> RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
            => _inner.RequestConfirmation(toolName, arguments);

        public Task<bool> TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments)
            => _inner.TryConfirmByPath(toolName, path, arguments);

        public HashSet<string> AutoConfirmTools
        {
            get => _inner.AutoConfirmTools;
            set => _inner.AutoConfirmTools = value;
        }

        public HashSet<string> AlwaysConfirmTools
        {
            get => _inner.AlwaysConfirmTools;
            set => _inner.AlwaysConfirmTools = value;
        }

        public HashSet<string> ReadOnlyTools
        {
            get => _inner.ReadOnlyTools;
            set => _inner.ReadOnlyTools = value;
        }

        public string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200)
            => _inner.FormatArguments(arguments, maxLength);
    }
}