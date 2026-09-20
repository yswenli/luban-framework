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
using LuBan.AIAgent.Wiki;
using LuBan.AIAgent.Wiki.Extractors;

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

    [TestMethod]
    public void WikiPageSerializer_RoundTrip_PreservesFieldsAndBody()
    {
        var page = new WikiPage
        {
            RelativePath = "entities/张三.md",
            Title = "张三",
            Type = "entity",
            Tags = { "人物", "客户" },
            Sources = { "raw/a.md" },
            Body = "# 张三\n\n正文",
            IndexSummary = "客户负责人",
            Created = new DateTime(2026, 9, 1),
            Updated = new DateTime(2026, 9, 20)
        };

        var markdown = WikiPageSerializer.Render(page);
        Assert.IsTrue(markdown.StartsWith("---\n"), "应以 frontmatter 开头");

        var parsed = WikiPageSerializer.Parse(page.RelativePath, markdown);
        Assert.AreEqual(page.Title, parsed.Title);
        Assert.AreEqual(page.Type, parsed.Type);
        CollectionAssert.AreEqual(page.Tags, parsed.Tags);
        CollectionAssert.AreEqual(page.Sources, parsed.Sources);
        StringAssert.Contains(parsed.Body, "正文");
    }

    [TestMethod]
    public void WikiFrontmatter_NoFrontmatter_ReturnsWholeAsBody()
    {
        var (fields, body) = WikiFrontmatter.Parse("纯文本内容");
        Assert.AreEqual(0, fields.Count);
        Assert.AreEqual("纯文本内容", body);
    }

    [TestMethod]
    public void WikiSlug_FromTitle_FiltersIllegalCharsAndKeepsChinese()
    {
        Assert.AreEqual("张三", WikiSlug.FromTitle("张三"));
        Assert.AreEqual("a-b", WikiSlug.FromTitle("a / b"));
        Assert.AreEqual("untitled", WikiSlug.FromTitle("  ??  "));
    }

    [TestMethod]
    public void WikiSlug_EnsureUnique_AppendsSuffix()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "张三", "张三-2" };
        Assert.AreEqual("张三-3", WikiSlug.EnsureUnique("张三", existing));
    }

    [TestMethod]
    public async Task SourceExtractorRegistry_UnknownExtension_FallsBackToText()
    {
        var registry = SourceExtractorRegistry.CreateDefault();
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.unknown");
            await File.WriteAllTextAsync(file, "hello");
            var source = await registry.ExtractAsync(file);
            Assert.AreEqual("hello", source.Text);
            StringAssert.Contains(source.Markdown, "hello");
        }
        finally { Directory.Delete(dir, true); }
    }
}