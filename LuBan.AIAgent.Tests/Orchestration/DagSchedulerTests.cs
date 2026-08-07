/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： DagSchedulerTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：DagScheduler 单元测试
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Tests;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class DagSchedulerTests
{
    /// <summary>
    /// 构建服务提供者。responder 为 null 时返回固定字符串，否则使用自定义 responder。
    /// </summary>
    private static ServiceProvider BuildServiceProvider(Func<IEnumerable<ChatMessage>, string>? responder = null)
    {
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { DefaultNodeTimeoutSeconds = 30 }
        };
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(responder == null
            ? new MockChatClient("test", _ => "节点结果")
            : new MockChatClient("test", responder));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddSingleton<SubAgentRoleRegistry>();
        services.AddScoped<SubAgentFactory>();
        services.AddSingleton<ContextStore>();
        services.AddScoped<DagScheduler>();
        return services.BuildServiceProvider();
    }

    private static TaskGraph BuildSimpleGraph()
    {
        return new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Description = "节点A", Prompt = "执行A", Dependencies = new(), ToolGroups = new() { "filesystem" } },
                new() { Id = "b", Description = "节点B", Prompt = "执行B", Dependencies = new() { "a" }, ToolGroups = new() { "filesystem" } }
            }
        };
    }

    [TestMethod]
    public async Task TestExecuteAsync_全成功返回Completed()
    {
        using var sp = BuildServiceProvider();
        var scheduler = sp.GetRequiredService<DagScheduler>();
        var graph = BuildSimpleGraph();

        var result = await scheduler.ExecuteAsync(graph);

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(2, result.Nodes.Count);
        Assert.IsTrue(result.Nodes.All(n => n.Status == TaskNodeStatus.Succeeded));
    }

    [TestMethod]
    public async Task TestExecuteAsync_全并行图谱单层执行()
    {
        using var sp = BuildServiceProvider();
        var scheduler = sp.GetRequiredService<DagScheduler>();
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Prompt = "A", Dependencies = new(), ToolGroups = new() { "filesystem" } },
                new() { Id = "b", Prompt = "B", Dependencies = new(), ToolGroups = new() { "filesystem" } },
                new() { Id = "c", Prompt = "C", Dependencies = new(), ToolGroups = new() { "filesystem" } }
            }
        };

        var result = await scheduler.ExecuteAsync(graph);

        Assert.AreEqual("completed", result.OverallStatus);
        Assert.AreEqual(3, result.Nodes.Count);
    }

    [TestMethod]
    public async Task TestExecuteAsync_关键节点失败后继跳过()
    {
        using var sp = BuildServiceProvider(_ => throw new InvalidOperationException("模拟节点失败"));
        var scheduler = sp.GetRequiredService<DagScheduler>();
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Prompt = "A", Dependencies = new(), IsCritical = true, ToolGroups = new() { "filesystem" } },
                new() { Id = "b", Prompt = "B", Dependencies = new() { "a" }, ToolGroups = new() { "filesystem" } }
            }
        };

        var result = await scheduler.ExecuteAsync(graph);

        Assert.AreEqual("failed", result.OverallStatus);
        var nodeB = result.Nodes.First(n => n.NodeId == "b");
        Assert.AreEqual(TaskNodeStatus.Skipped, nodeB.Status);
    }

    [TestMethod]
    public async Task TestExecuteAsync_非关键节点失败继续执行()
    {
        var callCount = 0;
        using var sp = BuildServiceProvider(_ =>
        {
            callCount++;
            if (callCount == 1) throw new InvalidOperationException("模拟节点A失败");
            return "节点B结果";
        });
        var scheduler = sp.GetRequiredService<DagScheduler>();
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Prompt = "A", Dependencies = new(), IsCritical = false, ToolGroups = new() { "filesystem" } },
                new() { Id = "b", Prompt = "B", Dependencies = new() { "a" }, ToolGroups = new() { "filesystem" } }
            }
        };

        var result = await scheduler.ExecuteAsync(graph);

        Assert.AreEqual("partial", result.OverallStatus);
        var nodeA = result.Nodes.First(n => n.NodeId == "a");
        Assert.AreEqual(TaskNodeStatus.Failed, nodeA.Status);
        var nodeB = result.Nodes.First(n => n.NodeId == "b");
        Assert.AreEqual(TaskNodeStatus.Succeeded, nodeB.Status);
    }
}
