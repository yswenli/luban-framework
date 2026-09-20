/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： RequiredArgumentGuardUnitTest
*唯一标识：必填参数守卫与工作区根兜底单测
*创建时间：2026/9/20
*描述：验证模型漏传必填参数时返回中文可自纠结果（而非抛英文 ArgumentException），
*      并验证只读发现类工具保留工作区根兜底、文件类工具不再被静默替换成工作区根
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.MCP.BuiltIn;
using LuBan.AIAgent.Tools;
using LuBan.AIAgent.Tools.FileSystem;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace LuBan.XTestProject;

[TestClass]
public class RequiredArgumentGuardUnitTest
{
    private static string _root = null!;
    private static string _agentFile = null!;
    private static LuBanAgentOptions _options = null!;
    private static FileSystemToolPlugin _plugin = null!;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        _root = Path.Combine(Path.GetTempPath(), "luban-guard-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(_root);
            _agentFile = Path.Combine(_root, "AGENTS.md");
            File.WriteAllText(_agentFile, "# AGENTS.md");

            _options = new LuBanAgentOptions
            {
                WorkspaceRoot = _root,
                Tools = new ToolGroupOptions
                {
                    FileSystem = new FileSystemToolOptions
                    {
                        Enabled = true,
                        AllowedRoots = new List<string> { _root }
                    }
                }
            };

            _plugin = new FileSystemToolPlugin(
                Options.Create(_options), new PathGuard(Options.Create(_options)));
        }
        catch
        {
            TryDeleteRoot();
            throw;
        }
    }

    [ClassCleanup]
    public static void Cleanup() => TryDeleteRoot();

    private static void TryDeleteRoot()
    {
        try
        {
            if (!string.IsNullOrEmpty(_root) && Directory.Exists(_root))
                Directory.Delete(_root, true);
        }
        catch { }
    }

    private static IReadOnlyList<AIFunction> BuildGuardedTools()
    {
        var options = Options.Create(_options);
        var confirmationContext = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(true),
            WorkspacePathChecker = _ => true
        };
        var provider = new StubServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IToolConfirmationService)] = new ToolConfirmationService(confirmationContext, options),
            [typeof(IOptions<LuBanAgentOptions>)] = options
        });

        // 复刻 LuBanAgentFactory.BuildTools：标记了工作区根兜底的只读发现类工具跳过守卫
        return _plugin.GetTools(provider)
            .Select(t => t is not WorkspaceRootFallbackAIFunction
                ? (AIFunction)new RequiredArgumentGuardAIFunction(t)
                : t)
            .ToList();
    }

    private static AIFunction FindTool(IReadOnlyList<AIFunction> tools, string name)
        => tools.Single(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 统一解析工具返回值：装饰器返回的原始 <see cref="ToolResult"/>，或 AIFunctionFactory
    /// 序列化后的 JsonElement（camelCase）。
    /// </summary>
    private static (bool Success, string Message, string? Data) Parse(object? result)
    {
        if (result is ToolResult raw)
            return (raw.IsSuccess, raw.Message ?? "", (raw as ToolResult<string>)?.Data);

        if (result is not JsonElement element)
        {
            element = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(result));
        }

        var success = element.TryGetProperty("isSuccess", out var s) && s.ValueKind == JsonValueKind.True;
        var message = element.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String
            ? m.GetString()!
            : "";
        var data = element.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.String
            ? d.GetString()
            : null;
        return (success, message, data);
    }

    [TestMethod]
    public async Task Guard_MissingRequiredParameter_ReturnsFriendlyResultWithoutThrowing()
    {
        var tool = new RequiredArgumentGuardAIFunction(
            AIFunctionFactory.Create((Func<string, string>)(path => "ok:" + path), "DemoTool"));

        var result = await tool.InvokeAsync(new AIFunctionArguments());

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success);
        StringAssert.Contains(message, "缺少必填参数");
        StringAssert.Contains(message, "path");
    }

    [TestMethod]
    public async Task Guard_WhitespaceValue_TreatedAsMissing()
    {
        var tool = new RequiredArgumentGuardAIFunction(
            AIFunctionFactory.Create((Func<string, string>)(path => "ok:" + path), "DemoTool"));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["path"] = "   " }));

        var (success, _, _) = Parse(result);
        Assert.IsFalse(success);
    }

    [TestMethod]
    public async Task Guard_ProvidedValue_PassesThrough()
    {
        var tool = new RequiredArgumentGuardAIFunction(
            AIFunctionFactory.Create((Func<string, string>)(path => "ok:" + path), "DemoTool"));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["path"] = "abc" }));

        StringAssert.Contains(result!.ToString(), "ok:abc");
    }

    [TestMethod]
    public async Task FileTool_MissingPath_ReturnsMissingParameter_NotWorkspaceRootFallback()
    {
        var tools = BuildGuardedTools();
        var readFile = FindTool(tools, "ReadFile");

        var result = await readFile.InvokeAsync(new AIFunctionArguments());

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success, "缺参时应返回失败结果，避免英文 ArgumentException");
        StringAssert.Contains(message, "缺少必填参数");
        StringAssert.Contains(message, "path");
        Assert.IsFalse(message.Contains("路径是目录"),
            "不得再把缺参静默替换成工作区根（旧行为会产生“路径是目录”误导）");
    }

    [TestMethod]
    public async Task FileTool_ProvidedPath_StillWorks()
    {
        var tools = BuildGuardedTools();
        var readFile = FindTool(tools, "ReadFile");

        var result = await readFile.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["path"] = _agentFile }));

        var (success, message, data) = Parse(result);
        Assert.IsTrue(success, message);
        StringAssert.Contains(data, "AGENTS.md");
    }

    [TestMethod]
    public async Task DiscoveryTool_MissingPath_StillFallsBackToWorkspaceRoot()
    {
        var tools = BuildGuardedTools();
        var listDirectory = FindTool(tools, "ListDirectory");

        Assert.IsInstanceOfType(listDirectory, typeof(WorkspaceRootFallbackAIFunction),
            "只读发现类工具应保留工作区根兜底标记");

        var result = await listDirectory.InvokeAsync(new AIFunctionArguments());

        var (success, message, data) = Parse(result);
        Assert.IsTrue(success, message);
        StringAssert.Contains(data, "AGENTS.md");
    }

    [TestMethod]
    public async Task ScriptTool_MissingCommand_ReturnsFriendlyResult()
    {
        // 复现原始故障：RunShell 的 command 漏传时应返回中文提示，而非
        // "The arguments dictionary is missing a value for the required parameter 'command'"
        var tool = new RequiredArgumentGuardAIFunction(
            AIFunctionFactory.Create((Func<string, string?>)(command => "ran:" + command), "RunShell"));

        var result = await tool.InvokeAsync(new AIFunctionArguments());

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success);
        StringAssert.Contains(message, "command");
    }

    private sealed class StubServiceProvider(Dictionary<Type, object> map) : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => map.TryGetValue(serviceType, out var value) ? value : null;
    }
}