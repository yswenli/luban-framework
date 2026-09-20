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
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Tools.Wiki;
using LuBan.AIAgent.Wiki;
using LuBan.AIAgent.Wiki.Extractors;

using MiniExcelLibs;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
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

    [TestMethod]
    public async Task JsonExtractor_Jsonl_ProducesOneBlockPerLine()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.jsonl");
            await File.WriteAllTextAsync(file, "{\"a\":1}\n{\"a\":2}\n");
            var source = await new JsonExtractor().ExtractAsync(file);
            StringAssert.Contains(source.Markdown, "1");
            StringAssert.Contains(source.Markdown, "2");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task DelimitedTextExtractor_Csv_ProducesMarkdownTable()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.csv");
            await File.WriteAllTextAsync(file, "name,age\n张三,30\n");
            var source = await new DelimitedTextExtractor().ExtractAsync(file);
            StringAssert.Contains(source.Markdown, "| name | age |");
            StringAssert.Contains(source.Markdown, "张三");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task HtmlExtractor_DegradesToPlainText()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.html");
            await File.WriteAllTextAsync(file, "<html><body><h1>标题</h1><script>var x=1;</script><p>段落</p></body></html>");
            var source = await new HtmlExtractor().ExtractAsync(file);
            StringAssert.Contains(source.Text, "标题");
            StringAssert.Contains(source.Text, "段落");
            Assert.IsFalse(source.Text.Contains("var x=1"), "script 内容应被剥离");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task ExcelExtractor_Xlsx_ProducesMarkdownTable()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.xlsx");
            MiniExcel.SaveAs(file, new[]
            {
                new Dictionary<string, object> { ["name"] = "张三", ["age"] = 30 }
            });
            var source = await new ExcelExtractor().ExtractAsync(file);
            StringAssert.Contains(source.Markdown, "张三");
        }
        finally { Directory.Delete(dir, true); }
    }

    private sealed class FakeRetrievalService : IRetrievalService
    {
        public List<string> IndexedFiles { get; } = new();
        public List<string> RemovedSources { get; } = new();
        public List<string> SearchedPrefixes { get; } = new();

        public Task<IndexReport> IndexFileAsync(string path, bool force = false, CancellationToken ct = default)
        {
            IndexedFiles.Add(path);
            return Task.FromResult(new IndexReport { ScannedFiles = 1, TotalChunks = 1 });
        }

        public Task RemoveAsync(string sourceName, CancellationToken ct = default)
        {
            RemovedSources.Add(sourceName);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null, CancellationToken ct = default)
        {
            SearchedPrefixes.Add(pathPrefix ?? "<all>");
            return Task.FromResult<IReadOnlyList<RetrievalResult>>(Array.Empty<RetrievalResult>());
        }

        public Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false, CancellationToken ct = default)
            => Task.FromResult(new IndexReport());
        public Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken ct = default)
            => Task.FromResult(new IndexReport());
        public Task<IndexStats> GetStatsAsync() => Task.FromResult(new IndexStats());
    }

    private sealed class FakeWikiContext : IWikiContext
    {
        public string? WorkspaceRoot { get; set; }
    }

    [TestMethod]
    public async Task WikiService_SaveAndDeletePage_MaintainsIndexLogAndVector()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var retrieval = new FakeRetrievalService();
        try
        {
            var service = new WikiService(retrieval, new FakeWikiContext { WorkspaceRoot = root }, new WikiToolOptions());
            var page = new WikiPage { RelativePath = "entities/张三.md", Title = "张三", Type = "entity", Body = "内容" };

            await service.SavePageAsync(page);
            Assert.IsTrue(File.Exists(Path.Combine(root, "wiki", "entities", "张三.md")));
            Assert.IsTrue(File.Exists(Path.Combine(root, "wiki", "index.md")));
            Assert.IsTrue(File.Exists(Path.Combine(root, "wiki", "log.md")));
            var index = await service.ReadIndexAsync();
            Assert.IsTrue(index.Entries.Any(e => e.RelativePath == "entities/张三.md"));
            Assert.IsTrue(retrieval.IndexedFiles.Any(p => p.EndsWith("张三.md")));

            await service.DeletePageAsync("entities/张三.md");
            Assert.IsFalse(File.Exists(Path.Combine(root, "wiki", "entities", "张三.md")));
            Assert.IsFalse((await service.ReadIndexAsync()).Entries.Any(e => e.RelativePath == "entities/张三.md"));
            Assert.IsTrue(retrieval.RemovedSources.Any(p => p.EndsWith("张三.md")));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task WikiService_Search_DefaultsToWikiPrefix()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var retrieval = new FakeRetrievalService();
            var service = new WikiService(retrieval, new FakeWikiContext { WorkspaceRoot = root }, new WikiToolOptions());
            await service.SearchAsync("q");
            Assert.AreEqual(Path.Combine(root, "wiki"), retrieval.SearchedPrefixes[0]);
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task WikiService_SavePage_RejectsPathTraversal()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new WikiService(new FakeRetrievalService(), new FakeWikiContext { WorkspaceRoot = root }, new WikiToolOptions());
            await Assert.ThrowsExactlyAsync<ArgumentException>(
                () => service.SavePageAsync(new WikiPage { RelativePath = "../evil.md", Title = "e", Body = "x" }));
            Assert.IsFalse(File.Exists(Path.Combine(root, "evil.md")));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public void WikiToolPlugin_IsOptIn_AndEnabledByDefault()
    {
        var plugin = new WikiToolPlugin(Options.Create(new LuBanAgentOptions()));
        Assert.AreEqual("wiki", plugin.GroupName);
        Assert.IsTrue(plugin.IsOptIn);
        Assert.IsTrue(plugin.IsEnabled(new LuBanAgentOptions()));
    }

    [TestMethod]
    public void WikiToolPlugin_GetTools_RespectsDisabledOption()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWikiService>(new StubWikiService());
        var sp = services.BuildServiceProvider();
        var plugin = new WikiToolPlugin(Options.Create(new LuBanAgentOptions()));

        var disabled = new ToolGroupOptions { Wiki = { Enabled = false } };
        Assert.AreEqual(0, plugin.GetTools(sp, disabled).Count);
    }

    private sealed class StubWikiService : IWikiService
    {
        public Task<WikiIndex> ReadIndexAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<WikiPage> ReadPageAsync(string relativePath, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task SavePageAsync(WikiPage page, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task DeletePageAsync(string relativePath, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 8, bool includeRaw = false, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<IndexReport> RebuildIndexAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<WikiLintReport> LintAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<WikiStats> GetStatsAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class CapturingConfirmationService : IToolConfirmationService
    {
        public List<string> CapturedToolNames { get; } = new();
        public HashSet<string> AutoConfirmTools { get; set; } = new();
        public HashSet<string> AlwaysConfirmTools { get; set; } = new();
        public HashSet<string> ReadOnlyTools { get; set; } = new();

        public Task<EnumConfirmationOutcome> EvaluateAsync(string toolName, string? path, IReadOnlyDictionary<string, object?> arguments)
        {
            CapturedToolNames.Add(toolName);
            return Task.FromResult(EnumConfirmationOutcome.Denied);
        }

        public Task<bool> RequestConfirmation(string toolName, IReadOnlyDictionary<string, object?> arguments)
            => Task.FromResult(false);

        public Task<bool> TryConfirmByPath(string toolName, string path, IReadOnlyDictionary<string, object?> arguments)
            => Task.FromResult(false);

        public string FormatArguments(IReadOnlyDictionary<string, object?> arguments, int maxLength = 200)
            => string.Empty;
    }

    [TestMethod]
    public void WikiIndex_Parse_RestoresSummaryAndUpdated()
    {
        var source = new WikiIndex();
        source.Entries.Add(new WikiIndexEntry
        {
            Category = "entities",
            RelativePath = "entities/张三.md",
            Title = "张三",
            Summary = "客户负责人",
            Updated = new DateTime(2026, 9, 20)
        });

        var parsed = WikiIndex.Parse(source.Render());

        Assert.AreEqual(1, parsed.Entries.Count);
        Assert.AreEqual("entities/张三.md", parsed.Entries[0].RelativePath);
        Assert.AreEqual("张三", parsed.Entries[0].Title);
        Assert.AreEqual("客户负责人", parsed.Entries[0].Summary);
        Assert.AreEqual(new DateTime(2026, 9, 20), parsed.Entries[0].Updated);
    }

    [TestMethod]
    public async Task WikiService_SavePage_PreservesOtherEntriesSummary()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new WikiService(new FakeRetrievalService(), new FakeWikiContext { WorkspaceRoot = root }, new WikiToolOptions());

            await service.SavePageAsync(new WikiPage
            {
                RelativePath = "entities/张三.md",
                Title = "张三",
                Type = "entity",
                Body = "正文",
                IndexSummary = "客户负责人"
            });
            await service.SavePageAsync(new WikiPage
            {
                RelativePath = "entities/李四.md",
                Title = "李四",
                Type = "entity",
                Body = "正文"
            });

            var index = await service.ReadIndexAsync();
            var zhang = index.Entries.Single(e => e.RelativePath == "entities/张三.md");
            Assert.AreEqual("客户负责人", zhang.Summary, "保存其它页面后原条目摘要不应丢失");
            Assert.IsNotNull(zhang.Updated, "保存其它页面后原条目更新时间不应丢失");
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task WikiService_Lint_NoUnindexedFindingForIndexAndOverview()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new WikiService(new FakeRetrievalService(), new FakeWikiContext { WorkspaceRoot = root }, new WikiToolOptions());
            await service.SavePageAsync(new WikiPage
            {
                RelativePath = "entities/张三.md",
                Title = "张三",
                Type = "entity",
                Body = "正文"
            });
            await File.WriteAllTextAsync(
                Path.Combine(root, "wiki", "overview.md"),
                "# 概览\n\n参见 [张三](entities/张三.md)。\n");

            var report = await service.LintAsync();

            Assert.IsFalse(report.Findings.Any(f => f.Kind == LintFindingKind.Unindexed && f.Path.Equals("index.md", StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(report.Findings.Any(f => f.Kind == LintFindingKind.Unindexed && f.Path.Equals("overview.md", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(0, report.Findings.Count, "干净 wiki 不应有任何 lint 发现");
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task WikiService_ReadPage_EmptyPath_ThrowsArgumentException()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var service = new WikiService(new FakeRetrievalService(), new FakeWikiContext { WorkspaceRoot = root }, new WikiToolOptions());
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => service.ReadPageAsync("   "));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public async Task WikiToolGroup_Delete_UsesMethodNameForConfirmation()
    {
        var confirmation = new CapturingConfirmationService();
        var group = new WikiToolGroup(new StubWikiService(), new WikiToolOptions(), confirmation);

        await group.DeletePageAsync("entities/张三.md");
        Assert.AreEqual(nameof(WikiToolGroup.DeletePageAsync), confirmation.CapturedToolNames.Single());

        await group.SavePageAsync("entities/李四.md", "李四", "entity", "正文");
        Assert.AreEqual(nameof(WikiToolGroup.SavePageAsync), confirmation.CapturedToolNames.Last());
    }
}