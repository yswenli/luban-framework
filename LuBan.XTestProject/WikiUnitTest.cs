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
using LuBan.AIAgent.Retrieval.Chunkers;
using LuBan.AIAgent.Tools.Wiki;
using LuBan.AIAgent.Wiki;
using LuBan.AIAgent.Wiki.Extractors;

using MiniExcelLibs;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Security.Cryptography;
using System.Text;

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

    [TestMethod]
    public async Task ExcelExtractor_BlankHeaderCell_FallsBackToColumnName()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "b.xlsx");
            MiniExcel.SaveAs(file, new[]
            {
                new Dictionary<string, object> { ["a"] = "姓名", ["b"] = "" }
            }, printHeader: false);

            var source = await new ExcelExtractor().ExtractAsync(file);
            StringAssert.Contains(source.Markdown, "姓名");
            StringAssert.Contains(source.Markdown, "列B");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task ExcelExtractor_EscapesPipeAndNewline()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "c.xlsx");
            MiniExcel.SaveAs(file, new[]
            {
                new Dictionary<string, object> { ["a"] = "x|y", ["b"] = "l1\nl2" }
            }, printHeader: false);

            var source = await new ExcelExtractor().ExtractAsync(file);
            StringAssert.Contains(source.Markdown, @"x\|y");
            StringAssert.Contains(source.Markdown, "l1<br>l2");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void SourceExtractorRegistry_DoesNotRegisterXls()
    {
        var registry = SourceExtractorRegistry.CreateDefault();
        Assert.IsTrue(registry.Supports("a.xlsx"));
        Assert.IsFalse(registry.Supports("a.xls"), ".xls（BIFF）MiniExcel 不支持，不应注册");
    }

    [TestMethod]
    public void SourceExtractorRegistry_UnregisteredBinary_Throws()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "d.bin");
            File.WriteAllBytes(file, new byte[] { 0x00, 0x01, 0x7F, 0x00 });
            var registry = SourceExtractorRegistry.CreateDefault();
            Assert.ThrowsExactly<NotSupportedException>(() => registry.Resolve(file));
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void SourceExtractorRegistry_UnregisteredText_FallsBackToTextExtractor()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-wiki-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "e.log");
            File.WriteAllText(file, "hello world");
            var registry = SourceExtractorRegistry.CreateDefault();
            Assert.IsInstanceOfType<TextExtractor>(registry.Resolve(file));
        }
        finally { Directory.Delete(dir, true); }
    }

    private sealed class FakeRetrievalService : IRetrievalService
    {
        public List<string> IndexedFiles { get; } = new();
        public List<string> RemovedSources { get; } = new();
        public List<string> SearchedPrefixes { get; } = new();

        public Task<IndexReport> IndexFileAsync(string path, bool force = false, CancellationToken ct = default, string? workspaceId = null)
        {
            IndexedFiles.Add(path);
            return Task.FromResult(new IndexReport { ScannedFiles = 1, TotalChunks = 1 });
        }

        public Task RemoveAsync(string sourceName, CancellationToken ct = default, string? workspaceId = null)
        {
            RemovedSources.Add(sourceName);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 5, string? pathPrefix = null, string? language = null, CancellationToken ct = default, string? workspaceId = null)
        {
            SearchedPrefixes.Add(pathPrefix ?? "<all>");
            return Task.FromResult<IReadOnlyList<RetrievalResult>>(Array.Empty<RetrievalResult>());
        }

        public Task<IndexReport> IndexDirectoryAsync(string path, string? glob = null, bool force = false, IProgress<IndexProgress>? progress = null, CancellationToken ct = default, string? workspaceId = null)
            => Task.FromResult(new IndexReport());
        public Task<IndexReport> IndexContentAsync(string content, string language, string sourceName, CancellationToken ct = default, string? workspaceId = null)
            => Task.FromResult(new IndexReport());
        public Task<IndexStats> GetStatsAsync(string? workspaceId = null) => Task.FromResult(new IndexStats());
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
        public Task<WikiIndex> ReadIndexAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task<WikiPage> ReadPageAsync(string relativePath, CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task SavePageAsync(WikiPage page, CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task DeletePageAsync(string relativePath, CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int? topK = null, bool? includeRaw = null, CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task<IndexReport> RebuildIndexAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task<WikiLintReport> LintAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
            => throw new NotImplementedException();
        public Task<WikiStats> GetStatsAsync(CancellationToken cancellationToken = default, string? workspaceId = null)
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

    private sealed class FakeEmbedder : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var list = values.Select(_ => new Embedding<float>(new float[] { 1f, 0f, 0f })).ToList();
            return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(list));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    private sealed class InMemoryVectorStore : IVectorStore
    {
        private sealed class FileRow
        {
            public string FilePath = "";
            public string FileHash = "";
            public string Language = "";
            public List<ChunkVectorPair> Chunks = new();
        }

        private readonly Dictionary<long, FileRow> _files = new();
        private long _nextId;

        public int ReplaceCalls { get; private set; }

        public Task<IReadOnlyList<IndexedFile>> GetFilesAsync(string? pathPrefix = null, string? workspaceId = null)
        {
            IReadOnlyList<IndexedFile> result = _files
                .Where(kv => pathPrefix == null || kv.Value.FilePath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(kv => new IndexedFile { Id = kv.Key, FilePath = kv.Value.FilePath, FileHash = kv.Value.FileHash, Language = kv.Value.Language })
                .ToList();
            return Task.FromResult(result);
        }

        public Task<long> UpsertFileAsync(string filePath, string fileHash, string language, int chunkCount, string? workspaceId = null)
        {
            var hit = _files.FirstOrDefault(kv => string.Equals(kv.Value.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            if (hit.Key != 0)
            {
                hit.Value.FileHash = fileHash;
                hit.Value.Language = language;
                return Task.FromResult(hit.Key);
            }
            var id = ++_nextId;
            _files[id] = new FileRow { FilePath = filePath, FileHash = fileHash, Language = language };
            return Task.FromResult(id);
        }

        public Task SoftDeleteFileAsync(long fileId, string? workspaceId = null)
        {
            _files.Remove(fileId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<StoredChunk>> GetFileChunksAsync(long fileId, string? workspaceId = null)
        {
            IReadOnlyList<StoredChunk> result = _files.TryGetValue(fileId, out var row)
                ? row.Chunks.Select((p, i) => new StoredChunk { Id = i + 1, ChunkIndex = i, ContentHash = Hash(p.Chunk.Content), Vector = p.Vector }).ToList()
                : new List<StoredChunk>();
            return Task.FromResult(result);
        }

        public Task ReplaceFileChunksAsync(long fileId, string modelId, IReadOnlyList<ChunkVectorPair> chunks, string? workspaceId = null)
        {
            if (_files.TryGetValue(fileId, out var row)) row.Chunks = chunks.ToList();
            ReplaceCalls++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<VectorEntry>> LoadVectorsAsync(string? pathPrefix = null, string? language = null, int maxResults = int.MaxValue, string? workspaceId = null)
        {
            var list = new List<VectorEntry>();
            long id = 0;
            foreach (var row in _files.Values)
                foreach (var p in row.Chunks)
                    list.Add(new VectorEntry { ChunkId = ++id, Vector = p.Vector });
            IReadOnlyList<VectorEntry> result = list;
            return Task.FromResult(result);
        }

        public Task<Dictionary<long, CodeChunk>> GetChunksAsync(IReadOnlyList<long> chunkIds, string? workspaceId = null)
        {
            var map = new Dictionary<long, CodeChunk>();
            long id = 0;
            foreach (var row in _files.Values)
                foreach (var p in row.Chunks)
                {
                    id++;
                    if (chunkIds.Contains(id)) map[id] = p.Chunk;
                }
            return Task.FromResult(map);
        }

        public Task<StoreStats> GetStatsAsync(string? workspaceId = null)
            => Task.FromResult(new StoreStats { FileCount = _files.Count, ChunkCount = _files.Values.Sum(r => r.Chunks.Count) });

        private static string Hash(string content) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
    }

    [TestMethod]
    public async Task RetrievalService_IndexFileAsync_DoesNotDeadlock()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-retrieval-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.txt");
            await File.WriteAllTextAsync(file, "hello world");
            var store = new InMemoryVectorStore();
            var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

            var report = await service.IndexFileAsync(file, force: true, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(1, report.ScannedFiles);
            Assert.AreEqual(1, store.ReplaceCalls, "IndexFileAsync 必须完成落库并返回，写锁重入会导致永久死锁");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task RetrievalService_IndexContentAsync_DoesNotDeadlock()
    {
        var store = new InMemoryVectorStore();
        var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

        var report = await service.IndexContentAsync("hello world", "text", "inline.txt", workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual(1, report.ScannedFiles);
        Assert.AreEqual(1, store.ReplaceCalls, "IndexContentAsync 必须完成落库并返回，写锁重入会导致永久死锁");
    }

    [TestMethod]
    public async Task RetrievalService_IndexThenSearch_ReturnsIndexedChunk()
    {
        var store = new InMemoryVectorStore();
        var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

        await service.IndexContentAsync("hello world", "text", "inline.txt", workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));
        var results = await service.SearchAsync("hello", topK: 5, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("inline.txt", results[0].FilePath);
    }

    [TestMethod]
    public void ChunkerFactory_ShouldIndex_UsesExtensionWhitelistWithoutReadingContent()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var factory = new ChunkerFactory();
            var root = Path.GetFullPath(dir);

            foreach (var ext in new[] { ".txt", ".csv", ".tsv", ".log", ".properties", ".xlsx", ".md", ".cs", ".json" })
            {
                var file = Path.Combine(dir, "a" + ext);
                File.WriteAllText(file, "hello world");
                Assert.IsTrue(factory.ShouldIndex(file, root, 5120 * 1024L), $"{ext} 应位于索引白名单内");
            }

            foreach (var ext in new[] { ".docx", ".pdf", ".png", ".ps1", ".exe" })
            {
                var file = Path.Combine(dir, "a" + ext);
                File.WriteAllText(file, "hello world");
                Assert.IsFalse(factory.ShouldIndex(file, root, 5120 * 1024L), $"{ext} 不在白名单内，应跳过");
            }

            var noExt = Path.Combine(dir, "Dockerfile");
            File.WriteAllText(noExt, "FROM scratch");
            Assert.IsFalse(factory.ShouldIndex(noExt, root, 5120 * 1024L), "无扩展名文件不在白名单内，应跳过");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void ChunkerFactory_ShouldIndex_SkipsExcludedDirsAndOversizedFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "node_modules"));
        try
        {
            var factory = new ChunkerFactory();
            var root = Path.GetFullPath(dir);

            var excluded = Path.Combine(dir, "node_modules", "a.txt");
            File.WriteAllText(excluded, "hello");
            Assert.IsFalse(factory.ShouldIndex(excluded, root, 5120 * 1024L), "排除目录内的文件应跳过");

            var oversized = Path.Combine(dir, "big.txt");
            File.WriteAllText(oversized, "0123456789");
            Assert.IsFalse(factory.ShouldIndex(oversized, root, 4), "超出大小上限的文件应跳过");
            Assert.IsTrue(factory.ShouldIndex(oversized, root, 64), "未超上限时应通过");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void ChunkerFactory_EnumerateFiles_SkipsExcludedDirectoriesAndReparsePoints()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "obj"));
        Directory.CreateDirectory(Path.Combine(dir, "src"));
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
            File.WriteAllText(Path.Combine(dir, "obj", "b.txt"), "b");
            File.WriteAllText(Path.Combine(dir, "src", "c.txt"), "c");

            var files = ChunkerFactory.EnumerateFiles(Path.GetFullPath(dir), "*")
                .Select(Path.GetFileName)
                .ToList();

            CollectionAssert.Contains(files, "a.txt");
            CollectionAssert.Contains(files, "c.txt");
            CollectionAssert.DoesNotContain(files, "b.txt");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task RetrievalService_IndexDirectoryAsync_ExtractsXlsxContent()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var xlsx = Path.Combine(dir, "people.xlsx");
            MiniExcel.SaveAs(xlsx, new[] { new { Name = "张三", Age = 18 } });
            File.WriteAllText(Path.Combine(dir, "note.txt"), "hello world");

            var store = new InMemoryVectorStore();
            var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

            var report = await service.IndexDirectoryAsync(dir, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(2, report.ScannedFiles, ".xlsx 与 .txt 都应进入索引");
            Assert.AreEqual(0, report.Warnings.Count, "未超上限的表格不应产生截断警告");
            var results = await service.SearchAsync("张三", topK: 5, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(results.Any(r => r.FilePath.EndsWith("people.xlsx", StringComparison.OrdinalIgnoreCase) && r.Content.Contains("张三")),
                ".xlsx 应被提取为文本后参与检索（不能按 UTF-8 直读出乱码）");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task RetrievalService_IndexDirectoryAsync_SkipsBinaryContentInWhitelistedExtension()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(dir, "weird.log"), new byte[] { 0x01, 0x00, 0x02, 0x00 });
            File.WriteAllText(Path.Combine(dir, "note.txt"), "hello world");

            var store = new InMemoryVectorStore();
            var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

            var report = await service.IndexDirectoryAsync(dir, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(2, report.ScannedFiles, "白名单扩展名在扫描阶段一律计入");
            Assert.AreEqual(1, report.SkippedFiles, "读取阶段发现 NUL 字节的伪文本应被跳过");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void ChunkerFactory_RoutesXlsxToHeaderChunker()
    {
        var factory = new ChunkerFactory();

        Assert.AreEqual("excel", factory.GetLanguage("a.xlsx"), ".xlsx 提取后是 markdown 表格，应路由到标题分节切块器");

        // 每节 >MinChars(150)，避免被 MergeSmall 合并，从而验证“按 sheet 分节”本身
        static string Sheet(string name)
            => $"## {name}\n" + string.Concat(Enumerable.Range(0, 12).Select(i => $"| {name}列{i} | 值{i:D4} |\n"));

        var chunks = factory.GetChunker("a.xlsx").Chunk("a.xlsx", Sheet("S1") + "\n" + Sheet("S2"));

        Assert.AreEqual(2, chunks.Count, ".xlsx 应按 sheet（## 标题）分节，而非整体滑动窗口");
        CollectionAssert.AreEqual(new[] { "S1", "S2" }, chunks.Select(c => c.SymbolName).ToArray());
    }

    [TestMethod]
    public void ExcelExtractor_ExtractAsync_RespectsMaxChars()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var xlsx = Path.Combine(dir, "big.xlsx");
            MiniExcel.SaveAs(xlsx, Enumerable.Range(0, 50).Select(i => new { Name = "row" + i, Value = i }).ToArray());

            var full = new ExcelExtractor().ExtractAsync(xlsx).GetAwaiter().GetResult();
            Assert.IsFalse(full.Truncated, "默认上限下小表不应截断");
            Assert.IsTrue(full.Markdown.Contains("row49"), "小表应完整提取");

            var cut = new ExcelExtractor().ExtractAsync(xlsx, maxChars: 80).GetAwaiter().GetResult();
            Assert.IsTrue(cut.Truncated, "超出 maxChars 应标记 Truncated（索引侧据此写入 IndexReport.Warnings）");
            Assert.AreEqual(80, cut.Markdown.Length);
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task RetrievalService_IndexFileAsync_SkipsNonWhitelistedExtension()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "a.docx");
            await File.WriteAllTextAsync(file, "not really docx");

            var store = new InMemoryVectorStore();
            var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

            var report = await service.IndexFileAsync(file, force: true, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(1, report.ScannedFiles);
            Assert.AreEqual(1, report.SkippedFiles);
            Assert.AreEqual(0, store.ReplaceCalls, ".docx 不在白名单内，不应按 UTF-8 直读入库");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task RetrievalService_IndexDirectoryAsync_DeduplicatesOverlappingGlobs()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.txt"), "hello");
            File.WriteAllText(Path.Combine(dir, "b.log"), "world");

            var store = new InMemoryVectorStore();
            var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

            var report = await service.IndexDirectoryAsync(dir, glob: "a.*;*.txt", workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

            Assert.AreEqual(1, report.ScannedFiles, "重叠 glob 命中的同一文件只应扫描一次");
            Assert.AreEqual(1, report.NewFiles);
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task RetrievalService_IndexDirectoryAsync_ReportsScanningProgressPerDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-index-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dir, "sub"));
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.txt"), "hello");
            File.WriteAllText(Path.Combine(dir, "sub", "b.txt"), "world");

            var collector = new ProgressCollector();
            var store = new InMemoryVectorStore();
            var service = new RetrievalService(store, new FakeEmbedder(), Options.Create(new LuBanAgentOptions()));

            await service.IndexDirectoryAsync(dir, progress: collector, workspaceId: "ws").WaitAsync(TimeSpan.FromSeconds(10));

            var scanning = collector.Items.Where(p => p.Stage == IndexStage.Scanning).ToList();
            Assert.IsTrue(scanning.Any(p => p.CurrentFile != null && p.CurrentFile.EndsWith("sub", StringComparison.OrdinalIgnoreCase)),
                "扫描阶段应按目录上报，调用方据此显示“正在扫描：<目录>（已发现 N 个）”");
            Assert.AreEqual(2, scanning[^1].Total, "枚举结束时应上报最终可索引文件数");
        }
        finally { Directory.Delete(dir, true); }
    }

    private sealed class ProgressCollector : IProgress<IndexProgress>
    {
        public List<IndexProgress> Items { get; } = new();
        public void Report(IndexProgress value) => Items.Add(value);
    }
}