/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： SubAgentFactoryTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：SubAgentFactory 单元测试
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Plugins;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class SubAgentFactoryTests
{
    [TestMethod]
    public async Task TestCreateSubAgentAsync_创建成功且名称为SubAgent()
    {
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => "子代理结果"));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        var sp = services.BuildServiceProvider();

        var factory = sp.GetRequiredService<LuBanAgentFactory>();
        var agent = await factory.CreateSubAgentAsync(
            modelName: null,
            toolGroups: null,
            systemPrompt: "你是子代理",
            cancellationToken: default);

        Assert.IsNotNull(agent);
        Assert.AreEqual("SubAgent", agent.Name);
    }

    [TestMethod]
    public async Task TestSubAgentFactory_从Spec创建Agent()
    {
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => "结果"));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddScoped<SubAgentFactory>();
        var sp = services.BuildServiceProvider();

        var subFactory = sp.GetRequiredService<SubAgentFactory>();
        var spec = new SubAgentSpec
        {
            NodeId = "test",
            Prompt = "执行测试",
            ParentSessionId = "parent-1"
        };
        var agent = await subFactory.CreateAsync(spec);

        Assert.IsNotNull(agent);
        Assert.IsNotNull(spec.SessionId);
    }
}
