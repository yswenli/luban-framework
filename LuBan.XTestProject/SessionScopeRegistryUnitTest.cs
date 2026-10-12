/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： SessionScopeRegistryUnitTest
*创建时间：2026/10/11
*描述：验证规则/技能/MCP 注册表按会话作用域隔离，支持跨工作区并发
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.LocalMemory;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.Rules;
using LuBan.AIAgent.Skills;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

[TestClass]
public class SessionScopeRegistryUnitTest
{
    private static ServiceProvider Build()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLuBanAgent(config, _ => new MockChatClient("test"));
        // ILocalMemoryStore 由宿主注册，本单测仅需可解析（不调用具体方法）
        services.AddSingleton<ILocalMemoryStore, NullLocalMemoryStore>();
        // 仅启用 ValidateScopes：本单测未注册宿主负责的具体工厂，全图 ValidateOnBuild 由 agent 宿主启用。
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private sealed class NullLocalMemoryStore : ILocalMemoryStore
    {
        public Task<MemoryEntry> UpsertAsync(MemoryEntry entry, byte[] vectorBytes, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<MemoryEntry>> ListAsync(string? category = null, string? workspaceId = null, int limit = 100, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadAllAsync(string? category = null, string? workspaceId = null, bool includeAllWorkspaces = false, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<(MemoryEntry Entry, byte[] VectorBytes)>> LoadByIdsAsync(IEnumerable<string> ids, string? workspaceId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    [TestMethod]
    public void RuleEngine_DiffersPerSessionScope()
    {
        using var sp = Build();
        using var a = sp.CreateScope();
        using var b = sp.CreateScope();
        Assert.AreNotSame(
            a.ServiceProvider.GetRequiredService<RuleEngine>(),
            b.ServiceProvider.GetRequiredService<RuleEngine>());
    }

    [TestMethod]
    public void SkillRegistry_DiffersPerSessionScope()
    {
        using var sp = Build();
        using var a = sp.CreateScope();
        using var b = sp.CreateScope();
        Assert.AreNotSame(
            a.ServiceProvider.GetRequiredService<SkillRegistry>(),
            b.ServiceProvider.GetRequiredService<SkillRegistry>());
    }

    [TestMethod]
    public void McpRegistry_DiffersPerSessionScope()
    {
        using var sp = Build();
        using var a = sp.CreateScope();
        using var b = sp.CreateScope();
        Assert.AreNotSame(
            a.ServiceProvider.GetRequiredService<MCPRegistry>(),
            b.ServiceProvider.GetRequiredService<MCPRegistry>());
    }
}