/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： LlmTaskPlannerTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：LlmTaskPlanner 单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Planner;
using LuBan.AIAgent.Tests;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class LlmTaskPlannerTests
{
    private static string ValidGraphJson => """
        {
            "nodes": [
                {
                    "id": "research",
                    "description": "搜索资料",
                    "prompt": "搜索相关资料",
                    "dependencies": [],
                    "isCritical": true
                },
                {
                    "id": "analyze",
                    "description": "分析结果",
                    "prompt": "基于 {dep:research} 进行分析",
                    "dependencies": ["research"],
                    "isCritical": false
                }
            ]
        }
        """;

    private static (IChatClient, IServiceProvider) BuildPlannerDeps(string llmResponse)
    {
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { MaxNodes = 10 }
        };
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => llmResponse));
        services.AddSingleton<ToolPluginRegistry>();
        var sp = services.BuildServiceProvider();
        return (sp.GetRequiredService<IChatClient>(), sp);
    }

    [TestMethod]
    public async Task TestPlanAsync_合法JSON返回图谱()
    {
        var (chatClient, sp) = BuildPlannerDeps(ValidGraphJson);
        var options = sp.GetRequiredService<IOptions<LuBanAgentOptions>>();
        var planner = new LlmTaskPlanner(chatClient, sp, options);

        var graph = await planner.PlanAsync("搜索并分析");

        Assert.IsNotNull(graph);
        Assert.AreEqual(2, graph!.Nodes.Count);
        Assert.AreEqual("research", graph.Nodes[0].Id);
        Assert.AreEqual("llm", graph.Source);
        Assert.AreEqual("搜索并分析", graph.OriginalTask);
    }

    [TestMethod]
    public async Task TestPlanAsync_非法JSON重试后抛异常()
    {
        var (chatClient, sp) = BuildPlannerDeps("不是JSON");
        var options = sp.GetRequiredService<IOptions<LuBanAgentOptions>>();
        var planner = new LlmTaskPlanner(chatClient, sp, options);

        await Assert.ThrowsExceptionAsync<TaskPlanningException>(
            () => planner.PlanAsync("测试任务"));
    }

    [TestMethod]
    public async Task TestPlanAsync_空内容抛异常()
    {
        var (chatClient, sp) = BuildPlannerDeps("");
        var options = sp.GetRequiredService<IOptions<LuBanAgentOptions>>();
        var planner = new LlmTaskPlanner(chatClient, sp, options);

        await Assert.ThrowsExceptionAsync<TaskPlanningException>(
            () => planner.PlanAsync("测试任务"));
    }

    [TestMethod]
    public async Task TestPlanAsync_超过MaxNodes截断()
    {
        var manyNodes = """
            {
                "nodes": [
                    {"id":"n1","description":"n1","prompt":"p1","dependencies":[]},
                    {"id":"n2","description":"n2","prompt":"p2","dependencies":[]},
                    {"id":"n3","description":"n3","prompt":"p3","dependencies":[]}
                ]
            }
            """;
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { MaxNodes = 2 }
        };
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => manyNodes));
        services.AddSingleton<ToolPluginRegistry>();
        var sp = services.BuildServiceProvider();

        var planner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>());

        var graph = await planner.PlanAsync("多节点任务");

        Assert.IsNotNull(graph);
        Assert.AreEqual(2, graph!.Nodes.Count);
    }

    [TestMethod]
    public async Task TestPlanAsync_有环校验失败重试()
    {
        var cyclicGraph = """
            {
                "nodes": [
                    {"id":"a","description":"a","prompt":"p","dependencies":["c"]},
                    {"id":"b","description":"b","prompt":"p","dependencies":["a"]},
                    {"id":"c","description":"c","prompt":"p","dependencies":["b"]}
                ]
            }
            """;
        var (chatClient, sp) = BuildPlannerDeps(cyclicGraph);
        var options = sp.GetRequiredService<IOptions<LuBanAgentOptions>>();
        var planner = new LlmTaskPlanner(chatClient, sp, options);

        await Assert.ThrowsExceptionAsync<TaskPlanningException>(
            () => planner.PlanAsync("环任务"));
    }
}
