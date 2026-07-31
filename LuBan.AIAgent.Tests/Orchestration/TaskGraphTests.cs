/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： TaskGraphTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：TaskGraph DAG 数据结构单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class TaskGraphTests
{
    [TestMethod]
    public void TestTaskGraph_无环合法返回True()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Dependencies = new() },
                new() { Id = "b", Dependencies = new() { "a" } }
            }
        };
        Assert.IsTrue(graph.Validate(out var errors));
        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void TestTaskGraph_有环返回False()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Dependencies = new() { "c" } },
                new() { Id = "b", Dependencies = new() { "a" } },
                new() { Id = "c", Dependencies = new() { "b" } }
            }
        };
        Assert.IsFalse(graph.Validate(out var errors));
        Assert.IsTrue(errors.Any(e => e.Contains("环")));
    }

    [TestMethod]
    public void TestTaskGraph_依赖缺失返回错误()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a", Dependencies = new() { "x" } }
            }
        };
        Assert.IsFalse(graph.Validate(out var errors));
        Assert.IsTrue(errors.Any(e => e.Contains("x")));
    }

    [TestMethod]
    public void TestTaskGraph_重复ID返回错误()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a" },
                new() { Id = "a" }
            }
        };
        Assert.IsFalse(graph.Validate(out var errors));
        Assert.IsTrue(errors.Any(e => e.Contains("重复")));
    }

    [TestMethod]
    public void TestTaskGraph_空图谱返回False()
    {
        var graph = new TaskGraph();
        Assert.IsFalse(graph.Validate(out var errors));
        Assert.IsTrue(errors.Count > 0);
    }

    [TestMethod]
    public void TestGetTopologicalLayers_线性链返回3层()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a" },
                new() { Id = "b", Dependencies = new() { "a" } },
                new() { Id = "c", Dependencies = new() { "b" } }
            }
        };
        var layers = graph.GetTopologicalLayers();
        Assert.AreEqual(3, layers.Count);
        Assert.AreEqual("a", layers[0][0].Id);
        Assert.AreEqual("b", layers[1][0].Id);
        Assert.AreEqual("c", layers[2][0].Id);
    }

    [TestMethod]
    public void TestGetTopologicalLayers_全并行返回1层()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a" },
                new() { Id = "b" },
                new() { Id = "c" }
            }
        };
        var layers = graph.GetTopologicalLayers();
        Assert.AreEqual(1, layers.Count);
        Assert.AreEqual(3, layers[0].Count);
    }

    [TestMethod]
    public void TestGetTopologicalLayers_菱形返回3层()
    {
        var graph = new TaskGraph
        {
            Nodes = new()
            {
                new() { Id = "a" },
                new() { Id = "b", Dependencies = new() { "a" } },
                new() { Id = "c", Dependencies = new() { "a" } },
                new() { Id = "d", Dependencies = new() { "b", "c" } }
            }
        };
        var layers = graph.GetTopologicalLayers();
        Assert.AreEqual(3, layers.Count);
        Assert.AreEqual(1, layers[0].Count);
        Assert.AreEqual(2, layers[1].Count);
        Assert.AreEqual(1, layers[2].Count);
    }

    [TestMethod]
    public void TestOrchestrationOptions_默认值正确()
    {
        var opts = new OrchestrationOptions();
        Assert.IsTrue(opts.Enabled);
        Assert.AreEqual("composite", opts.PlannerType);
        Assert.AreEqual(120, opts.DefaultNodeTimeoutSeconds);
        Assert.AreEqual(4, opts.MaxParallelism);
        Assert.AreEqual(10, opts.MaxNodes);
        Assert.AreEqual("Templates", opts.TemplatesDirectory);
        Assert.IsTrue(opts.ExposeAsTool);
    }

    [TestMethod]
    public void TestLuBanAgentOptions_包含Orchestration字段()
    {
        var opts = new LuBanAgentOptions();
        Assert.IsNull(opts.Orchestration);
        opts.Orchestration = new OrchestrationOptions();
        Assert.IsNotNull(opts.Orchestration);
    }
}
