using LuBan.AIAgent;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LogAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogDiUnitTest
{
    [TestMethod]
    public void AddLuBanAgent_Registers_LogAggregator_And_Binds_Options()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddLuBanAgent(configuration);
        using var sp = services.BuildServiceProvider();

        Assert.IsNotNull(sp.GetService<ILogAggregator>());
        Assert.IsNotNull(sp.GetService<IOptions<LogToolOptions>>());
        Assert.IsTrue(sp.GetRequiredService<IOptions<LogToolOptions>>().Value.Enabled);
    }

    [TestMethod]
    public void AddLuBanAgent_Binds_Log_Section_Values()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LuBanAgent:Tools:Log:TopN"] = "5",
                ["LuBanAgent:Tools:Log:Enabled"] = "true"
            })
            .Build();

        services.AddLuBanAgent(configuration);
        using var sp = services.BuildServiceProvider();

        var value = sp.GetRequiredService<IOptions<LogToolOptions>>().Value;
        Assert.AreEqual(5, value.TopN);
    }

    [TestMethod]
    public void LogAggregator_Is_Registered_As_Singleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddLuBanAgent(configuration);
        using var sp = services.BuildServiceProvider();

        Assert.AreSame(sp.GetRequiredService<ILogAggregator>(), sp.GetRequiredService<ILogAggregator>());
    }
}
