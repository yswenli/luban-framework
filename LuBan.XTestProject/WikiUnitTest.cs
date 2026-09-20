/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： WikiUnitTest
*版本号： V1.0.0.0
*唯一标识：b2c3d4e5-f6a7-8901-bcde-f12345678901
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：LuBan.AIAgent opt-in 工具组门控单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class WikiUnitTest
{
    private sealed class FakePlugin : ILuBanToolPlugin
    {
        public FakePlugin(string name, bool optIn) { GroupName = name; IsOptIn = optIn; }
        public string GroupName { get; }
        public string? Description => null;
        public bool IsOptIn { get; }
        public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
            => Array.Empty<AIFunction>();
        public bool IsEnabled(LuBanAgentOptions options) => true;
    }

    [TestMethod]
    public void GetPlugins_NullGroupNames_ExcludesOptIn()
    {
        var registry = new ToolPluginRegistry(
            new ILuBanToolPlugin[] { new FakePlugin("filesystem", false), new FakePlugin("wiki", true) },
            Options.Create(new LuBanAgentOptions()));

        var all = registry.GetPlugins(null);
        Assert.IsFalse(all.Any(p => p.GroupName == "wiki"), "ToolGroups=null 时不应包含 opt-in 工具组");
        Assert.IsTrue(all.Any(p => p.GroupName == "filesystem"));

        var explicitWiki = registry.GetPlugins(new[] { "wiki" });
        Assert.IsTrue(explicitWiki.Any(p => p.GroupName == "wiki"), "显式点名时应包含 opt-in 工具组");
    }
}