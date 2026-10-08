/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： GraphPlanStoreUnitTest
*唯一标识：GraphPlanStore 暂存单测
*创建时间：2026/9/23
*描述：验证图谱写取、TTL 过期、容量淘汰与并发安全（纯单元，不访问网络）
*
*****************************************************************************/
using System.Reflection;

using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.XTestProject;

[TestClass]
public class GraphPlanStoreUnitTest
{
    private static TaskGraph BuildGraph(string? graphId = null) => new()
    {
        GraphId = graphId ?? Guid.NewGuid().ToString("N"),
        OriginalTask = "复合任务",
        Nodes =
        [
            new TaskNode { Id = "n0", Description = "第一步", Prompt = "p0" },
            new TaskNode { Id = "n1", Description = "第二步", Prompt = "p1", Dependencies = ["n0"] }
        ]
    };

    /// <summary>
    /// 把条目过期时刻改为 <paramref name="age"/> 之前，用于在不等待真实 TTL 的前提下验证懒清理。
    /// </summary>
    private static void AgeEntry(GraphPlanStore store, string graphId, TimeSpan age)
    {
        var field = typeof(GraphPlanStore).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = field.GetValue(store)!;
        var entry = dict.GetType().GetProperty("Item")!.GetValue(dict, [graphId])!;
        var entryType = entry.GetType();
        var graph = entryType.GetProperty("Graph")!.GetValue(entry);
        var aged = entryType.GetConstructors()[0].Invoke([graph!, DateTime.UtcNow - age]);
        ((System.Collections.IDictionary)dict)[graphId] = aged;
    }

    [TestMethod]
    public void Save_ReturnsGraphId_AndGetReturnsSameGraph()
    {
        var store = new GraphPlanStore();
        var graph = BuildGraph();

        var graphId = store.Save(graph);

        Assert.AreEqual(graph.GraphId, graphId);
        Assert.AreSame(graph, store.Get(graphId));
    }

    [TestMethod]
    public void Get_UnknownId_ReturnsNull()
    {
        var store = new GraphPlanStore();

        Assert.IsNull(store.Get("not-exist"));
    }

    [TestMethod]
    public void Get_NullOrEmpty_ReturnsNull()
    {
        var store = new GraphPlanStore();
        store.Save(BuildGraph());

        Assert.IsNull(store.Get(null!));
        Assert.IsNull(store.Get(""));
    }

    [TestMethod]
    public void Remove_ThenGet_ReturnsNull()
    {
        var store = new GraphPlanStore();
        var graphId = store.Save(BuildGraph());

        store.Remove(graphId);

        Assert.IsNull(store.Get(graphId));
        Assert.AreEqual(0, store.Count);
    }

    [TestMethod]
    public void Get_ExpiredEntry_ReturnsNull_AndCleansUp()
    {
        var store = new GraphPlanStore();
        var graphId = store.Save(BuildGraph());

        AgeEntry(store, graphId, GraphPlanStore.Ttl + TimeSpan.FromMinutes(1));

        Assert.IsNull(store.Get(graphId));
        Assert.AreEqual(0, store.Count);
    }

    [TestMethod]
    public void Save_ExceedsCapacity_EvictsOldest()
    {
        var store = new GraphPlanStore();
        var ids = new List<string>();
        var total = GraphPlanStore.MaxEntries + 3;

        for (var i = 0; i < total; i++)
        {
            ids.Add(store.Save(BuildGraph()));
            Thread.Sleep(2); // 拉开过期时刻，保证淘汰顺序即插入顺序
        }

        Assert.AreEqual(GraphPlanStore.MaxEntries, store.Count);
        foreach (var evicted in ids.Take(total - GraphPlanStore.MaxEntries))
            Assert.IsNull(store.Get(evicted), $"最旧条目应被淘汰: {evicted}");
        foreach (var kept in ids.Skip(total - GraphPlanStore.MaxEntries))
            Assert.IsNotNull(store.Get(kept), $"最新条目应保留: {kept}");
    }

    [TestMethod]
    public void SaveAndGet_Concurrent_DoesNotThrow()
    {
        var store = new GraphPlanStore();

        Parallel.For(0, 200, _ =>
        {
            var graph = BuildGraph();
            var graphId = store.Save(graph);
            store.Get(graphId);
            store.Remove(graphId);
        });

        Assert.IsTrue(store.Count <= GraphPlanStore.MaxEntries);
    }
}