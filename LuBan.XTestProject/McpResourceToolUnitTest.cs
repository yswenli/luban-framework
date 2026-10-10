/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： McpResourceToolUnitTest
*创建时间：2026/10/9
*描述：验证 MCP 资源列表/读取工具的返回语义、确认分支与内容截断
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.MCP;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace LuBan.XTestProject;

[TestClass]
public class McpResourceToolUnitTest
{
    [TestMethod]
    public async Task ListResources_ReturnsResourceUriAndName()
    {
        var client = new FakeMcpClient("demo", new[]
        {
            new MCPResource { Uri = "file:///a.txt", Name = "a.txt", Description = "desc", MimeType = "text/plain" }
        });
        var tool = ListTool(new MCPRegistry(new IMCPClient[] { client }));

        var result = await tool.InvokeAsync(new AIFunctionArguments());

        var (success, message, data) = Parse(result);
        Assert.IsTrue(success, message);
        StringAssert.Contains(data!, "file:///a.txt");
        StringAssert.Contains(data!, "a.txt");
        StringAssert.Contains(data!, "demo");
    }

    [TestMethod]
    public async Task ListResources_NoResources_ReturnsOkFriendlyText()
    {
        var tool = ListTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo") }));

        var result = await tool.InvokeAsync(new AIFunctionArguments());

        var (success, _, data) = Parse(result);
        Assert.IsTrue(success);
        StringAssert.Contains(data!, "没有可用");
    }

    [TestMethod]
    public async Task ListResources_UnknownClientFilter_ReturnsFail()
    {
        var tool = ListTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo") }));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["client"] = "nope" }));

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success);
        StringAssert.Contains(message, "未找到");
    }

    [TestMethod]
    public async Task ListResources_ClientThrows_ReturnsFail()
    {
        var tool = ListTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("bad", throwOnList: true) }));

        var result = await tool.InvokeAsync(new AIFunctionArguments());

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success);
        StringAssert.Contains(message, "调用出错");
    }

    [TestMethod]
    public async Task ReadResource_KnownClientAndUri_ReturnsContent()
    {
        var tool = ReadTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo", content: "hello-resource") }));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["client"] = "demo", ["uri"] = "file:///a.txt" }));

        var (success, message, data) = Parse(result);
        Assert.IsTrue(success, message);
        Assert.AreEqual("hello-resource", data);
    }

    [TestMethod]
    public async Task ReadResource_CaseInsensitiveArguments_Resolve()
    {
        var tool = ReadTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo", content: "ci") }));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["Client"] = "demo", ["URI"] = "file:///a.txt" }));

        var (success, message, data) = Parse(result);
        Assert.IsTrue(success, message);
        Assert.AreEqual("ci", data);
    }

    [TestMethod]
    public async Task ReadResource_UnknownClient_ReturnsFail()
    {
        var tool = ReadTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo", content: "x") }));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["client"] = "nope", ["uri"] = "file:///a.txt" }));

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success);
        StringAssert.Contains(message, "未找到");
    }

    [TestMethod]
    public async Task ReadResource_MissingArguments_ReturnsFail()
    {
        var tool = ReadTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo", content: "x") }));

        var result = await tool.InvokeAsync(new AIFunctionArguments());

        var (success, message, _) = Parse(result);
        Assert.IsFalse(success);
        StringAssert.Contains(message, "参数缺失");
    }

    [TestMethod]
    public async Task ReadResource_LongContent_IsTruncated()
    {
        var tool = ReadTool(new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo", content: new string('x', 20000)) }));

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["client"] = "demo", ["uri"] = "file:///big" }));

        var (success, message, data) = Parse(result);
        Assert.IsTrue(success, message);
        StringAssert.Contains(data!, "[内容已截断]");
        Assert.IsTrue(data!.Length <= 12000 + "\n\n[内容已截断]".Length);
    }

    [TestMethod]
    public async Task ReadResource_Denied_ReturnsDeniedResult()
    {
        var tool = ReadTool(
            new MCPRegistry(new IMCPClient[] { new FakeMcpClient("demo", content: "x") }),
            ConfirmResult.Deny);

        var result = await tool.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?> { ["client"] = "demo", ["uri"] = "file:///a.txt" }));

        Assert.IsInstanceOfType<ToolResult>(result);
        var tr = (ToolResult)result!;
        Assert.IsFalse(tr.IsSuccess);
        Assert.IsTrue(tr.UserCancelled);
    }

    private static MCPToolPlugin.McpListResourcesAIFunction ListTool(MCPRegistry registry, ConfirmResult? confirm = null)
        => new(registry, Confirm(confirm ?? ConfirmResult.Allow));

    private static MCPToolPlugin.McpReadResourceAIFunction ReadTool(MCPRegistry registry, ConfirmResult? confirm = null)
        => new(registry, Confirm(confirm ?? ConfirmResult.Allow));

    private static IToolConfirmationService Confirm(ConfirmResult result)
    {
        var ctx = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(result),
            WorkspacePathChecker = _ => true
        };
        return new ToolConfirmationService(ctx);
    }

    /// <summary>
    /// 统一解析工具返回值：原始 <see cref="ToolResult"/>，或序列化后的 JsonElement（camelCase）。
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

    private sealed class FakeMcpClient : IMCPClient
    {
        private readonly List<MCPResource> _resources;
        private readonly string? _content;
        private readonly bool _throwOnList;

        public FakeMcpClient(
            string name,
            IEnumerable<MCPResource>? resources = null,
            string? content = null,
            bool throwOnList = false)
        {
            Name = name;
            _resources = resources?.ToList() ?? new List<MCPResource>();
            _content = content;
            _throwOnList = throwOnList;
        }

        public string Name { get; }

        public string Description => "fake";

        public bool IsConnected { get; set; } = true;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task DisconnectAsync()
        {
            IsConnected = false;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<MCPTool>>(Array.Empty<MCPTool>());

        public Task<MCPToolResult> CallToolAsync(
            string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default)
            => Task.FromResult(new MCPToolResult { Success = true });

        public Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default)
        {
            if (_throwOnList)
                throw new InvalidOperationException("boom");
            return Task.FromResult<IEnumerable<MCPResource>>(_resources);
        }

        public Task<MCPResourceContent> ReadResourceAsync(string resourceUri, CancellationToken cancellationToken = default)
            => Task.FromResult(new MCPResourceContent { Uri = resourceUri, Content = _content });
    }
}