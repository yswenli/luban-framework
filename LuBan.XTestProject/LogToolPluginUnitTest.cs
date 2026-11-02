/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： LogToolPluginUnitTest
*版本号： V1.0.0.0
*唯一标识：968c3d5a-2528-4dc2-8360-d68a290da6be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/10 10:03:08
*描述：日志工具插件元数据与工具暴露单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/10 10:03:08
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志工具插件元数据与工具暴露单元测试
*
*****************************************************************************/

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

        var tools = plugin.GetTools(sp.AsDiContainer());

        Assert.AreEqual(3, tools.Count);
        var names = tools.Select(t => t.Name).ToHashSet();
        Assert.IsTrue(names.Contains("LogStats"));
        Assert.IsTrue(names.Contains("LogGrep"));
        Assert.IsTrue(names.Contains("LogSlice"));
    }

    [TestMethod]
    public void GetTools_Ignores_ToolsOptions_Override()
    {
        using var sp = BuildSp();
        var plugin = new LogToolPlugin(Options.Create(new LogToolOptions()));

        // 传入一个 Log.Enabled=false 的 toolsOptions，插件仍应返回工具（以注入配置为准）
        var overrideOptions = new ToolGroupOptions();
        overrideOptions.Log.Enabled = false;

        var tools = plugin.GetTools(sp.AsDiContainer(), overrideOptions);

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

    [TestMethod]
    public void GetTools_Returns_Empty_When_Disabled()
    {
        var options = new LogToolOptions { Enabled = false };
        using var sp = BuildSp(options);
        var plugin = new LogToolPlugin(Options.Create(options));

        var tools = plugin.GetTools(sp.AsDiContainer());

        Assert.AreEqual(0, tools.Count);
    }

    [TestMethod]
    public void GetTools_Returns_Empty_When_Aggregator_Missing()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<LogToolOptions>>(Options.Create(new LogToolOptions()));
        services.AddSingleton<IToolConfirmationService>(new LogTestConfirmation());
        using var sp = services.BuildServiceProvider();
        var plugin = new LogToolPlugin(Options.Create(new LogToolOptions()));

        var tools = plugin.GetTools(sp.AsDiContainer());

        Assert.AreEqual(0, tools.Count);
    }
}
