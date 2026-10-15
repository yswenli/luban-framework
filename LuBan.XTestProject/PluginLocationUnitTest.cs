/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginLocationUnitTest
*描述：验证插件目录解析：仅应用级（无用户级覆盖）、清单/入口程序集定位与 lib 优先回退
*
*****************************************************************************/
using LuBan.AIAgent.Plugins.Loading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="PluginLocation"/> 测试（PRD §3.3：插件只有应用级）。
/// </summary>
[TestClass]
public class PluginLocationUnitTest
{
    [TestMethod]
    public void BuiltinRoot_IsPluginsUnderAppBase()
    {
        Assert.AreEqual(
            Path.Combine(AppContext.BaseDirectory, "plugins"),
            PluginLocation.BuiltinRoot);
    }

    [TestMethod]
    public void ResolveDirectories_OnlyAppLevel_NeverUserLevel()
    {
        var directories = PluginLocation.ResolveDirectories();

        // 回归守卫：插件目录只认应用级，任何用户级覆盖（~/.luban-agent/plugins、%LocalAppData%）都不允许
        // 出现在解析结果中（PRD §3.3 v0.16 定案：用户级覆盖本期不实现）。
        var unexpected = directories
            .Where(directory => !string.Equals(directory, PluginLocation.BuiltinRoot, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.AreEqual(0, unexpected.Length, $"出现非应用级插件目录：{string.Join(" | ", unexpected)}");
        Assert.IsTrue(directories.Count <= 1);
    }

    [TestMethod]
    public void ResolveDirectories_ReturnsRootOnlyWhenItExists()
    {
        var directories = PluginLocation.ResolveDirectories();

        Assert.AreEqual(Directory.Exists(PluginLocation.BuiltinRoot) ? 1 : 0, directories.Count);
    }

    [TestMethod]
    public void ResolvePluginDirectory_FindsDirectoryWithManifest()
    {
        using var root = PluginTestDirectory.Create();
        root.WriteFile(Path.Combine("com.tjc.demo", "plugin.json"), "{}");

        var resolved = PluginLocation.ResolvePluginDirectory("com.tjc.demo", new[] { root.Path });

        Assert.AreEqual(Path.Combine(root.Path, "com.tjc.demo"), resolved);
    }

    [TestMethod]
    public void ResolvePluginDirectory_IgnoresDirectoryWithoutManifest()
    {
        using var root = PluginTestDirectory.Create();
        root.CreateDirectory("com.tjc.demo");

        Assert.IsNull(PluginLocation.ResolvePluginDirectory("com.tjc.demo", new[] { root.Path }));
    }

    [TestMethod]
    public void ResolvePluginDirectory_FirstRootWins()
    {
        using var first = PluginTestDirectory.Create();
        using var second = PluginTestDirectory.Create();
        first.WriteFile(Path.Combine("com.tjc.demo", "plugin.json"), """{"id":"first"}""");
        second.WriteFile(Path.Combine("com.tjc.demo", "plugin.json"), """{"id":"second"}""");

        var resolved = PluginLocation.ResolvePluginDirectory("com.tjc.demo", new[] { first.Path, second.Path });

        Assert.AreEqual(Path.Combine(first.Path, "com.tjc.demo"), resolved);
    }

    [TestMethod]
    public void ResolvePluginDirectory_BlankIdOrMissingRoot_ReturnsNull()
    {
        using var root = PluginTestDirectory.Create();

        Assert.IsNull(PluginLocation.ResolvePluginDirectory("  ", new[] { root.Path }));
        Assert.IsNull(PluginLocation.ResolvePluginDirectory("com.tjc.demo", new[] { root.Path }));
        Assert.IsNull(PluginLocation.ResolvePluginDirectory("com.tjc.demo", Array.Empty<string>()));
    }

    [TestMethod]
    public void ResolveEntryAssemblyPath_PrefersLibFolder()
    {
        using var directory = PluginTestDirectory.Create();
        directory.WriteFile("Demo.dll", "root");
        directory.WriteFile(Path.Combine("lib", "Demo.dll"), "lib");

        Assert.AreEqual(
            Path.Combine(directory.Path, "lib", "Demo.dll"),
            PluginLocation.ResolveEntryAssemblyPath(directory.Path, "Demo.dll"));
    }

    [TestMethod]
    public void ResolveEntryAssemblyPath_FallsBackToPluginRoot()
    {
        using var directory = PluginTestDirectory.Create();
        directory.WriteFile("Demo.dll", "root");

        Assert.AreEqual(
            Path.Combine(directory.Path, "Demo.dll"),
            PluginLocation.ResolveEntryAssemblyPath(directory.Path, "Demo.dll"));
    }

    [TestMethod]
    public void ResolveEntryAssemblyPath_Missing_ReturnsNull()
    {
        using var directory = PluginTestDirectory.Create();

        Assert.IsNull(PluginLocation.ResolveEntryAssemblyPath(directory.Path, "Demo.dll"));
        Assert.IsNull(PluginLocation.ResolveEntryAssemblyPath(directory.Path, null));
        Assert.IsNull(PluginLocation.ResolveEntryAssemblyPath(string.Empty, "Demo.dll"));
    }

    [TestMethod]
    public void GetManifestPath_CombinesFileName()
    {
        Assert.AreEqual(
            Path.Combine("D:\\plugins\\com.tjc.demo", "plugin.json"),
            PluginLocation.GetManifestPath("D:\\plugins\\com.tjc.demo"));
    }

    [TestMethod]
    public void DirectoryConventionNames_MatchPrd()
    {
        var names = string.Join(
            "|",
            PluginLocation.ManifestFileName,
            PluginLocation.LibraryDirectoryName,
            PluginLocation.AssetsDirectoryName);

        Assert.AreEqual("plugin.json|lib|assets", names);
    }
}