/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： McpCredentialReferenceUnitTest
*唯一标识：通用 MCP 能力单测
*创建时间：2026/10/19
*描述：验证 ① $credential:<键> 引用解析（命中/缺失/非引用透传）；
*      ② McpConfigSecurityValidator 拒绝明文令牌落盘（敏感头/敏感环境变量/URL 内嵌凭据）；
*      ③ StdioMCPClient 环境变量注入（解析缺失即中止启动）。
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.Plugins.Security;
using System.Diagnostics;

namespace LuBan.XTestProject;

[TestClass]
public class McpCredentialReferenceUnitTest
{
    /// <summary>内存凭据存储替身。</summary>
    private sealed class StubCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public StubCredentialStore(params (string Key, string Value)[] items)
        {
            foreach (var (key, value) in items) _values[key] = value;
        }

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

    [TestMethod]
    public void CredentialValueResolver_引用识别与键提取()
    {
        Assert.IsTrue(CredentialValueResolver.IsReference("$credential:connector:demo:token"));
        Assert.IsFalse(CredentialValueResolver.IsReference("Bearer abc"));
        Assert.IsFalse(CredentialValueResolver.IsReference(""));

        Assert.AreEqual(
            "connector:demo:token",
            CredentialValueResolver.ExtractKey("$credential:connector:demo:token"));
        Assert.IsNull(CredentialValueResolver.ExtractKey("$credential:   "));
        Assert.IsNull(CredentialValueResolver.ExtractKey("not-a-ref"));
    }

    [TestMethod]
    public async Task CredentialValueResolver_应解析引用并透传普通值()
    {
        var resolver = new CredentialValueResolver(
            new StubCredentialStore(("connector:demo:token", "secret-value")));

        Assert.AreEqual("secret-value", await resolver.ResolveAsync("$credential:connector:demo:token"));
        // 内联使用（Bearer 前缀）
        Assert.AreEqual("Bearer secret-value", await resolver.ResolveAsync("Bearer $credential:connector:demo:token"));
        // 非引用原样返回
        Assert.AreEqual("Bearer literal", await resolver.ResolveAsync("Bearer literal"));
        // 空值原样返回
        Assert.IsNull(await resolver.ResolveAsync(null));
        // 引用缺失 → null
        Assert.IsNull(await resolver.ResolveAsync("$credential:connector:demo:missing"));
    }

    [TestMethod]
    public async Task NullMcpConfigValueResolver_应原样透传()
    {
        Assert.AreEqual("$credential:x", await NullMcpConfigValueResolver.Instance.ResolveAsync("$credential:x"));
    }

    [TestMethod]
    public void McpConfigSecurityValidator_敏感头明文应被拒()
    {
        var error = McpConfigSecurityValidator.Validate(new McpServerConfig
        {
            Name = "demo",
            Transport = "http",
            Url = "https://example.invalid/ai/mcp",
            Headers = { ["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiJ9.literal.token" }
        });

        Assert.IsNotNull(error);
        StringAssert.Contains(error!, "Authorization");

        // 改成凭据引用即通过
        var ok = McpConfigSecurityValidator.Validate(new McpServerConfig
        {
            Name = "demo",
            Transport = "http",
            Url = "https://example.invalid/ai/mcp",
            Headers = { ["Authorization"] = "Bearer $credential:connector:demo:token" }
        });

        Assert.IsNull(ok);
    }

    [TestMethod]
    public void McpConfigSecurityValidator_敏感环境变量明文应被拒()
    {
        var error = McpConfigSecurityValidator.Validate(new McpServerConfig
        {
            Name = "demo",
            Transport = "stdio",
            Command = "npx",
            Environment = { ["DEMO_ACCESS_TOKEN"] = "plain-text-token" }
        });

        Assert.IsNotNull(error);
        StringAssert.Contains(error!, "DEMO_ACCESS_TOKEN");

        // 非敏感环境变量允许明文字面量
        var ok = McpConfigSecurityValidator.Validate(new McpServerConfig
        {
            Name = "demo",
            Transport = "stdio",
            Command = "npx",
            Environment = { ["DEMO_REGION"] = "cn-hangzhou" }
        });

        Assert.IsNull(ok);
    }

    [TestMethod]
    public void McpConfigSecurityValidator_非敏感头允许明文字面量()
    {
        var ok = McpConfigSecurityValidator.Validate(new McpServerConfig
        {
            Name = "demo",
            Transport = "http",
            Url = "https://example.invalid/ai/mcp",
            Headers = { ["X-Devops-Toolsets"] = "code-management" }
        });

        Assert.IsNull(ok);
    }

    [TestMethod]
    public void McpConfigSecurityValidator_URL内嵌凭据应被拒且不回显明文()
    {
        var error = McpConfigSecurityValidator.Validate(new McpServerConfig
        {
            Name = "demo",
            Transport = "stdio",
            Command = "git",
            Args = { "clone", "https://oauth2:ghp_supersecrettoken@example.invalid/repo.git" }
        });

        Assert.IsNotNull(error);
        Assert.IsFalse(error!.Contains("ghp_supersecrettoken"), "错误信息不得回显明文令牌");
        StringAssert.Contains(error, "***");
    }

    [TestMethod]
    public async Task StdioMCPClient_应注入解析后的环境变量()
    {
        var resolver = new CredentialValueResolver(
            new StubCredentialStore(("connector:demo:token", "resolved-token")));

        var psi = new ProcessStartInfo();
        var ok = await StdioMCPClient.ApplyEnvironmentAsync(
            psi,
            new Dictionary<string, string>
            {
                ["DEMO_ACCESS_TOKEN"] = "$credential:connector:demo:token",
                ["DEMO_REGION"] = "cn-hangzhou"
            },
            resolver);

        Assert.IsTrue(ok);
        Assert.AreEqual("resolved-token", psi.Environment["DEMO_ACCESS_TOKEN"]);
        Assert.AreEqual("cn-hangzhou", psi.Environment["DEMO_REGION"]);
    }

    [TestMethod]
    public async Task StdioMCPClient_引用解析失败应中止启动()
    {
        var resolver = new CredentialValueResolver(new StubCredentialStore());

        var psi = new ProcessStartInfo();
        var ok = await StdioMCPClient.ApplyEnvironmentAsync(
            psi,
            new Dictionary<string, string> { ["DEMO_ACCESS_TOKEN"] = "$credential:connector:demo:missing" },
            resolver);

        Assert.IsFalse(ok);
        Assert.IsFalse(psi.Environment.ContainsKey("DEMO_ACCESS_TOKEN"));
    }
}