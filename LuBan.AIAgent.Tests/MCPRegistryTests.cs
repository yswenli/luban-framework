using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class MCPRegistryTests
{
    private string _tempPath = "";

    [TestInitialize]
    public void Setup()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"luban_test_{Guid.NewGuid():N}.json");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_tempPath)) File.Delete(_tempPath);
    }

    private sealed class FakeBuiltinMCPClient : IMCPClient
    {
        public string Name => "builtin-mcp";
        public string Description => "测试";
        public bool IsConnected => false;
        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task DisconnectAsync() => Task.CompletedTask;
        public Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<MCPTool>());
        public Task<MCPToolResult> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default)
            => Task.FromResult(new MCPToolResult { Success = true });
        public Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<MCPResource>());
        public Task<MCPResourceContent> ReadResourceAsync(string resourceUri, CancellationToken cancellationToken = default)
            => Task.FromResult(new MCPResourceContent { Uri = resourceUri });
    }

    [TestMethod]
    public void GetAll_CreatesExternalInstanceFromConfig()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddMcpServer(new McpServerConfig { Name = "ext1", Command = "npx" });
        var registry = new MCPRegistry(new IMCPClient[] { new FakeBuiltinMCPClient() }, cm);

        var all = registry.GetAll();
        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all.Any(c => c.Name == "ext1"));
    }

    [TestMethod]
    public void GetAll_RemovesDisabledExternal()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddMcpServer(new McpServerConfig { Name = "ext1", Command = "npx" });
        var registry = new MCPRegistry(Array.Empty<IMCPClient>(), cm);

        Assert.AreEqual(1, registry.GetAll().Count);

        cm.SetMcpServerEnabled("ext1", false);

        Assert.AreEqual(0, registry.GetAll().Count);
    }

    [TestMethod]
    public void GetAll_ExcludesDisabledBuiltin()
    {
        var cm = new ConfigManager(_tempPath);
        cm.SetBuiltinMcpClientEnabled("builtin-mcp", false);
        var registry = new MCPRegistry(new IMCPClient[] { new FakeBuiltinMCPClient() }, cm);

        Assert.AreEqual(0, registry.GetAll().Count);
    }

    [TestMethod]
    public void Get_BuiltinWinsOnNameCollision()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddMcpServer(new McpServerConfig { Name = "builtin-mcp", Command = "npx" });
        var registry = new MCPRegistry(new IMCPClient[] { new FakeBuiltinMCPClient() }, cm);

        var client = registry.Get("builtin-mcp");
        Assert.IsInstanceOfType<FakeBuiltinMCPClient>(client);
        Assert.AreEqual(1, registry.GetAll().Count);
    }

    [TestMethod]
    public void IsBuiltin_DistinguishesExternal()
    {
        var cm = new ConfigManager(_tempPath);
        cm.AddMcpServer(new McpServerConfig { Name = "ext1", Command = "npx" });
        var registry = new MCPRegistry(new IMCPClient[] { new FakeBuiltinMCPClient() }, cm);

        Assert.IsTrue(registry.IsBuiltin("builtin-mcp"));
        Assert.IsFalse(registry.IsBuiltin("ext1"));
    }
}
