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
*文件名： LogDiUnitTest
*版本号： V1.0.0.0
*唯一标识：795c1180-e717-4c1b-b4ee-0cebadba18b4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9 18:21:53
*描述：日志组件的 DI 注册与选项绑定单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/9 18:21:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志组件的 DI 注册与选项绑定单元测试
*
*****************************************************************************/

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

        services.AsDiContainer().AddLuBanAgent(configuration);
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

        services.AsDiContainer().AddLuBanAgent(configuration);
        using var sp = services.BuildServiceProvider();

        var value = sp.GetRequiredService<IOptions<LogToolOptions>>().Value;
        Assert.AreEqual(5, value.TopN);
    }

    [TestMethod]
    public void LogAggregator_Is_Registered_As_Singleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AsDiContainer().AddLuBanAgent(configuration);
        using var sp = services.BuildServiceProvider();

        Assert.AreSame(sp.GetRequiredService<ILogAggregator>(), sp.GetRequiredService<ILogAggregator>());
    }
}
