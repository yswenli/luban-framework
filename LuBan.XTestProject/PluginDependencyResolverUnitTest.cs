/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginDependencyResolverUnitTest
*描述：验证插件依赖的拓扑排序（依赖在前）、环检测、缺失依赖与重复 id 报错
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Loading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="PluginDependencyResolver"/> 测试（PRD §3.2 <c>dependencies</c>）。
/// </summary>
[TestClass]
public class PluginDependencyResolverUnitTest
{
    private readonly PluginDependencyResolver _resolver = new();

    private static PluginManifest Manifest(string id, params string[] dependencies) => new()
    {
        Id = id,
        Name = id,
        Version = "1.0.0",
        EntryAssembly = id + ".dll",
        Dependencies = dependencies.ToList()
    };

    private static int IndexOf(DependencyResolutionResult result, string id)
        => result.Ordered.Select((m, index) => (m, index))
            .FirstOrDefault(pair => string.Equals(pair.m.Id, id, StringComparison.OrdinalIgnoreCase))
            .index;

    [TestMethod]
    public void Chain_DependencyComesFirst()
    {
        var result = _resolver.Resolve(new[]
        {
            Manifest("a", "b"),
            Manifest("b", "c"),
            Manifest("c")
        });

        Assert.IsFalse(result.HasErrors, string.Join("；", result.Errors));
        Assert.AreEqual(3, result.Ordered.Count);
        Assert.IsTrue(IndexOf(result, "c") < IndexOf(result, "b"));
        Assert.IsTrue(IndexOf(result, "b") < IndexOf(result, "a"));
    }

    [TestMethod]
    public void Diamond_KeepsEachNodeOnce()
    {
        var result = _resolver.Resolve(new[]
        {
            Manifest("app", "left", "right"),
            Manifest("left", "base"),
            Manifest("right", "base"),
            Manifest("base")
        });

        Assert.IsFalse(result.HasErrors, string.Join("；", result.Errors));
        Assert.AreEqual(4, result.Ordered.Count);
        Assert.AreEqual(0, IndexOf(result, "base"));
        Assert.AreEqual(3, IndexOf(result, "app"));
    }

    [TestMethod]
    public void IndependentPlugins_AreAllOrdered()
    {
        var result = _resolver.Resolve(new[] { Manifest("a"), Manifest("b"), Manifest("c") });

        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(3, result.Ordered.Count);
    }

    [TestMethod]
    public void Cycle_ReportsError()
    {
        var result = _resolver.Resolve(new[]
        {
            Manifest("a", "b"),
            Manifest("b", "c"),
            Manifest("c", "a")
        });

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(string.Join("；", result.Errors).Contains("环"));
    }

    [TestMethod]
    public void SelfDependency_ReportsCycleError()
    {
        var result = _resolver.Resolve(new[] { Manifest("a", "a") });

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(string.Join("；", result.Errors).Contains("环"));
    }

    [TestMethod]
    public void MissingDependency_ReportsError()
    {
        var result = _resolver.Resolve(new[] { Manifest("a", "ghost") });

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(string.Join("；", result.Errors).Contains("不存在"));
    }

    [TestMethod]
    public void DuplicateId_ReportsError()
    {
        var result = _resolver.Resolve(new[] { Manifest("a"), Manifest("A") });

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(string.Join("；", result.Errors).Contains("重复"));
    }

    [TestMethod]
    public void EmptyIdAndBlankDependency_AreIgnored()
    {
        var result = _resolver.Resolve(new[]
        {
            new PluginManifest { Id = string.Empty, Name = "无 id" },
            Manifest("a", "  ")
        });

        Assert.IsFalse(result.HasErrors, string.Join("；", result.Errors));
        Assert.AreEqual(1, result.Ordered.Count);
        Assert.AreEqual("a", result.Ordered[0].Id);
    }

    [TestMethod]
    public void NullArgument_Throws()
    {
        try
        {
            _resolver.Resolve(null!);
            Assert.Fail("应抛出 ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // 预期
        }
    }

    [TestMethod]
    public void EmptyInput_ReturnsEmptyOrdered()
    {
        var result = _resolver.Resolve(Array.Empty<PluginManifest>());

        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(0, result.Ordered.Count);
    }
}