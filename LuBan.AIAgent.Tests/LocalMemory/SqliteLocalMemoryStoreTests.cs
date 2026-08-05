using LuBan.AIAgent.LocalMemory;

namespace LuBan.AIAgent.Tests.LocalMemory;

[TestClass]
public class SqliteLocalMemoryStoreTests
{
    private string _dbPath = "";
    private SqliteLocalMemoryStore? _store;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"luban_store_test_{Guid.NewGuid():N}.db");
        _store = new SqliteLocalMemoryStore(_dbPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _store?.Dispose();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    private static byte[] Vec(params float[] v)
    {
        var bytes = new byte[v.Length * sizeof(float)];
        Buffer.BlockCopy(v, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private static MemoryEntry NewEntry(string content, string category, string? workspaceId = null, DateTime? expiresAt = null)
    {
        return new MemoryEntry
        {
            Content = content,
            Category = category,
            WorkspaceId = workspaceId,
            ContentHash = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            VectorDimension = 2
        };
    }

    [TestMethod]
    public async Task Upsert_SameWorkspaceAndHash_UpdatesExisting_KeepsId()
    {
        var first = await _store.UpsertAsync(NewEntry("内容A", "fact", "ws1"), Vec(1, 0));
        var second = await _store.UpsertAsync(NewEntry("内容A", "fact", "ws1"), Vec(0, 1));

        Assert.AreEqual(first.Id, second.Id, "重复内容应复用原条目");
        Assert.AreEqual(1, (await _store.LoadAllAsync(null, null, includeAllWorkspaces: true)).Count, "不应新增行");
    }

    [TestMethod]
    public async Task Upsert_SameHash_DifferentWorkspace_CreatesSeparate()
    {
        var a = await _store.UpsertAsync(NewEntry("内容A", "fact", "ws1"), Vec(1, 0));
        var b = await _store.UpsertAsync(NewEntry("内容A", "fact", "ws2"), Vec(1, 0));

        Assert.AreNotEqual(a.Id, b.Id);
        Assert.AreEqual(2, (await _store.LoadAllAsync(null, null, includeAllWorkspaces: true)).Count);
    }

    [TestMethod]
    public async Task List_WorkspaceIsolation_ShowsOwnAndGlobalOnly()
    {
        await _store.UpsertAsync(NewEntry("工作区1的事", "fact", "ws1"), Vec(1, 0));
        await _store.UpsertAsync(NewEntry("工作区2的事", "fact", "ws2"), Vec(1, 0));
        await _store.UpsertAsync(NewEntry("全局偏好", "global"), Vec(1, 0));

        var ws1 = await _store.ListAsync(null, "ws1", 100);
        Assert.AreEqual(2, ws1.Count, "ws1 应看到自己的 + 全局");

        var ws2 = await _store.ListAsync(null, "ws2", 100);
        Assert.AreEqual(2, ws2.Count, "ws2 应看到自己的 + 全局");

        var all = await _store.LoadAllAsync(null, null, includeAllWorkspaces: true);
        Assert.AreEqual(3, all.Count, "includeAllWorkspaces 应返回全部 3 行");
    }

    [TestMethod]
    public async Task Search_FiltersExpired()
    {
        var expired = NewEntry("过期记忆", "fact", null, DateTime.UtcNow.AddMinutes(-1));
        var alive = NewEntry("活跃记忆", "fact");
        await _store.UpsertAsync(expired, Vec(1, 0));
        await _store.UpsertAsync(alive, Vec(0, 1));

        var all = await _store.LoadAllAsync(null, null, includeAllWorkspaces: true);
        Assert.AreEqual(1, all.Count, "过期条目不应被加载");
        Assert.AreEqual("活跃记忆", all[0].Entry.Content);
    }

    [TestMethod]
    public async Task DeleteExpired_RemovesRows()
    {
        await _store.UpsertAsync(NewEntry("过期", "fact", null, DateTime.UtcNow.AddMinutes(-1)), Vec(1, 0));
        await _store.UpsertAsync(NewEntry("活跃", "fact"), Vec(1, 0));

        var removed = await _store.DeleteExpiredAsync();
        Assert.AreEqual(1, removed);
        Assert.AreEqual(1, (await _store.ListAsync(null, null, 100)).Count);
    }
}
