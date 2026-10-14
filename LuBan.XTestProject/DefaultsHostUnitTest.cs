/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： DefaultsHostUnitTest
*创建时间：2026/10/14
*描述：验证 AddLuBanAgentDefaults 使最小宿主开箱可用（默认上下文/记忆存储/会话管理）
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Defaults;
using LuBan.AIAgent.LocalMemory;
using LuBan.AIAgent.Sessions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class DefaultsHostUnitTest
{
    private static ServiceProvider Build()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLuBanAgentDefaults(config, _ => new MockChatClient("test"));
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    [TestMethod]
    public void Defaults_RegisterContextsAndStores()
    {
        using var sp = Build();
        using var scope = sp.CreateScope();
        var provider = scope.ServiceProvider;

        Assert.IsNotNull(provider.GetRequiredService<ISessionContext>());
        Assert.IsNotNull(provider.GetRequiredService<IWorkspaceContext>());
        Assert.IsNotNull(provider.GetRequiredService<IUserContext>());
        Assert.IsNotNull(provider.GetRequiredService<ILocalMemoryStore>());
        Assert.IsNotNull(provider.GetRequiredService<ISessionManager>());
    }

    [TestMethod]
    public void Defaults_SessionContextNeverThrows()
    {
        using var sp = Build();
        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ISessionContext>();

        Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.EffectiveRoot));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ctx.ResolveTempDirectory()));
        Assert.IsTrue(ctx.IsWithinEffectiveRoot(Path.Combine(ctx.EffectiveRoot, "a.txt")));
    }

    [TestMethod]
    public async Task Defaults_FactoryCreateAsync_DoesNotThrow()
    {
        using var sp = Build();
        using var scope = sp.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<ILuBanAgentFactory>();

        var agent = await factory.CreateAsync(systemPrompt: "测试");
        Assert.IsNotNull(agent);
    }

    [TestMethod]
    public async Task InMemoryLocalMemoryStore_DedupesAndFilters()
    {
        var store = new InMemoryLocalMemoryStore();
        var userId = "u1";

        var first = await store.UpsertAsync(userId, new MemoryEntry
        {
            Content = "喜欢喝咖啡",
            ContentHash = "h1",
            WorkspaceId = null
        }, new byte[] { 1, 2, 3 });

        var second = await store.UpsertAsync(userId, new MemoryEntry
        {
            Content = "喜欢喝咖啡（改）",
            ContentHash = "h1",
            WorkspaceId = null
        }, new byte[] { 4, 5, 6 });

        Assert.AreEqual(first.Id, second.Id, "同 ContentHash 应去重并保留原 Id");

        var list = await store.ListAsync(userId);
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("喜欢喝咖啡（改）", list[0].Content);

        // 全局条目（WorkspaceId = null）对任意工作区可见
        Assert.AreEqual(1, (await store.ListAsync(userId, workspaceId: "ws-other")).Count);

        // 工作区条目仅对本工作区（+全局）可见
        await store.UpsertAsync(userId, new MemoryEntry
        {
            Content = "仅 ws-a 可见",
            ContentHash = "h2",
            WorkspaceId = "ws-a"
        }, new byte[] { 7 });

        Assert.AreEqual(2, (await store.ListAsync(userId, workspaceId: "ws-a")).Count);
        Assert.AreEqual(1, (await store.ListAsync(userId, workspaceId: "ws-b")).Count);

        // category=global：既是类别过滤，也隐含 WorkspaceId IS NULL
        await store.UpsertAsync(userId, new MemoryEntry
        {
            Content = "全局类别",
            ContentHash = "h3",
            Category = MemoryCategories.Global
        }, new byte[] { 8 });
        Assert.AreEqual(1, (await store.ListAsync(userId, category: MemoryCategories.Global, workspaceId: "ws-a")).Count);
    }

    [TestMethod]
    public async Task InMemoryLocalMemoryStore_ExpiryIsFiltered()
    {
        var store = new InMemoryLocalMemoryStore();
        await store.UpsertAsync("u1", new MemoryEntry
        {
            Content = "过期",
            ContentHash = "e1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        }, new byte[] { 1 });

        Assert.AreEqual(0, (await store.ListAsync("u1")).Count);
        Assert.AreEqual(1, await store.DeleteExpiredAsync("u1"));
    }

    [TestMethod]
    public async Task InMemorySessionManager_SupportsHistoryAndCompaction()
    {
        var manager = new InMemorySessionManager();
        var session = await manager.CreateSessionAsync("会话A");

        await manager.AddMessageAsync(session.SessionId, "user", "你好", tokens: 10);
        var reply = await manager.AddMessageAsync(session.SessionId, "assistant", "在的", tokens: 20);

        var all = (await manager.GetMessagesAsync(session.SessionId)).ToList();
        Assert.AreEqual(2, all.Count);

        var latest = (await manager.GetLatestMessagesAsync(session.SessionId, 1)).ToList();
        Assert.AreEqual(reply.Id, latest[0].Id);

        var stats = await manager.GetSessionStatsAsync(session.SessionId);
        Assert.AreEqual(2, stats.TotalMessages);
        Assert.AreEqual(30, stats.TotalTokens);

        await manager.MarkMessagesCompactedAsync(session.SessionId, new[] { all[0].Id });
        var active = (await manager.GetActiveMessagesAsync(session.SessionId)).ToList();
        Assert.AreEqual(1, active.Count);
        Assert.AreEqual(reply.Id, active[0].Id);
    }
}
