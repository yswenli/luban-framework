/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests
*文件名： AIAgentTests
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

namespace LuBan.AIAgent.Tests;

[TestClass]
public class AIAgentTests
{
    [TestMethod]
    public void TestLuBanAgentOptions_Defaults()
    {
        var options = new LuBanAgentOptions();

        Assert.IsNull(options.DefaultModel);
        Assert.IsNull(options.SystemPrompt);
        Assert.IsNull(options.Description);
        Assert.AreEqual(10, options.MaxToolLoopIterations);
    }

    [TestMethod]
    public void TestLuBanAgentOptions_CustomValues()
    {
        var options = new LuBanAgentOptions
        {
            DefaultModel = "openai:gpt-4",
            SystemPrompt = "测试提示词",
            Description = "测试描述",
            MaxToolLoopIterations = 5
        };

        Assert.AreEqual("openai:gpt-4", options.DefaultModel);
        Assert.AreEqual("测试提示词", options.SystemPrompt);
        Assert.AreEqual("测试描述", options.Description);
        Assert.AreEqual(5, options.MaxToolLoopIterations);
    }

    [TestMethod]
    public void TestToolGroupOptions_Defaults()
    {
        var options = new ToolGroupOptions();

        Assert.IsNotNull(options.FileSystem);
        Assert.IsNotNull(options.Browser);
        Assert.IsNotNull(options.Script);
        Assert.IsNotNull(options.Database);
        Assert.IsNotNull(options.Redis);
        Assert.IsNotNull(options.Web);
    }

    [TestMethod]
    public void TestFileSystemToolOptions_AllowedRoots()
    {
        var options = new FileSystemToolOptions
        {
            Enabled = true,
            AllowedRoots = new List<string> { "C:\\Temp", "D:\\Work" }
        };

        Assert.IsTrue(options.Enabled);
        Assert.AreEqual(2, options.AllowedRoots.Count);
        Assert.IsTrue(options.AllowedRoots.Contains("C:\\Temp"));
    }

    [TestMethod]
    public void TestPathGuard_AllowedPaths()
    {
        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions
                {
                    Enabled = true,
                    AllowedRoots = new List<string>
                    {
                        "C:\\Temp",
                        "D:\\Work"
                    }
                }
            }
        };

        var pathGuard = new PathGuard(Options.Create(options));

        Assert.IsTrue(pathGuard.IsAllowed("C:\\Temp\\test.txt"));
        Assert.IsTrue(pathGuard.IsAllowed("C:\\Temp\\subdir\\file.cs"));
        Assert.IsTrue(pathGuard.IsAllowed("D:\\Work\\project\\file.cs"));
        Assert.IsFalse(pathGuard.IsAllowed("C:\\Windows\\system32"));
        Assert.IsFalse(pathGuard.IsAllowed("E:\\Data\\file.txt"));
    }

    [TestMethod]
    public void TestPathGuard_EmptyAllowedRoots()
    {
        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions
                {
                    Enabled = true,
                    AllowedRoots = new List<string>()
                }
            }
        };

        var pathGuard = new PathGuard(Options.Create(options));

        Assert.IsTrue(pathGuard.IsAllowed("C:\\Any\\Path\\file.txt"));
        Assert.IsTrue(pathGuard.IsAllowed("D:\\Any\\Path\\file.txt"));
    }

    [TestMethod]
    public void TestPathGuard_IsPathSafe()
    {
        Assert.IsTrue(PathGuard.IsPathSafe("C:\\Temp\\file.txt"));
        Assert.IsTrue(PathGuard.IsPathSafe("file.txt"));
        Assert.IsFalse(PathGuard.IsPathSafe(""));
        Assert.IsFalse(PathGuard.IsPathSafe("   "));
        Assert.IsFalse(PathGuard.IsPathSafe("C:\\Temp\\..\\Windows"));
    }

    [TestMethod]
    public void TestToolPluginRegistry_GetEnabledPlugins()
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

    [TestMethod]
    public void TestToolPluginRegistry_GetPluginsByGroup()
    {
        var services = new ServiceCollection();

        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions { Enabled = true },
                Web = new WebToolOptions { Enabled = true },
                Browser = new BrowserToolOptions { Enabled = false }
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

        var filesystemPlugins = registry.GetPlugins(new[] { "filesystem" });
        Assert.AreEqual(1, filesystemPlugins.Count);
        Assert.AreEqual("filesystem", filesystemPlugins[0].GroupName);
    }

    [TestMethod]
    public void TestLuBanChatClient_ProviderRouting()
    {
        var mockClient1 = new MockChatClient("openai");
        var mockClient2 = new MockChatClient("azure");

        var clients = new List<KeyValuePair<string, IChatClient>>
        {
            new("openai", mockClient1),
            new("azure", mockClient2)
        };

        var luBanClient = new LuBanChatClient(clients, "openai");

        Assert.IsNotNull(luBanClient);

        var provider = luBanClient.GetType()
            .GetMethod("GetProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(luBanClient, new object[] { "openai:gpt-4" });

        Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void TestLuBanChatClient_DefaultProvider()
    {
        var mockClient = new MockChatClient("default");
        var clients = new List<KeyValuePair<string, IChatClient>>
        {
            new("default", mockClient)
        };

        var luBanClient = new LuBanChatClient(clients, "default");
        Assert.IsNotNull(luBanClient);
    }

    [TestMethod]
    public void TestPluginInterface_HasCorrectProperties()
    {
        var services = new ServiceCollection();

        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions { Enabled = true }
            }
        };

        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ProcessRunner>();
        services.AddSingleton<PathGuard>();
        services.AddSingleton<ILuBanToolPlugin, FileSystemToolPlugin>();

        var sp = services.BuildServiceProvider();
        var plugin = sp.GetRequiredService<ILuBanToolPlugin>();

        Assert.AreEqual("filesystem", plugin.GroupName);
        Assert.IsNotNull(plugin.Description);
        Assert.IsTrue(plugin.IsEnabled(options));
    }

    [TestMethod]
    public void TestDIExtensions_AddLuBanAgent()
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
        services.AddSingleton<IChatClient>(sp => new MockChatClient("test"));
        services.AddLuBanAgent(config);

        var sp = services.BuildServiceProvider();

        var factory = sp.GetService<ILuBanAgentFactory>();
        Assert.IsNotNull(factory);

        var registry = sp.GetService<ToolPluginRegistry>();
        Assert.IsNotNull(registry);
    }
}

public class MockChatClient : IChatClient
{
    public string Name { get; }

    private readonly Func<IEnumerable<ChatMessage>, string>? _responder;

    public MockChatClient(string name)
    {
        Name = name;
    }

    public MockChatClient(string name, Func<IEnumerable<ChatMessage>, string> responder)
    {
        Name = name;
        _responder = responder;
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var response = _responder != null ? _responder(messages) : $"Response from {Name}";
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, response)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
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