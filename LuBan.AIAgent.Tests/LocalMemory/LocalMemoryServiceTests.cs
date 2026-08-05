using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LocalMemory;
using LuBan.AIAgent.Tools.LocalMemory;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.LocalMemory;

[TestClass]
public class LocalMemoryServiceTests
{
    private string _dbPath = "";
    private SqliteLocalMemoryStore? _store;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"luban_localmemory_test_{Guid.NewGuid():N}.db");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _store?.Dispose();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    private sealed class FakeWorkspace(string? wsId) : IWorkspaceContextProvider
    {
        public string? CurrentWorkspaceId { get; set; } = wsId;
    }

    private LocalMemoryService CreateService(IWorkspaceContextProvider? workspace = null, int? ttlDays = null)
    {
        _store = new SqliteLocalMemoryStore(_dbPath);
        var options = Options.Create(new LocalMemoryOptions
        {
            FallbackDimension = 64,
            TtlDays = ttlDays
        });
        return new LocalMemoryService(_store, options, embedder: null, workspace);
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
    public async Task SaveAsync_DuplicateContent_ReturnsSameId()
    {
        var service = CreateService(new FakeWorkspace("ws1"));
        var a = await service.SaveAsync(" 记住这个事实 ", "fact");
        var b = await service.SaveAsync("记住这个事实", "fact");

        Assert.AreEqual(a.Id, b.Id);
        Assert.AreEqual(1, (await service.ListAsync("fact")).Count);
    }

    [TestMethod]
    public async Task WorkspaceIsolation_AcrossWorkspaces()
    {
        var ws1 = CreateService(new FakeWorkspace("ws1"));
        var ws2 = CreateService(new FakeWorkspace("ws2"));
        await ws1.SaveAsync("工作区1的秘密", "fact");

        var inWs1 = await ws1.SearchAsync("工作区1的秘密", null, 5);
        var inWs2 = await ws2.SearchAsync("工作区1的秘密", null, 5);

        Assert.IsTrue(inWs1.Count > 0);
        Assert.AreEqual(0, inWs2.Count, "其他工作区不应看到该记忆");
    }

    [TestMethod]
    public async Task GlobalCategory_VisibleAcrossWorkspaces()
    {
        var ws1 = CreateService(new FakeWorkspace("ws1"));
        var ws2 = CreateService(new FakeWorkspace("ws2"));
        await ws1.SaveAsync("用户偏好简洁回答", MemoryCategories.Global);

        var inWs2 = await ws2.SearchAsync("偏好简洁回答", MemoryCategories.Global, 5);
        Assert.IsTrue(inWs2.Count > 0, "全局记忆应跨工作区可见");
    }

    [TestMethod]
    public async Task Ttl_ExpiredEntries_NotReturned()
    {
        var service = CreateService(ttlDays: 0);
        await service.SaveAsync("会过期的记忆", "fact");

        var results = await service.SearchAsync("会过期的记忆", null, 5);
        Assert.AreEqual(0, results.Count, "TTL=0 的条目应立即过期");
    }

    [TestMethod]
    public async Task InvertedIndex_MatchesFullScan()
    {
        var service = CreateService(new FakeWorkspace("ws1"));
        await service.SaveAsync("我喜欢编程和算法", "fact");
        await service.SaveAsync("团队喜欢团建活动", "fact");
        await service.SaveAsync("今天天气晴朗", "fact");

        var viaIndex = await service.SearchAsync("喜欢编程", null, 5);

        Assert.IsTrue(viaIndex.Count > 0, "倒排预筛应命中相关记忆");
        Assert.IsTrue(viaIndex.Any(r => r.Content.Contains("编程")), "最相关记忆应排在最前");
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
