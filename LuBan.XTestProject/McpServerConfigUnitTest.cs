/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： McpServerConfigUnitTest
*唯一标识：通用 MCP 能力单测
*创建时间：2026/10/19
*描述：验证 ① McpServerConfig 新增 Url/Headers/Environment 字段的序列化往返；
*      ② MCPClientFactory 的传输分派（stdio / sse / http）；
*      ③ HttpMCPClient 端点解析（Url 优先、旧写法回退、sse 追加 /message）；
*      ④ MCPRegistry 指纹把 Url/Headers/Environment 计入（热重载替换客户端）。
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;

namespace LuBan.XTestProject;

[TestClass]
public class McpServerConfigUnitTest
{
    [TestMethod]
    public void McpServerConfig_新增字段应可序列化往返()
    {
        var config = new McpServerConfig
        {
            Name = "demo",
            Transport = "http",
            Url = "https://example.invalid/ai/mcp",
            Headers = { ["Authorization"] = "Bearer $credential:connector:demo:token" },
            Environment = { ["DEMO_ACCESS_TOKEN"] = "$credential:connector:demo:token" }
        };

        var json = JsonSerializer.Serialize(config);
        var back = JsonSerializer.Deserialize<McpServerConfig>(json);

        Assert.IsNotNull(back);
        Assert.AreEqual("https://example.invalid/ai/mcp", back!.Url);
        Assert.AreEqual("Bearer $credential:connector:demo:token", back.Headers["Authorization"]);
        Assert.AreEqual("$credential:connector:demo:token", back.Environment["DEMO_ACCESS_TOKEN"]);
        // 旧字段不受影响
        Assert.AreEqual("http", back.Transport);
        Assert.IsTrue(back.Enabled);
    }

    [TestMethod]
    public void MCPClientFactory_应按传输方式分派()
    {
        Assert.IsInstanceOfType<HttpMCPClient>(MCPClientFactory.Create(new McpServerConfig { Transport = "http" }));
        Assert.IsInstanceOfType<HttpMCPClient>(MCPClientFactory.Create(new McpServerConfig { Transport = "sse" }));
        Assert.IsInstanceOfType<HttpMCPClient>(MCPClientFactory.Create(new McpServerConfig { Transport = "HTTP" }));
        Assert.IsInstanceOfType<StdioMCPClient>(MCPClientFactory.Create(new McpServerConfig { Transport = "stdio" }));
        // 旧配置未写 transport → stdio（保持既有默认）
        Assert.IsInstanceOfType<StdioMCPClient>(MCPClientFactory.Create(new McpServerConfig()));
    }

    [TestMethod]
    public void HttpMCPClient_端点解析应Url优先且兼容旧写法()
    {
        var http = (HttpMCPClient)MCPClientFactory.Create(new McpServerConfig
        {
            Transport = "http",
            Url = "https://example.invalid/ai/mcp"
        });
        Assert.AreEqual("https://example.invalid/ai/mcp", http.Endpoint);
        Assert.IsTrue(http.StreamableHttp);

        // sse：Url + /message（去尾斜杠）
        var sse = (HttpMCPClient)MCPClientFactory.Create(new McpServerConfig
        {
            Transport = "sse",
            Url = "https://example.invalid/ai/mcp/"
        });
        Assert.AreEqual("https://example.invalid/ai/mcp/message", sse.Endpoint);
        Assert.IsFalse(sse.StreamableHttp);

        // 旧写法：URL 塞在 Args[0]
        var legacy = (HttpMCPClient)MCPClientFactory.Create(new McpServerConfig
        {
            Transport = "sse",
            Args = { "https://example.invalid/legacy" }
        });
        Assert.AreEqual("https://example.invalid/legacy/message", legacy.Endpoint);

        // 旧写法：URL 塞在 Command
        var legacyCommand = (HttpMCPClient)MCPClientFactory.Create(new McpServerConfig
        {
            Transport = "http",
            Command = "https://example.invalid/cmd"
        });
        Assert.AreEqual("https://example.invalid/cmd", legacyCommand.Endpoint);

        // 未配置端点
        var empty = (HttpMCPClient)MCPClientFactory.Create(new McpServerConfig { Transport = "http" });
        Assert.IsNull(empty.Endpoint);
    }

    [TestMethod]
    public void MCPRegistry_指纹应包含Url_改动即替换客户端()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "luban-mcp-fp-" + Guid.NewGuid().ToString("N"));
        var mcpsDir = Path.Combine(workspace, ".luban-agent", "mcps");
        Directory.CreateDirectory(mcpsDir);
        var file = Path.Combine(mcpsDir, "remote.json");

        try
        {
            File.WriteAllText(file, """
                {
                  "name": "remote",
                  "transport": "http",
                  "url": "https://example.invalid/a",
                  "enabled": true
                }
                """);

            var registry = new MCPRegistry(Array.Empty<IMCPClient>());
            registry.LoadFromWorkspace(workspace);
            var first = registry.Get("remote");
            Assert.IsNotNull(first);

            File.WriteAllText(file, """
                {
                  "name": "remote",
                  "transport": "http",
                  "url": "https://example.invalid/b",
                  "enabled": true
                }
                """);

            registry.LoadFromWorkspace(workspace);
            var second = registry.Get("remote");

            Assert.IsNotNull(second);
            Assert.AreNotSame(first, second, "URL 变化必须重建客户端（指纹需覆盖 Url）");
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public void MCPRegistry_指纹应包含Headers_改动即替换客户端()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "luban-mcp-fp-h-" + Guid.NewGuid().ToString("N"));
        var mcpsDir = Path.Combine(workspace, ".luban-agent", "mcps");
        Directory.CreateDirectory(mcpsDir);
        var file = Path.Combine(mcpsDir, "remote.json");

        try
        {
            File.WriteAllText(file, """
                {
                  "name": "remote",
                  "transport": "http",
                  "url": "https://example.invalid/a",
                  "headers": { "X-Devops-Toolsets": "code-management" },
                  "enabled": true
                }
                """);

            var registry = new MCPRegistry(Array.Empty<IMCPClient>());
            registry.LoadFromWorkspace(workspace);
            var first = registry.Get("remote");
            Assert.IsNotNull(first);

            File.WriteAllText(file, """
                {
                  "name": "remote",
                  "transport": "http",
                  "url": "https://example.invalid/a",
                  "headers": { "X-Devops-Toolsets": "project-management" },
                  "enabled": true
                }
                """);

            registry.LoadFromWorkspace(workspace);
            var second = registry.Get("remote");

            Assert.IsNotNull(second);
            Assert.AreNotSame(first, second, "请求头变化必须重建客户端（指纹需覆盖 Headers）");
        }
        finally
        {
            try { Directory.Delete(workspace, recursive: true); } catch { }
        }
    }
}