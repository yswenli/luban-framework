/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： ModelRoutingTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：多模型路由单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Planner;
using LuBan.AIAgent.Tests;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class ModelRoutingTests
{
    private static ServiceProvider BuildFactoryServices(MockProviderRouter router)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new LuBanAgentOptions()));
        services.AddSingleton<IChatClient>(new MockChatClient("default", _ => "结果"));
        services.AddSingleton<IProviderRouter>(router);
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TestCreateSubAgentAsync_指定模型时走路由()
    {
        var router = new MockProviderRouter(new MockChatClient("kimi", _ => "结果"));
        using var sp = BuildFactoryServices(router);
        var factory = sp.GetRequiredService<LuBanAgentFactory>();

        var agent = await factory.CreateSubAgentAsync("kimi:k2", null, "你是子代理");

        Assert.IsNotNull(agent);
        Assert.IsTrue(router.RequestedModels.Contains("kimi:k2"));
    }

    [TestMethod]
    public async Task TestCreateSubAgentAsync_未指定模型不走路由()
    {
        var router = new MockProviderRouter(new MockChatClient("kimi", _ => "结果"));
        using var sp = BuildFactoryServices(router);
        var factory = sp.GetRequiredService<LuBanAgentFactory>();

        var agent = await factory.CreateSubAgentAsync(null, null, "你是子代理");

        Assert.IsNotNull(agent);
        Assert.AreEqual(0, router.RequestedModels.Count);
    }

    [TestMethod]
    public async Task TestCreateSubAgentAsync_路由失败回退默认模型()
    {
        var router = new MockProviderRouter(new MockChatClient("default", _ => "结果"), throwOnRoute: true);
        using var sp = BuildFactoryServices(router);
        var factory = sp.GetRequiredService<LuBanAgentFactory>();

        var agent = await factory.CreateSubAgentAsync("missing:model", null, "你是子代理");

        Assert.IsNotNull(agent);
    }

    private const string PlannerGraphJson = """
        { "nodes": [ { "id": "a", "description": "a", "prompt": "p", "dependencies": [], "toolGroups": ["filesystem"] } ] }
        """;

    private static ServiceProvider BuildPlannerServices(MockProviderRouter router, string? plannerModel)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { PlannerModel = plannerModel, MaxNodes = 10 }
        }));
        services.AddSingleton<IChatClient>(new MockChatClient("default", _ => PlannerGraphJson));
        services.AddSingleton<IProviderRouter>(router);
        services.AddSingleton<ToolPluginRegistry>();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TestLlmTaskPlanner_配置PlannerModel时走路由()
    {
        var router = new MockProviderRouter(new MockChatClient("planner", _ => PlannerGraphJson));
        using var sp = BuildPlannerServices(router, "kimi:planner-strong");

        var planner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>(),
            router);
        var graph = await planner.PlanAsync("任意任务");

        Assert.IsNotNull(graph);
        Assert.IsTrue(router.RequestedModels.Contains("kimi:planner-strong"));
    }

    [TestMethod]
    public async Task TestLlmTaskPlanner_未配置PlannerModel不走路由()
    {
        var router = new MockProviderRouter(new MockChatClient("planner", _ => PlannerGraphJson));
        using var sp = BuildPlannerServices(router, null);

        var planner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>(),
            router);
        var graph = await planner.PlanAsync("任意任务");

        Assert.IsNotNull(graph);
        Assert.AreEqual(0, router.RequestedModels.Count);
    }

    [TestMethod]
    public async Task TestLlmTaskPlanner_路由失败回退注入客户端()
    {
        var router = new MockProviderRouter(new MockChatClient("x", _ => PlannerGraphJson), throwOnRoute: true);
        using var sp = BuildPlannerServices(router, "missing:model");

        var planner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>(),
            router);
        var graph = await planner.PlanAsync("任意任务");

        Assert.IsNotNull(graph);
    }
}
