/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： WebAgentSessionRegistryUnitTest
*版本号： V1.0.0.0
*唯一标识：c9b2e5a0-7f41-4d63-8e95-1a6c4b0d9e27
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：Web 会话注册表（含归属解析）单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Web 会话注册表（含归属解析）单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Sessions;
using LuBan.Web.Core.AIAgent;

namespace LuBan.XTestProject;

[TestClass]
public class WebAgentSessionRegistryUnitTest
{
    [TestMethod]
    public async Task NoRequestedSession_CreatesOnce_ThenReuses()
    {
        var sessions = new FakeSessionManager();
        var sut = new WebAgentSessionRegistry();

        var first = await sut.ResolveAsync(1, null, sessions);
        var second = await sut.ResolveAsync(1, null, sessions);

        Assert.IsTrue(first.Created);
        Assert.AreEqual(WebSessionStatus.Ok, first.Status);
        Assert.IsFalse(second.Created);
        Assert.AreEqual(first.SessionId, second.SessionId);
        Assert.AreEqual(1, sessions.Created.Count);
    }

    [TestMethod]
    public async Task DifferentUsers_GetDifferentSessions()
    {
        var sessions = new FakeSessionManager();
        var sut = new WebAgentSessionRegistry();

        var a = await sut.ResolveAsync(1, null, sessions);
        var b = await sut.ResolveAsync(2, null, sessions);

        Assert.AreNotEqual(a.SessionId, b.SessionId);
        Assert.IsTrue(sut.IsOwnedBy(a.SessionId, 1));
        Assert.IsFalse(sut.IsOwnedBy(a.SessionId, 2));
    }

    [TestMethod]
    public async Task RequestedOwnSession_IsReused()
    {
        var sessions = new FakeSessionManager();
        var sut = new WebAgentSessionRegistry();
        var created = await sut.ResolveAsync(1, null, sessions);

        var resolved = await sut.ResolveAsync(1, created.SessionId, sessions);

        Assert.AreEqual(WebSessionStatus.Ok, resolved.Status);
        Assert.IsFalse(resolved.Created);
        Assert.AreEqual(created.SessionId, resolved.SessionId);
    }

    [TestMethod]
    public async Task RequestedMissingSession_IsNotFound()
    {
        var sut = new WebAgentSessionRegistry();

        var resolved = await sut.ResolveAsync(1, "no-such", new FakeSessionManager());

        Assert.AreEqual(WebSessionStatus.NotFound, resolved.Status);
    }

    [TestMethod]
    public async Task RequestedOtherUsersSession_IsForbidden()
    {
        var sessions = new FakeSessionManager();
        sessions.Seed("other-1", userId: "2");
        var sut = new WebAgentSessionRegistry();

        var resolved = await sut.ResolveAsync(1, "other-1", sessions);

        Assert.AreEqual(WebSessionStatus.Forbidden, resolved.Status);
        Assert.IsFalse(sut.IsOwnedBy("other-1", 1));
    }

    [TestMethod]
    public async Task RequestedSessionOfSameUser_SwitchesPointer()
    {
        var sessions = new FakeSessionManager();
        sessions.Seed("mine-old", userId: "1");
        var sut = new WebAgentSessionRegistry();
        var current = await sut.ResolveAsync(1, null, sessions);

        var resolved = await sut.ResolveAsync(1, "mine-old", sessions);

        Assert.AreEqual(WebSessionStatus.Ok, resolved.Status);
        Assert.IsTrue(sut.IsOwnedBy("mine-old", 1));
        Assert.IsFalse(sut.IsOwnedBy(current.SessionId, 1));
    }

    private sealed class FakeSessionManager : ISessionManager
    {
        private readonly Dictionary<string, SessionInfo> _store = new(StringComparer.Ordinal);

        /// <summary>按创建顺序记录，便于断言「只建了一次」。</summary>
        public List<string> Created { get; } = [];

        public void Seed(string sessionId, string? userId)
            => _store[sessionId] = new SessionInfo { SessionId = sessionId, UserId = userId };

        public Task<SessionInfo> CreateSessionAsync(string? title = null, string? workspaceId = null)
        {
            var session = new SessionInfo { SessionId = Guid.NewGuid().ToString("N"), Title = title };
            _store[session.SessionId] = session;
            Created.Add(session.SessionId);
            return Task.FromResult(session);
        }

        public Task<SessionInfo?> GetSessionAsync(string sessionId)
            => Task.FromResult(_store.TryGetValue(sessionId, out var s) ? s : null);

        public Task<IEnumerable<SessionInfo>> GetUserSessionsAsync() => throw new NotSupportedException();
        public Task UpdateSessionTitleAsync(string sessionId, string title) => throw new NotSupportedException();
        public Task DeleteSessionAsync(string sessionId) => throw new NotSupportedException();
        public Task<SessionMessage> AddMessageAsync(string sessionId, string role, string content, int? tokens = null,
            string? thinking = null, string? attachments = null, string? toolCalls = null)
            => throw new NotSupportedException();
        public Task<IEnumerable<SessionMessage>> GetMessagesAsync(string sessionId, int? limit = null) => throw new NotSupportedException();
        public Task<IEnumerable<SessionMessage>> GetLatestMessagesAsync(string sessionId, int count) => throw new NotSupportedException();
        public Task ClearMessagesAsync(string sessionId) => throw new NotSupportedException();
        public Task<SessionStats> GetSessionStatsAsync(string sessionId) => throw new NotSupportedException();
        public Task SetCurrentSessionAsync(string sessionId) => throw new NotSupportedException();
        public void ClearCurrentSession() => throw new NotSupportedException();
        public SessionInfo? CurrentSession => null;
        public Task ClearAllSessionsAsync(string? workspaceId = null) => throw new NotSupportedException();
        public Task<GlobalSessionStats> GetGlobalStatsAsync(int? days = null) => throw new NotSupportedException();
        public Task<IEnumerable<SessionMessage>> GetActiveMessagesAsync(string sessionId) => throw new NotSupportedException();
        public Task MarkMessagesCompactedAsync(string sessionId, IEnumerable<long> messageIds) => throw new NotSupportedException();
    }
}