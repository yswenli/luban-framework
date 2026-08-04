using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LocalMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.LocalMemory;

[TestClass]
public class LocalMemoryServiceTests
{
    private string _dbPath = "";

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"luban_localmemory_test_{Guid.NewGuid():N}.db");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    private LocalMemoryService CreateService(IEmbeddingGenerator<string, Embedding<float>>? embedder = null)
    {
        var store = new SqliteLocalMemoryStore(_dbPath);
        var options = Options.Create(new LocalMemoryOptions { FallbackDimension = 64 });
        return new LocalMemoryService(store, options, embedder);
    }

    [TestMethod]
    public async Task SaveAsync_ReturnsEntryWithId()
    {
        var service = CreateService();
        var entry = await service.SaveAsync("用户偏好使用 C#", "preference");

        Assert.IsNotNull(entry.Id);
        Assert.AreEqual("preference", entry.Category);
        Assert.IsTrue(entry.Content.Contains("C#"));
    }

    [TestMethod]
    public async Task SearchAsync_WithFallbackEmbedding_FindsRelevantContent()
    {
        var service = CreateService();
        await service.SaveAsync("项目使用 .NET 8 开发", "project");
        await service.SaveAsync("后端数据库是 PostgreSQL", "project");
        await service.SaveAsync("我最喜欢的颜色是蓝色", "preference");

        var results = await service.SearchAsync("技术栈", "project", 2);

        Assert.IsTrue(results.Count > 0);
        Assert.IsTrue(results.All(r => r.Category == "project"));
        Assert.IsTrue(results.First().Score > 0);
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesEntry()
    {
        var service = CreateService();
        var entry = await service.SaveAsync("待删除", "todo");
        var ok = await service.DeleteAsync(entry.Id);

        Assert.IsTrue(ok);
        var list = await service.ListAsync("todo");
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public async Task ListAsync_RespectsCategoryFilter()
    {
        var service = CreateService();
        await service.SaveAsync("A", "fact");
        await service.SaveAsync("B", "preference");

        var facts = await service.ListAsync("fact");
        var prefs = await service.ListAsync("preference");

        Assert.AreEqual(1, facts.Count);
        Assert.AreEqual(1, prefs.Count);
    }

    [TestMethod]
    public async Task SaveAsync_ToolResult_WrapsOk()
    {
        var service = CreateService();
        var tool = new LocalMemoryToolGroup(service, new LocalMemoryOptions());
        var result = await tool.SaveAsync("测试内容", "fact");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
    }

    [TestMethod]
    public async Task SearchAsync_ToolResult_WrapsOk()
    {
        var service = CreateService();
        await service.SaveAsync("测试内容", "fact");
        var tool = new LocalMemoryToolGroup(service, new LocalMemoryOptions());
        var result = await tool.SearchAsync("测试", "fact", 5);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Data));
    }
}
