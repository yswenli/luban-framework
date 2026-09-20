/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： AIAgentUnitTest
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-e5f6-7890-abcd-ef1234567890
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：LuBan.AIAgent 单元测试
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.Tools.FileSystem;
using LuBan.AIAgent.Tools.Web;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class AIAgentUnitTest
{
    /// <summary>
    /// 测试配置选项
    /// </summary>
    [TestMethod]
    public void TestLuBanAgentOptions()
    {
        var options = new LuBanAgentOptions
        {
            DefaultModel = "openai:gpt-4",
            SystemPrompt = "你是一个智能助手",
            Description = "测试 Agent",
            MaxToolLoopIterations = 5
        };

        Assert.AreEqual("openai:gpt-4", options.DefaultModel);
        Assert.AreEqual("你是一个智能助手", options.SystemPrompt);
        Assert.AreEqual("测试 Agent", options.Description);
        Assert.AreEqual(5, options.MaxToolLoopIterations);
    }

    /// <summary>
    /// 测试工具组配置
    /// </summary>
    [TestMethod]
    public void TestToolGroupOptions()
    {
        var options = new ToolGroupOptions
        {
            FileSystem = new FileSystemToolOptions
            {
                Enabled = true,
                AllowedRoots = new List<string> { "C:\\Temp" }
            },
            Browser = new BrowserToolOptions
            {
                Enabled = false
            },
            Web = new WebToolOptions
            {
                Enabled = true,
                MaxCharacters = 10000
            }
        };

        Assert.IsTrue(options.FileSystem.Enabled);
        Assert.IsFalse(options.Browser.Enabled);
        Assert.IsTrue(options.Web.Enabled);
        Assert.AreEqual(10000, options.Web.MaxCharacters);
    }


    /// <summary>
    /// 测试插件注册表
    /// </summary>
    [TestMethod]
    public void TestToolPluginRegistry()
    {
        var services = new ServiceCollection();
        
        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions { Enabled = true },
                Web = new WebToolOptions { Enabled = true },
                Browser = new BrowserToolOptions { Enabled = false },
                Script = new ScriptToolOptions { Enabled = false }
            }
        };

        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ILuBanToolPlugin, FileSystemToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, WebToolPlugin>();
        services.AddSingleton<ProcessRunner>();
        services.AddSingleton<PathGuard>();
        services.AddSingleton<ToolPluginRegistry>();

        var sp = services.BuildServiceProvider();
        var registry = sp.GetRequiredService<ToolPluginRegistry>();
        var plugins = registry.GetEnabledPlugins();

        Assert.AreEqual(2, plugins.Count);
        Assert.IsTrue(plugins.Any(p => p.GroupName == "filesystem"));
        Assert.IsTrue(plugins.Any(p => p.GroupName == "web"));
    }

    /// <summary>
    /// 测试 DI 注册
    /// </summary>
    [TestMethod]
    public void TestDependencyInjection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LuBanAgent:DefaultModel"] = "openai:gpt-4",
                ["LuBanAgent:SystemPrompt"] = "测试提示词",
                ["LuBanAgent:Tools:FileSystem:Enabled"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLuBanAgent(config);

        var sp = services.BuildServiceProvider();

        var factory = sp.GetService<ILuBanAgentFactory>();
        Assert.IsNotNull(factory);

        var registry = sp.GetService<ToolPluginRegistry>();
        Assert.IsNotNull(registry);
    }

    /// <summary>
    /// 内嵌 Lua 沙箱：print 输出与末尾表达式均写入 stdout
    /// </summary>
    [TestMethod]
    public void TestLuaRunner_CapturesPrintAndExpression()
    {
        using var runner = new LuaScriptRunner();

        var result = runner.Execute("print('hello'); print(1 + 2)", 5000);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        StringAssert.Contains(result.StandardOutput, "hello");
        StringAssert.Contains(result.StandardOutput, "3");
        Assert.IsFalse(result.TimedOut);
    }

    /// <summary>
    /// 内嵌 Lua 沙箱：return 表达式作为结果输出
    /// </summary>
    [TestMethod]
    public void TestLuaRunner_ReturnsExpression()
    {
        using var runner = new LuaScriptRunner();

        var result = runner.Execute("return 6 * 7", 5000);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        StringAssert.Contains(result.StandardOutput, "42");
    }

    /// <summary>
    /// 内嵌 Lua 沙箱：脚本运行错误映射为 exitCode=1 与 stderr
    /// </summary>
    [TestMethod]
    public void TestLuaRunner_ScriptError()
    {
        using var runner = new LuaScriptRunner();

        var result = runner.Execute("error('boom')", 5000);

        Assert.AreEqual(1, result.ExitCode);
        StringAssert.Contains(result.StandardError, "boom");
    }

    /// <summary>
    /// 内嵌 Lua 沙箱：无文件系统与系统命令能力（io/os.execute 不可用）
    /// </summary>
    [TestMethod]
    public void TestLuaRunner_SandboxHasNoIoOrSystem()
    {
        using var runner = new LuaScriptRunner();

        var result = runner.Execute("print(type(io)); print(type(os.execute))", 5000);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        StringAssert.Contains(result.StandardOutput, "nil");
        Assert.IsFalse(result.StandardOutput.Contains("function"));
    }

    /// <summary>
    /// 内嵌 Lua 沙箱：内置辅助函数可用
    /// </summary>
    [TestMethod]
    public void TestLuaRunner_BuiltInHelpers()
    {
        using var runner = new LuaScriptRunner();

        var result = runner.Execute("print(md5('abc')); print(base64_encode('hi'))", 5000);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        StringAssert.Contains(result.StandardOutput, "900150983cd24fb0d6963f7d28e17f72");
        StringAssert.Contains(result.StandardOutput, "aGk=");
    }

    /// <summary>
    /// Shell 环境探测：自动探测能解析到可用可执行文件
    /// </summary>
    [TestMethod]
    public void TestShellDetector_AutoDetect_Available()
    {
        var detector = new ShellEnvironmentDetector();

        var environment = detector.Resolve(string.Empty);

        Assert.IsTrue(environment.IsAvailable, environment.Warning);
        Assert.AreNotEqual(ShellKind.Unknown, environment.Kind);
        Assert.IsTrue(File.Exists(environment.Executable), environment.Executable);
    }

    /// <summary>
    /// Shell 命令构造：cmd 使用 /d /s /c 与 UTF-8 前缀，且不破坏内部引号
    /// </summary>
    [TestMethod]
    public void TestShellDetector_BuildCommand_Cmd()
    {
        var environment = new ShellEnvironment(ShellKind.Cmd, "cmd.exe", "cmd", "Windows", true, null);

        var command = ShellEnvironmentDetector.BuildCommand(environment, "echo \"a b\"");

        Assert.IsNull(command.ArgumentList);
        StringAssert.Contains(command.Arguments!, "/d /s /c");
        StringAssert.Contains(command.Arguments!, "chcp 65001");
        StringAssert.Contains(command.Arguments!, "echo \"a b\"");
    }

    /// <summary>
    /// Shell 命令构造：PowerShell 通过 ArgumentList 传 -Command，避免转义问题
    /// </summary>
    [TestMethod]
    public void TestShellDetector_BuildCommand_PowerShell()
    {
        var environment = new ShellEnvironment(ShellKind.PowerShell, "pwsh.exe", "pwsh", "Windows", true, null);

        var command = ShellEnvironmentDetector.BuildCommand(environment, "Get-Date");

        Assert.IsNotNull(command.ArgumentList);
        CollectionAssert.Contains(command.ArgumentList!.ToList(), "-Command");
        Assert.IsTrue(command.ArgumentList!.Any(a => a.Contains("Get-Date")));
    }

    /// <summary>
    /// Shell 环境探测：配置的 shell 不存在时回退自动探测并给出告警
    /// </summary>
    [TestMethod]
    public void TestShellDetector_ConfiguredMissing_FallsBackWithWarning()
    {
        var detector = new ShellEnvironmentDetector();

        var environment = detector.Resolve("definitely-not-a-shell-xyz");

        Assert.IsTrue(environment.IsAvailable, environment.Warning);
        Assert.IsNotNull(environment.Warning);
    }

    /// <summary>
    /// Shell 端到端：自动探测 + 自适应参数构造 + UTF-8 中文输出
    /// </summary>
    [TestMethod]
    public async Task TestShellExecution_EndToEnd_AdaptiveWithUtf8()
    {
        var detector = new ShellEnvironmentDetector();
        var environment = detector.Resolve(string.Empty);
        Assert.IsTrue(environment.IsAvailable, environment.Warning);

        var command = ShellEnvironmentDetector.BuildCommand(environment, "echo luban-中文-ok");
        var runner = new ProcessRunner();
        var result = await runner.RunAsync(
            command.Executable,
            command.Arguments ?? string.Empty,
            workingDir: null,
            stdin: null,
            timeoutMs: 30000,
            cancellationToken: default,
            argumentList: command.ArgumentList,
            outputEncoding: Encoding.UTF8);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        StringAssert.Contains(result.StandardOutput, "luban-中文-ok");
    }
}

/// <summary>
/// 模拟 ChatClient 用于测试
/// </summary>
internal class MockChatClient : IChatClient
{
    public string Name { get; }

    public MockChatClient(string name)
    {
        Name = name;
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, $"Response from {Name}")));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new ChatResponseUpdate(ChatRole.Assistant, $"Streaming from {Name}");
        await Task.CompletedTask;
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        return null;
    }

    public void Dispose()
    {
    }
}