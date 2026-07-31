/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： ContextStoreTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：ContextStore 单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class ContextStoreTests
{
    [TestMethod]
    public void TestSetOutput_GetOutput_读写一致()
    {
        var store = new ContextStore();
        store.SetOutput("graph1", "nodeA", "输出内容");
        Assert.AreEqual("输出内容", store.GetOutput("graph1", "nodeA"));
    }

    [TestMethod]
    public void TestGetOutput_不存在返回Null()
    {
        var store = new ContextStore();
        Assert.IsNull(store.GetOutput("graph1", "nodeA"));
    }

    [TestMethod]
    public void TestResolvePlaceholders_替换dep占位符()
    {
        var store = new ContextStore();
        var graph = new TaskGraph
        {
            GraphId = "g1",
            Nodes = new()
            {
                new() { Id = "a", Status = TaskNodeStatus.Succeeded },
                new() { Id = "b", Dependencies = new() { "a" } }
            }
        };
        store.SetOutput("g1", "a", "前驱结果");
        var node = graph.Nodes[1];
        var resolved = store.ResolvePlaceholders("基于 {dep:a} 进行分析", graph, node);
        Assert.AreEqual("基于 前驱结果 进行分析", resolved);
    }

    [TestMethod]
    public void TestResolvePlaceholders_引用不存在的dep返回占位文本()
    {
        var store = new ContextStore();
        var graph = new TaskGraph
        {
            GraphId = "g1",
            Nodes = new()
            {
                new() { Id = "a", Dependencies = new() { "x" } }
            }
        };
        var node = graph.Nodes[0];
        var resolved = store.ResolvePlaceholders("引用 {dep:x}", graph, node);
        Assert.IsTrue(resolved.Contains("无输出"));
    }

    [TestMethod]
    public void TestResolvePlaceholders_引用失败的dep返回失败信息()
    {
        var store = new ContextStore();
        var graph = new TaskGraph
        {
            GraphId = "g1",
            Nodes = new()
            {
                new() { Id = "a", Status = TaskNodeStatus.Failed, Error = "超时" },
                new() { Id = "b", Dependencies = new() { "a" } }
            }
        };
        var node = graph.Nodes[1];
        var resolved = store.ResolvePlaceholders("基于 {dep:a}", graph, node);
        Assert.IsTrue(resolved.Contains("失败"));
        Assert.IsTrue(resolved.Contains("超时"));
    }

    [TestMethod]
    public void TestClear_清理后Get返回Null()
    {
        var store = new ContextStore();
        store.SetOutput("g1", "a", "数据");
        store.Clear("g1");
        Assert.IsNull(store.GetOutput("g1", "a"));
    }
}
