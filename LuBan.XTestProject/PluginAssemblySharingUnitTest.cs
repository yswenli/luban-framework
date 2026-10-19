/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginAssemblySharingUnitTest
*描述：验证插件 ALC 共享程序集策略：默认值与历史硬编码集合一致、可追加、规范化（W2）
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="PluginAssemblySharing"/> 兼容性与追加语义测试。
/// </summary>
/// <remarks>
/// 默认值必须与框架历史硬编码集合一致（否则插件加载会因双份 Type 崩坏）；
/// 宿主只能<b>追加</b>产品 / 厂商程序集，不能削弱必备集合。
/// </remarks>
[TestClass]
public class PluginAssemblySharingUnitTest
{
    [TestMethod]
    public void Default_MatchesHistoricalHardcodedSet()
    {
        var sharing = PluginAssemblySharing.Default;

        foreach (var prefix in new[] { "LuBan.", "Microsoft.", "System.", "netstandard", "mscorlib", "Avalonia", "WindowsBase", "System.Runtime" })
            Assert.IsTrue(sharing.IsShared(prefix + "Anything"), $"前缀 {prefix} 必须共享");

        foreach (var exact in new[] { "LuBan.AIAgent", "LuBan.Common", "LuBan.Logging", "LuBan.DI", "LuBan.Orm", "LuBan.Threading", "Avalonia", "Telegram.Bot", "CommunityToolkit.Mvvm" })
            Assert.IsTrue(sharing.IsShared(exact), $"精确名 {exact} 必须共享");
    }

    [TestMethod]
    public void Default_DoesNotShareUnknownAssemblies()
    {
        var sharing = PluginAssemblySharing.Default;

        Assert.IsFalse(sharing.IsShared("LubanAgent.Connectors.AliyunDevOps"));
        Assert.IsFalse(sharing.IsShared("Newtonsoft.Json"));
        Assert.IsFalse(sharing.IsShared(""));
    }

    [TestMethod]
    public void AdditionalPrefixAndExactName_AreShared()
    {
        var sharing = PluginAssemblySharing.Default;
        sharing.AdditionalPrefixes.Add("LubanAgent.");
        sharing.AdditionalExactNames.Add("My.Plugin.Contract");

        Assert.IsTrue(sharing.IsShared("LubanAgent.Connectors.AliyunDevOps"));
        Assert.IsTrue(sharing.IsShared("LubanAgent.Codex"));
        Assert.IsTrue(sharing.IsShared("My.Plugin.Contract"));
        Assert.IsFalse(sharing.IsShared("Newtonsoft.Json"));
    }

    [TestMethod]
    public void IsShared_IsCaseInsensitive()
    {
        var sharing = PluginAssemblySharing.Default;
        sharing.AdditionalExactNames.Add("My.Plugin.Contract");

        Assert.IsTrue(sharing.IsShared("luban.aiaagent"));
        Assert.IsTrue(sharing.IsShared("my.plugin.contract"));
    }

    [TestMethod]
    public void Normalize_CleansAdditionalLists()
    {
        var sharing = new PluginAssemblySharing
        {
            AdditionalPrefixes = new List<string> { "  LubanAgent.  ", "", "  ", "lubanagent." },
            AdditionalExactNames = new List<string> { " My.Plugin ", "my.plugin" }
        };

        sharing.Normalize();

        CollectionAssert.AreEqual(new[] { "LubanAgent." }, sharing.AdditionalPrefixes.ToArray());
        CollectionAssert.AreEqual(new[] { "My.Plugin" }, sharing.AdditionalExactNames.ToArray());
    }

    [TestMethod]
    public void Normalize_KeepsRequiredSetsIntact()
    {
        var sharing = new PluginAssemblySharing();

        sharing.Normalize();

        Assert.IsTrue(sharing.IsShared("LuBan.AIAgent"));
        Assert.IsTrue(sharing.IsShared("Microsoft.Extensions.DependencyInjection"));
    }
}