/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： McpRegistryConnectUnitTest
*描述：验证 MCPRegistry.ConnectAllAsync 跳过内置、并发、超时
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class McpRegistryConnectUnitTest
{
    [TestMethod]
    public async Task ConnectAllAsync_SkipsBuiltin()
    {
        // 内置客户端（IsConnected 恒 true）
        var builtin = new FakeMcpClient("filesystem", isBuiltin: true);
        // 非内置客户端
        var external = new FakeMcpClient("demo-mcp", isBuiltin: false);

        var registry = new MCPRegistry(new[] { builtin, external });

        var results = await registry.ConnectAllAsync();

        // 内置应被跳过，不在结果中
        Assert.IsFalse(results.ContainsKey("filesystem"));
        // 非内置应被连接
        Assert.IsTrue(results.ContainsKey("demo-mcp"));
        Assert.IsTrue(results["demo-mcp"]);
    }

    [TestMethod]
    public async Task ConnectAllAsync_RespectsTimeout()
    {
        var slow = new SlowFakeMcpClient("slow", delayMs: 500);
        var registry = new MCPRegistry(new[] { slow });

        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var results = await registry.ConnectAllAsync(cts.Token);

        Assert.IsFalse(results["slow"]);
    }

    private sealed class FakeMcpClient : IMCPClient
    {
        private readonly bool _isBuiltin;
        public FakeMcpClient(string name, bool isBuiltin) { Name = name; _isBuiltin = isBuiltin; }
        public string Name { get; }
        public string Description => "fake";
        public bool IsConnected => _isBuiltin;
        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task DisconnectAsync() => Task.CompletedTask;
        public Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<MCPTool>());
        public Task<MCPToolResult> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default) => Task.FromResult(new MCPToolResult { Success = false });
        public Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<MCPResource>());
        public Task<MCPResourceContent> ReadResourceAsync(string resourceUri, CancellationToken cancellationToken = default) => Task.FromResult(new MCPResourceContent());
    }

    private sealed class SlowFakeMcpClient : IMCPClient
    {
        private readonly int _delayMs;
        public SlowFakeMcpClient(string name, int delayMs) { Name = name; _delayMs = delayMs; }
        public string Name { get; }
        public string Description => "slow";
        public bool IsConnected => false;
        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delayMs, cancellationToken);
            return !cancellationToken.IsCancellationRequested;
        }
        public Task DisconnectAsync() => Task.CompletedTask;
        public Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<MCPTool>());
        public Task<MCPToolResult> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default) => Task.FromResult(new MCPToolResult { Success = false });
        public Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<MCPResource>());
        public Task<MCPResourceContent> ReadResourceAsync(string resourceUri, CancellationToken cancellationToken = default) => Task.FromResult(new MCPResourceContent());
    }
}