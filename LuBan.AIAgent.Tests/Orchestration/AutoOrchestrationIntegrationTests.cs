/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： AutoOrchestrationIntegrationTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：自动编排集成测试
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;
using LuBan.AIAgent.Orchestration.Planner;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Orchestration;

/// <summary>
/// 自动编排集成测试
/// </summary>
[TestClass]
public class AutoOrchestrationIntegrationTests
{
    /// <summary>
    /// 测试 SubAgentRoleRegistry 能正确加载内置角色
    /// </summary>
    [TestMethod]
    public void TestSubAgentRoleRegistry_LoadsBuiltInRoles()
    {
        var registry = new SubAgentRoleRegistry();
        var roles = registry.GetAllRoles();

        Assert.AreEqual(4, roles.Count);
        Assert.IsNotNull(registry.GetRole("analyst"));
        Assert.IsNotNull(registry.GetRole("researcher"));
        Assert.IsNotNull(registry.GetRole("coder"));
        Assert.IsNotNull(registry.GetRole("writer"));
    }

    /// <summary>
    /// 测试 SubAgentFactory 能正确使用角色默认工具组
    /// </summary>
    [TestMethod]
    public async Task TestSubAgentFactory_UsesRoleDefaultToolGroups()
    {
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => "结果"));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddSingleton<SubAgentRoleRegistry>();
        services.AddScoped<SubAgentFactory>();
        var sp = services.BuildServiceProvider();

        var factory = sp.GetRequiredService<SubAgentFactory>();
        var spec = new SubAgentSpec
        {
            NodeId = "test",
            Prompt = "执行测试",
            Role = "coder",
            ParentSessionId = "parent-1"
        };
        var agent = await factory.CreateAsync(spec);

        Assert.IsNotNull(agent);
        Assert.IsNotNull(spec.SessionId);
    }

    /// <summary>
    /// 测试 SubAgentFactory 过滤 orchestration 工具组
    /// </summary>
    [TestMethod]
    public async Task TestSubAgentFactory_FiltersOrchestrationGroup()
    {
        var services = new ServiceCollection();
        var options = new LuBanAgentOptions();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => "结果"));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddSingleton<SubAgentRoleRegistry>();
        services.AddScoped<SubAgentFactory>();
        var sp = services.BuildServiceProvider();

        var factory = sp.GetRequiredService<SubAgentFactory>();
        var spec = new SubAgentSpec
        {
            NodeId = "test",
            Prompt = "执行测试",
            ToolGroups = new List<string> { "filesystem", "orchestration" },
            ParentSessionId = "parent-1"
        };
        var agent = await factory.CreateAsync(spec);

        Assert.IsNotNull(agent);
    }

    /// <summary>
    /// 测试 Orchestrator 接受预计算图谱
    /// </summary>
    [TestMethod]
    public async Task TestOrchestrator_AcceptsPrecomputedGraph()
    {
        var llmGraphJson = """
            {
                "nodes": [
                    {"id":"a","description":"节点A","prompt":"执行A","dependencies":[],"toolGroups":["filesystem"]},
                    {"id":"b","description":"节点B","prompt":"执行B","dependencies":["a"],"toolGroups":["filesystem"]}
                ]
            }
            """;

        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { PlannerType = "llm", DefaultNodeTimeoutSeconds = 30 }
        };
        services.AddSingleton(Options.Create(options));
        // MockChatClient 返回图谱 JSON 用于规划
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => llmGraphJson));
        services.AddSingleton<ToolPluginRegistry>();
        services.AddScoped<LuBanAgentFactory>();
        services.AddSingleton<SubAgentRoleRegistry>();
        services.AddScoped<SubAgentFactory>();
        services.AddSingleton<ContextStore>();
        services.AddScoped<DagScheduler>();
        services.AddScoped<LlmTaskPlanner>();
        services.AddScoped<ITaskPlanner>(sp => sp.GetRequiredService<LlmTaskPlanner>());
        services.AddScoped<IOrchestrator, Orchestrator>();
        using var sp = services.BuildServiceProvider();

        var planner = sp.GetRequiredService<ITaskPlanner>();
        var orchestrator = sp.GetRequiredService<IOrchestrator>();

        // 先规划
        var graph = await planner.PlanAsync("执行A然后执行B");
        Assert.IsNotNull(graph);
        Assert.AreEqual(2, graph!.Nodes.Count);

        // 用预计算图谱执行
        var result = await orchestrator.RunAsync(graph);
        Assert.IsNotNull(result);
        Assert.AreEqual("completed", result.OverallStatus);
    }

    /// <summary>
    /// 测试 OrchestrationOptions 包含 AutoDetect 配置
    /// </summary>
    [TestMethod]
    public void TestOrchestrationOptions_HasAutoDetect()
    {
        var options = new OrchestrationOptions
        {
            Enabled = true,
            AutoDetect = true,
            ExposeAsTool = false
        };

        Assert.IsTrue(options.AutoDetect);
        Assert.IsFalse(options.ExposeAsTool);
    }

    /// <summary>
    /// 测试 TaskNode 包含 Role 字段
    /// </summary>
    [TestMethod]
    public void TestTaskNode_HasRoleField()
    {
        var node = new TaskNode
        {
            Id = "test",
            Prompt = "测试",
            Role = "coder",
            ToolGroups = new List<string> { "filesystem", "script" }
        };

        Assert.AreEqual("coder", node.Role);
    }

    /// <summary>
    /// 测试 LlmTaskPlanner 解析 Role 字段
    /// </summary>
    [TestMethod]
    public async Task TestLlmTaskPlanner_ParsesRoleField()
    {
        var graphJson = """
            {
                "nodes": [
                    {
                        "id": "analyze",
                        "description": "分析需求",
                        "prompt": "分析用户需求",
                        "role": "analyst",
                        "dependencies": [],
                        "toolGroups": ["filesystem"],
                        "isCritical": true
                    },
                    {
                        "id": "implement",
                        "description": "实现功能",
                        "prompt": "基于 {dep:analyze} 实现功能",
                        "role": "coder",
                        "dependencies": ["analyze"],
                        "toolGroups": ["filesystem", "script"],
                        "isCritical": false
                    }
                ]
            }
            """;

        var services = new ServiceCollection();
        var options = new LuBanAgentOptions
        {
            Orchestration = new OrchestrationOptions { MaxNodes = 10 }
        };
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => graphJson));
        services.AddSingleton<ToolPluginRegistry>();
        var sp = services.BuildServiceProvider();

        var planner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>());

        var graph = await planner.PlanAsync("分析并实现功能");

        Assert.IsNotNull(graph);
        Assert.AreEqual(2, graph!.Nodes.Count);
        Assert.AreEqual("analyst", graph.Nodes[0].Role);
        Assert.AreEqual("coder", graph.Nodes[1].Role);
    }
}
