using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LogAnalysis;
using LuBan.AIAgent.Tools.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class LogToolPluginUnitTest
{
    private static ServiceProvider BuildSp(LogToolOptions? options = null)
    {
        var services = new ServiceCollection();
        var effective = options ?? new LogToolOptions();
        services.AddSingleton<IOptions<LogToolOptions>>(Options.Create(effective));
        services.AddSingleton<ILogAggregator>(new LogAggregator(Options.Create(effective)));
        services.AddSingleton<IToolConfirmationService>(new LogTestConfirmation());
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void Metadata_Is_NonOptIn_Log_Group()
    {
        var plugin = new LogToolPlugin(Options.Create(new LogToolOptions()));

        Assert.AreEqual("log", plugin.GroupName);
        Assert.IsFalse(plugin.IsOptIn);
        Assert.IsTrue(plugin.IsEnabled(new LuBanAgentOptions()));
    }

    [TestMethod]
    public void IsEnabled_Follows_Config()
    {
        var plugin = new LogToolPlugin(Options.Create(new LogToolOptions()));
        var disabled = new LuBanAgentOptions();
        disabled.Tools.Log.Enabled = false;

        Assert.IsFalse(plugin.IsEnabled(disabled));
    }

    [TestMethod]
    public void GetTools_Exposes_Three_Functions()
    {
        using var sp = BuildSp();
        var plugin = new LogToolPlugin(Options.Create(new LogToolOptions()));

        var tools = plugin.GetTools(sp);

        Assert.AreEqual(3, tools.Count);
        var names = tools.Select(t => t.Name).ToHashSet();
        Assert.IsTrue(names.Contains("LogStatsAsync"));
        Assert.IsTrue(names.Contains("LogGrepAsync"));
        Assert.IsTrue(names.Contains("LogSliceAsync"));
    }

    [TestMethod]
    public void GetTools_Ignores_ToolsOptions_Override()
    {
        using var sp = BuildSp();
        var plugin = new LogToolPlugin(Options.Create(new LogToolOptions()));

        // 传入一个 Log.Enabled=false 的 toolsOptions，插件仍应返回工具（以注入配置为准）
        var overrideOptions = new ToolGroupOptions();
        overrideOptions.Log.Enabled = false;

        var tools = plugin.GetTools(sp, overrideOptions);

        Assert.AreEqual(3, tools.Count);
    }

    [TestMethod]
    public void DefaultReadOnlyTools_Contains_Log_Tools()
    {
        var service = new ToolConfirmationService(new ToolConfirmationContext());

        Assert.IsTrue(service.ReadOnlyTools.Contains("LogStatsAsync"));
        Assert.IsTrue(service.ReadOnlyTools.Contains("LogGrepAsync"));
        Assert.IsTrue(service.ReadOnlyTools.Contains("LogSliceAsync"));
    }
}
