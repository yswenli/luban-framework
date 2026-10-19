/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： McpManageToolGroupUnitTest
*唯一标识：通用 MCP 能力单测
*创建时间：2026/10/19
*描述：验证 MCP 管理工具写入新字段（url/headers/environment）、必填校验、
*      明文凭据拒绝、覆盖保护与文件名校验。
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.Plugins.Security;
using LuBan.AIAgent.Tools.McpManage;

namespace LuBan.XTestProject;

[TestClass]
public class McpManageToolGroupUnitTest
{
    private sealed class StubCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);

        public Task SetAsync(string key, string? value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value)) _values.Remove(key);
            else _values[key] = value;
            return Task.CompletedTask;
        }

        public bool Contains(string key) => _values.ContainsKey(key);

        public IReadOnlyCollection<string> Keys => _values.Keys.ToList();
    }

    private static (McpManageToolGroup Group, MCPRegistry Registry, string Workspace) CreateGroup()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "luban-mcpmanage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);

        var registry = new MCPRegistry(
            Array.Empty<IMCPClient>(),
            null,
            new CredentialValueResolver(new StubCredentialStore()));

        var group = new McpManageToolGroup(
            registry,
            new McpManageToolOptions(),
            new McpOptions(),
            null,
            new CredentialValueResolver(new StubCredentialStore()));

        return (group, registry, workspace);
    }

    private static string ConfigPath(string workspace, string name)
        => Path.Combine(workspace, ".luban-agent", "mcps", name + ".json");

    [TestMethod]
    public async Task WriteMcpServerAsync_应支持http与凭据引用头()
    {
        var (group, _, workspace) = CreateGroup();
        try
        {
            var result = await group.WriteMcpServerAsync(
                name: "demo-remote",
                command: null,
                args: null,
                transport: "http",
                description: "远程 MCP",
                overwrite: false,
                workspaceRoot: workspace,
                url: "https://example.invalid/ai/mcp",
                headers: new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer $credential:connector:demo:token"
                });

            Assert.IsTrue(result.IsSuccess, result.Message);

            var file = ConfigPath(workspace, "demo-remote");
            Assert.IsTrue(File.Exists(file));

            var text = await File.ReadAllTextAsync(file);
            StringAssert.Contains(text, "https://example.invalid/ai/mcp");
            StringAssert.Contains(text, "$credential:connector:demo:token");
            StringAssert.Contains(text, "\"http\"");
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public async Task WriteMcpServerAsync_http缺Url应失败()
    {
        var (group, _, workspace) = CreateGroup();
        try
        {
            var result = await group.WriteMcpServerAsync(
                "demo", null, null, "http", null, false, workspace);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message!, "URL");
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public async Task WriteMcpServerAsync_stdio缺Command应失败()
    {
        var (group, _, workspace) = CreateGroup();
        try
        {
            var result = await group.WriteMcpServerAsync(
                "demo", null, null, "stdio", null, false, workspace);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message!, "启动命令");
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public async Task WriteMcpServerAsync_明文令牌应被拒且不落盘()
    {
        var (group, _, workspace) = CreateGroup();
        try
        {
            var result = await group.WriteMcpServerAsync(
                name: "demo-remote",
                command: null,
                args: null,
                transport: "http",
                description: null,
                overwrite: false,
                workspaceRoot: workspace,
                url: "https://example.invalid/ai/mcp",
                headers: new Dictionary<string, string> { ["Authorization"] = "Bearer plain-token" });

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message!, "凭据引用");
            Assert.IsFalse(File.Exists(ConfigPath(workspace, "demo-remote")), "明文配置不得落盘");
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public async Task WriteMcpServerAsync_应拒绝路径穿越名称()
    {
        var (group, _, workspace) = CreateGroup();
        try
        {
            var result = await group.WriteMcpServerAsync(
                "../evil", "npx", null, "stdio", null, false, workspace);

            Assert.IsFalse(result.IsSuccess);
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public async Task WriteMcpServerAsync_无覆盖标志时同名应失败()
    {
        var (group, _, workspace) = CreateGroup();
        try
        {
            var config = new McpServerConfig
            {
                Name = "demo",
                Transport = "stdio",
                Command = "npx"
            };

            var first = await group.WriteConfigAsync(config, overwrite: false, workspaceRoot: workspace);
            Assert.IsTrue(first.IsSuccess, first.Message);

            var second = await group.WriteConfigAsync(config, overwrite: false, workspaceRoot: workspace);
            Assert.IsFalse(second.IsSuccess);

            var third = await group.WriteConfigAsync(config, overwrite: true, workspaceRoot: workspace);
            Assert.IsTrue(third.IsSuccess, third.Message);
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }
}