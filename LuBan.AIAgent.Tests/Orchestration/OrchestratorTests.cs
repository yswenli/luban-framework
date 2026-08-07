/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： OrchestratorTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：Orchestrator 门面单元测试
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;
using LuBan.AIAgent.Tests;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class OrchestratorTests
{
    private static ServiceProvider BuildServiceProvider(string llmResponse)
    {
        var llmGraphJson = """
            {
                "nodes": [
                    {"id":"a","description":"节点A","prompt":"执行A","dependencies":[]},
                    {"id":"b","description":"节点B","prompt":"执行B","dependencies":["a"]}
                ]
            }
            """;

        var callCount = 0;
        var mockClient = new MockChatClient("test", _ =>
        {
            callCount++;
            return callCount == 1 ? llmGraphJson : llmResponse;
        });

        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { PlannerType = "llm", DefaultNodeTimeoutSeconds = 30 }
        };
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(mockClient);
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddSingleton<SubAgentRoleRegistry>();
        services.AddScoped<SubAgentFactory>();
        services.AddSingleton<ContextStore>();
        services.AddScoped<DagScheduler>();
        services.AddScoped<LlmTaskPlanner>();
        services.AddScoped<ITaskPlanner>(sp => sp.GetRequiredService<LlmTaskPlanner>());
        services.AddScoped<IOrchestrator, Orchestrator>();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TestRunAsync_完整流程()
    {
        using var sp = BuildServiceProvider("节点执行结果");
        var orchestrator = sp.GetRequiredService<IOrchestrator>();

        var result = await orchestrator.RunAsync("执行A然后执行B");

        Assert.IsNotNull(result);
        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(2, result.Nodes.Count);
        Assert.IsFalse(string.IsNullOrEmpty(result.FinalOutput));
    }

    [TestMethod]
    public async Task TestRunAsync_空任务抛异常()
    {
        using var sp = BuildServiceProvider("结果");
        var orchestrator = sp.GetRequiredService<IOrchestrator>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => orchestrator.RunAsync(""));
    }

    [TestMethod]
    public async Task TestRunStreamingAsync_推送进度事件()
    {
        using var sp = BuildServiceProvider("流式结果");
        var orchestrator = sp.GetRequiredService<IOrchestrator>();

        var events = new List<OrchestrationProgress>();
        await foreach (var p in orchestrator.RunStreamingAsync("流式任务"))
        {
            events.Add(p);
        }

        Assert.IsTrue(events.Count > 0);
        Assert.IsTrue(events.Any(e => e.EventType == ProgressEventType.PlanningStarted));
        Assert.IsTrue(events.Any(e => e.EventType == ProgressEventType.OrchestratingCompleted));
    }
}
