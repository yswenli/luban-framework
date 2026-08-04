using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Tests.Retrieval.Fakes;
using LuBan.AIAgent.Tools.Retrieval;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class RetrievalToolPluginTests
{
    [TestMethod]
    public void GetTools_NoService_ReturnsEmpty()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<LuBanAgentOptions>(_ => { });
        var sp = services.BuildServiceProvider();
        var plugin = new RetrievalToolPlugin(sp.GetRequiredService<IOptions<LuBanAgentOptions>>());
        var tools = plugin.GetTools(sp);
        Assert.AreEqual(0, tools.Count);
    }

    [TestMethod]
    public void GetTools_WithService_Returns4Functions()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<LuBanAgentOptions>(_ => { });
        services.AddSingleton<IRetrievalService>(new FakeRetrievalService());
        var sp = services.BuildServiceProvider();
        var plugin = new RetrievalToolPlugin(sp.GetRequiredService<IOptions<LuBanAgentOptions>>());
        var tools = plugin.GetTools(sp);
        Assert.AreEqual(4, tools.Count, $"Expected 4 tools but got {tools.Count}: {string.Join(", ", tools.Select(t => t.Name))}");
    }

    [TestMethod]
    public void IsEnabled_DefaultTrue()
    {
        var plugin = new RetrievalToolPlugin(Options.Create(new LuBanAgentOptions()));
        Assert.IsTrue(plugin.IsEnabled(new LuBanAgentOptions()));
    }
}
