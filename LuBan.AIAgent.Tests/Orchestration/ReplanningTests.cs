/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： ReplanningTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：DAG 动态重规划单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;
using LuBan.AIAgent.Plugins;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class ReplanningTests
{
    private static ServiceProvider BuildServiceProvider(
        string llmGraphJson,
        string? reflectionJson = null,
        int maxReplanAttempts = 3,
        Func<IEnumerable<ChatMessage>, string>? agentResponder = null)
    {
        var callCount = 0;
        var mockClient = new MockChatClient("test", messages =>
        {
            callCount++;
            var lastMsg = messages.LastOrDefault()?.Text ?? "";

            if (lastMsg.Contains("任务失败分析专家"))
            {
                return reflectionJson ?? """
                    {"analysis":"失败分析","fix_approach":"修复方案","should_retry":false,"new_nodes":[]}
                    """;
            }

            return llmGraphJson;
        });

        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions
            {
                PlannerType = "llm",
                DefaultNodeTimeoutSeconds = 30,
                MaxReplanAttempts = maxReplanAttempts,
                ReflectionTimeoutSeconds = 10
            }
        };
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(mockClient);
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddScoped<SubAgentFactory>();
        services.AddSingleton<ContextStore>();
        services.AddScoped<DagScheduler>();
        services.AddScoped<LlmTaskPlanner>();
        services.AddScoped<ITaskPlanner>(sp => sp.GetRequiredService<LlmTaskPlanner>());
        services.AddScoped<IOrchestrator, Orchestrator>();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task RunAsync_成功时不触发反思()
    {
        var graphJson = """
            {"nodes":[{"id":"a","description":"A","prompt":"执行A","dependencies":[],"isCritical":true}]}
            """;

        using var sp = BuildServiceProvider(graphJson);
        var orchestrator = sp.GetRequiredService<IOrchestrator>();

        var result = await orchestrator.RunAsync("测试任务");

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(0, result.ReplanningAttempts);
        Assert.IsNull(result.Reflection);
        Assert.IsFalse(result.ReplanningExhausted);
    }

    [TestMethod]
    public async Task RunAsync_MaxReplanAttempts为0时不重规划()
    {
        var graphJson = """
            {"nodes":[{"id":"a","description":"A","prompt":"执行A","dependencies":[],"isCritical":true}]}
            """;

        using var sp = BuildServiceProvider(graphJson, maxReplanAttempts: 0);
        var orchestrator = sp.GetRequiredService<IOrchestrator>();

        var result = await orchestrator.RunAsync("测试任务");

        Assert.AreEqual(0, result.ReplanningAttempts);
        Assert.IsNull(result.Reflection);
    }

    [TestMethod]
    public void ReplanContext_属性正确()
    {
        var context = new ReplanContext
        {
            UserGoal = "测试任务",
            Attempt = 2,
            OriginalGraph = new TaskGraph { OriginalTask = "测试" },
            FailedNodes = new List<FailedNodeInfo>
            {
                new() { NodeId = "a", Error = "失败", Description = "节点A" }
            }
        };

        Assert.AreEqual("测试任务", context.UserGoal);
        Assert.AreEqual(2, context.Attempt);
        Assert.AreEqual(1, context.FailedNodes.Count);
        Assert.AreEqual("a", context.FailedNodes[0].NodeId);
    }

    [TestMethod]
    public void ReflectionResult_默认值正确()
    {
        var result = new ReflectionResult();

        Assert.AreEqual("", result.Analysis);
        Assert.AreEqual("", result.FixApproach);
        Assert.IsFalse(result.ShouldRetry);
        Assert.AreEqual(0, result.FailedNodeIds.Count);
        Assert.AreEqual(0, result.NewNodes.Count);
    }

    [TestMethod]
    public void FailedNodeInfo_DependencyOutputs默认空()
    {
        var info = new FailedNodeInfo { NodeId = "test" };

        Assert.AreEqual(0, info.DependencyOutputs.Count);
        Assert.IsNull(info.Error);
        Assert.IsNull(info.Output);
    }

    [TestMethod]
    public void OrchestrationResult_重规划字段默认值()
    {
        var result = new OrchestrationResult();

        Assert.AreEqual(0, result.ReplanningAttempts);
        Assert.IsNull(result.Reflection);
        Assert.IsFalse(result.ReplanningExhausted);
    }

    [TestMethod]
    public async Task LlmTaskPlanner_ReflectAsync_解析JSON响应()
    {
        var reflectionJson = """
            {
                "analysis": "节点a因超时失败",
                "fix_approach": "增加超时时间重试",
                "should_retry": true,
                "new_nodes": [
                    {
                        "id": "retry_a",
                        "description": "重试节点A",
                        "prompt": "执行A（增加超时）",
                        "dependencies": [],
                        "toolGroups": ["web"],
                        "isCritical": true
                    }
                ]
            }
            """;

        var mockClient = new MockChatClient("test", _ => reflectionJson);
        var services = new ServiceCollection();
        services.AddSingleton<IChatClient>(mockClient);
        services.AddSingleton(Options.Create(new LuBanAgentOptions()));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LlmTaskPlanner>();

        using var sp = services.BuildServiceProvider();
        var planner = sp.GetRequiredService<LlmTaskPlanner>();

        var context = new ReplanContext
        {
            UserGoal = "测试",
            Attempt = 1,
            FailedNodes = new List<FailedNodeInfo>
            {
                new() { NodeId = "a", Error = "超时" }
            }
        };

        var result = await planner.ReflectAsync(context);

        Assert.IsTrue(result.ShouldRetry);
        Assert.AreEqual("节点a因超时失败", result.Analysis);
        Assert.AreEqual("增加超时时间重试", result.FixApproach);
        Assert.AreEqual(1, result.NewNodes.Count);
        Assert.AreEqual("retry_a", result.NewNodes[0].Id);
        Assert.IsTrue(result.NewNodes[0].IsCritical);
        Assert.AreEqual(1, result.NewNodes[0].ToolGroups?.Count);
        Assert.AreEqual("web", result.NewNodes[0].ToolGroups![0]);
    }

    [TestMethod]
    public async Task LlmTaskPlanner_ReflectAsync_空响应抛异常()
    {
        var mockClient = new MockChatClient("test", _ => "");
        var services = new ServiceCollection();
        services.AddSingleton<IChatClient>(mockClient);
        services.AddSingleton(Options.Create(new LuBanAgentOptions()));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LlmTaskPlanner>();

        using var sp = services.BuildServiceProvider();
        var planner = sp.GetRequiredService<LlmTaskPlanner>();

        var context = new ReplanContext { UserGoal = "测试", Attempt = 1 };

        await Assert.ThrowsExceptionAsync<TaskPlanningException>(
            () => planner.ReflectAsync(context));
    }

    [TestMethod]
    public async Task TemplateTaskPlanner_ReflectAsync_返回不重试()
    {
        var planner = new TemplateTaskPlanner(Enumerable.Empty<TaskGraphTemplate>());

        var context = new ReplanContext
        {
            UserGoal = "测试",
            Attempt = 1,
            FailedNodes = new List<FailedNodeInfo>
            {
                new() { NodeId = "a", Error = "失败" }
            }
        };

        var result = await planner.ReflectAsync(context);

        Assert.IsFalse(result.ShouldRetry);
        Assert.AreEqual(1, result.FailedNodeIds.Count);
    }
}
