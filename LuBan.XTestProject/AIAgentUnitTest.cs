/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Providers;
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
                Script = new ScriptToolOptions { Enabled = false },
                Database = new DatabaseToolOptions { Enabled = false },
                Redis = new RedisToolOptions { Enabled = false }
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
    /// 测试 LuBanChatClient 路由
    /// </summary>
    [TestMethod]
    public void TestLuBanChatClientRouting()
    {
        var mockClient1 = new MockChatClient("provider1");
        var mockClient2 = new MockChatClient("provider2");

        var clients = new List<KeyValuePair<string, IChatClient>>
        {
            new("provider1", mockClient1),
            new("provider2", mockClient2)
        };

        var luBanClient = new LuBanChatClient(clients, "provider1");

        Assert.IsNotNull(luBanClient);

        var provider1 = luBanClient.GetType()
            .GetMethod("GetProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(luBanClient, new object[] { "provider1:model" });

        var provider2 = luBanClient.GetType()
            .GetMethod("GetProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(luBanClient, new object[] { "provider2:model" });

        Assert.IsNotNull(provider1);
        Assert.IsNotNull(provider2);
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