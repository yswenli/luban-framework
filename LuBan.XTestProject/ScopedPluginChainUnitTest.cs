/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ScopedPluginChainUnitTest
*创建时间：2026/10/10
*描述：验证工具确认链与插件链按会话作用域隔离，且无 captive dependency
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class ScopedPluginChainUnitTest
{
    private static ServiceProvider Build()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLuBanAgent(config, _ => new MockChatClient("test"));
        // 仅启用 ValidateScopes：本单测未注册宿主负责的 ILocalMemoryStore/具体工厂，
        // 全图 ValidateOnBuild 由 agent 宿主（AgentHostBuilder）启用，那里依赖完整。
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    [TestMethod]
    public void ToolConfirmationContext_DiffersPerScope()
    {
        using var sp = Build();
        using var scope1 = sp.CreateScope();
        using var scope2 = sp.CreateScope();
        var a = scope1.ServiceProvider.GetRequiredService<ToolConfirmationContext>();
        var b = scope2.ServiceProvider.GetRequiredService<ToolConfirmationContext>();
        Assert.AreNotSame(a, b);
    }

    [TestMethod]
    public void ToolPluginRegistry_DiffersPerScope()
    {
        using var sp = Build();
        using var scope1 = sp.CreateScope();
        using var scope2 = sp.CreateScope();
        var a = scope1.ServiceProvider.GetRequiredService<ToolPluginRegistry>();
        var b = scope2.ServiceProvider.GetRequiredService<ToolPluginRegistry>();
        Assert.AreNotSame(a, b);
    }

    [TestMethod]
    public void RootResolve_Throws_ForScopedToolConfirmation()
    {
        using var sp = Build();
        Assert.Throws<InvalidOperationException>(
            () => sp.GetRequiredService<ToolConfirmationContext>());
    }
}