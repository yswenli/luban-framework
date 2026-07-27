using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Retrieval;
using LuBan.AIAgent.Tests.Retrieval.Fakes;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class RetrievalServiceTests
{
    private FakeVectorStore _store = null!;
    private FakeEmbeddingGenerator _embedder = null!;
    private RetrievalService _service = null!;
    private string _tempDir = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new FakeVectorStore();
        _embedder = new FakeEmbeddingGenerator();
        var options = Options.Create(new LuBanAgentOptions());
        _service = new RetrievalService(_store, _embedder, options);
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    [TestMethod]
    public async Task IndexThenSearch_ReturnsRelevantChunk()
    {
        File.WriteAllText(Path.Combine(_tempDir, "auth.cs"), "class Auth { void AuthenticateUser() { } }");
        File.WriteAllText(Path.Combine(_tempDir, "payment.cs"), "class Payment { void ProcessPayment() { } }");

        var report = await _service.IndexDirectoryAsync(_tempDir);
        Assert.AreEqual(2, report.ScannedFiles);
        Assert.AreEqual(2, report.NewFiles);
        Assert.IsTrue(report.TotalChunks >= 2);

        var results = await _service.SearchAsync("auth");
        Assert.IsTrue(results.Count > 0);
        Assert.IsTrue(results[0].FilePath.EndsWith("auth.cs"));
    }

    [TestMethod]
    public async Task Index_UnchangedFile_Skipped()
    {
        File.WriteAllText(Path.Combine(_tempDir, "a.cs"), "class A { }");
        File.WriteAllText(Path.Combine(_tempDir, "b.cs"), "class B { }");

        await _service.IndexDirectoryAsync(_tempDir);
        var report = await _service.IndexDirectoryAsync(_tempDir);
        Assert.AreEqual(2, report.SkippedFiles);
        Assert.AreEqual(0, report.EmbeddedChunks);
    }

    [TestMethod]
    public async Task IndexContent_SkipsIdenticalContent()
    {
        var html = "<html><body><p>Test content</p></body></html>";
        await _service.IndexContentAsync(html, "html", "web://test.com/page");
        var report = await _service.IndexContentAsync(html, "html", "web://test.com/page");
        Assert.AreEqual(1, report.SkippedFiles);
    }

    [TestMethod]
    public async Task GetStats_ReturnsCounts()
    {
        File.WriteAllText(Path.Combine(_tempDir, "a.cs"), "class A { }");
        await _service.IndexDirectoryAsync(_tempDir);
        var stats = await _service.GetStatsAsync();
        Assert.AreEqual(1, stats.TotalFiles);
        Assert.IsTrue(stats.TotalChunks >= 1);
        Assert.AreEqual(4, stats.VectorDimension);
    }
}
