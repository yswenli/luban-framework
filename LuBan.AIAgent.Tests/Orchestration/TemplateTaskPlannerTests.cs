/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： TemplateTaskPlannerTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：TemplateTaskPlanner 和 CompositeTaskPlanner 单元测试
*
*****************************************************************************/
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
public class TemplateTaskPlannerTests
{
    private static TaskGraphTemplate CreateSampleTemplate()
    {
        return new TaskGraphTemplate
        {
            Id = "code-review",
            Name = "代码审查",
            Description = "审查指定代码文件",
            Keywords = new[] { "代码审查", "code review", "审查" },
            Prototype = new TaskGraph
            {
                Nodes = new()
                {
                    new() { Id = "read", Description = "读取文件", Prompt = "读取文件 {param:path}", Dependencies = new() },
                    new() { Id = "review", Description = "审查代码", Prompt = "审查 {dep:read} 的代码质量", Dependencies = new() { "read" } }
                }
            },
            Parameters = new()
            {
                new() { Name = "path", Description = "文件路径", Required = true }
            }
        };
    }

    [TestMethod]
    public void TestInstantiate_参数替换正确()
    {
        var template = CreateSampleTemplate();
        var graph = template.Instantiate(new Dictionary<string, string>
        {
            ["path"] = "src/Program.cs"
        });

        Assert.AreEqual("template", graph.Source);
        Assert.AreEqual(2, graph.Nodes.Count);
        Assert.IsTrue(graph.Nodes[0].Prompt.Contains("src/Program.cs"));
    }

    [TestMethod]
    public async Task TestTemplateTaskPlanner_命中模板返回图谱()
    {
        var template = CreateSampleTemplate();
        var planner = new TemplateTaskPlanner(new[] { template });

        var graph = await planner.PlanAsync("请对代码审查 src/Main.cs");

        Assert.IsNotNull(graph);
        Assert.AreEqual("template", graph!.Source);
    }

    [TestMethod]
    public async Task TestTemplateTaskPlanner_未命中返回Null()
    {
        var template = CreateSampleTemplate();
        var planner = new TemplateTaskPlanner(new[] { template });

        var graph = await planner.PlanAsync("今天天气怎么样");

        Assert.IsNull(graph);
    }

    [TestMethod]
    public async Task TestCompositeTaskPlanner_模板命中走模板()
    {
        var template = CreateSampleTemplate();
        var templatePlanner = new TemplateTaskPlanner(new[] { template });

        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new LuBanAgentOptions()));
        services.AddSingleton<IChatClient>(new MockChatClient("不应该走到这里"));
        services.AddSingleton<ToolPluginRegistry>();
        var sp = services.BuildServiceProvider();

        var llmPlanner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>());
        var composite = new CompositeTaskPlanner(templatePlanner, llmPlanner);

        var graph = await composite.PlanAsync("代码审查 src/Program.cs");

        Assert.IsNotNull(graph);
        Assert.AreEqual("template", graph!.Source);
    }

    [TestMethod]
    public async Task TestCompositeTaskPlanner_模板未命中回退LLM()
    {
        var template = CreateSampleTemplate();
        var templatePlanner = new TemplateTaskPlanner(new[] { template });

        var llmResponse = """
            {
                "nodes": [
                    {"id":"a","description":"a","prompt":"p","dependencies":[]}
                ]
            }
            """;
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new LuBanAgentOptions()));
        services.AddSingleton<IChatClient>(new MockChatClient("test", _ => llmResponse));
        services.AddSingleton<ToolPluginRegistry>();
        var sp = services.BuildServiceProvider();

        var llmPlanner = new LlmTaskPlanner(
            sp.GetRequiredService<IChatClient>(),
            sp,
            sp.GetRequiredService<IOptions<LuBanAgentOptions>>());
        var composite = new CompositeTaskPlanner(templatePlanner, llmPlanner);

        var graph = await composite.PlanAsync("一个完全无关的任务");

        Assert.IsNotNull(graph);
        Assert.AreEqual("llm", graph!.Source);
    }

    private static string CreateTempWorkspace()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [TestMethod]
    public async Task TestLoadFromWorkspace_加载模板并命中()
    {
        var workspace = CreateTempWorkspace();
        try
        {
            var plansDir = Path.Combine(workspace, ".luban-agent", "plans");
            Directory.CreateDirectory(plansDir);
            File.WriteAllText(Path.Combine(plansDir, "code-review.json"), """
            {
              "name": "code-review",
              "keywords": ["代码审查", "code review"],
              "graph": {
                "nodes": [
                  { "id": "analyze", "description": "分析代码", "prompt": "分析代码结构", "role": "analyst", "toolGroups": ["filesystem"], "dependencies": [], "isCritical": true },
                  { "id": "review", "description": "审查意见", "prompt": "基于 {dep:analyze} 给出审查意见", "role": "coder", "toolGroups": ["filesystem"], "dependencies": ["analyze"], "isCritical": false }
                ]
              }
            }
            """);

            var planner = new TemplateTaskPlanner(Array.Empty<TaskGraphTemplate>());
            var loaded = planner.LoadFromWorkspace(workspace);

            Assert.AreEqual(1, loaded);
            var graph = await planner.PlanAsync("请做一次代码审查");
            Assert.IsNotNull(graph);
            Assert.AreEqual(2, graph!.Nodes.Count);
            Assert.AreEqual("analyst", graph.Nodes[0].Role);
        }
        finally { Directory.Delete(workspace, true); }
    }

    [TestMethod]
    public void TestLoadFromWorkspace_目录不存在返回0()
    {
        var planner = new TemplateTaskPlanner(Array.Empty<TaskGraphTemplate>());
        var loaded = planner.LoadFromWorkspace(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        Assert.AreEqual(0, loaded);
    }

    [TestMethod]
    public void TestLoadFromWorkspace_无效JSON被容忍()
    {
        var workspace = CreateTempWorkspace();
        try
        {
            var plansDir = Path.Combine(workspace, ".luban-agent", "plans");
            Directory.CreateDirectory(plansDir);
            File.WriteAllText(Path.Combine(plansDir, "bad.json"), "{ not valid json !!!");

            var planner = new TemplateTaskPlanner(Array.Empty<TaskGraphTemplate>());
            var loaded = planner.LoadFromWorkspace(workspace);
            Assert.AreEqual(0, loaded);
        }
        finally { Directory.Delete(workspace, true); }
    }
}
