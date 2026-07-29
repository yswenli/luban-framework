#pragma warning disable MAAI001

using LuBan.AIAgent.Sessions;
using Microsoft.Extensions.AI;
using AgentClient = Microsoft.Agents.AI.ChatClientAgent;
using ChatHistoryProvider = Microsoft.Agents.AI.ChatHistoryProvider;

namespace LuBan.AIAgent.Tests;

[TestClass]
public class SessionChatHistoryProviderTests
{
    private sealed class FakeSessionManager : ISessionManager
    {
        private readonly List<SessionMessage> _messages = new();
        private long _nextId = 1;

        public SessionInfo? CurrentSession { get; private set; }

        public List<long> CompactedIds { get; } = new();

        public Task<SessionInfo> CreateSessionAsync(string? userId = null, string? title = null)
        {
            CurrentSession = new SessionInfo { SessionId = "s1", Title = title };
            return Task.FromResult(CurrentSession);
        }

        public void Seed(params (string role, string content)[] messages)
        {
            foreach (var (role, content) in messages)
                _messages.Add(new SessionMessage { Id = _nextId++, SessionId = "s1", Role = role, Content = content });
        }

        public Task<SessionMessage> AddMessageAsync(string sessionId, string role, string content, int? tokens = null)
        {
            var msg = new SessionMessage { Id = _nextId++, SessionId = sessionId, Role = role, Content = content, Tokens = tokens };
            _messages.Add(msg);
            return Task.FromResult(msg);
        }

        public Task<IEnumerable<SessionMessage>> GetActiveMessagesAsync(string sessionId)
            => Task.FromResult(_messages.Where(m => !CompactedIds.Contains(m.Id)).OrderBy(m => m.Role == "summary" ? 0 : 1).ThenBy(m => m.Id).AsEnumerable());

        public Task MarkMessagesCompactedAsync(string sessionId, IEnumerable<long> messageIds)
        {
            CompactedIds.AddRange(messageIds);
            return Task.CompletedTask;
        }

        public Task<SessionInfo?> GetSessionAsync(string sessionId) => throw new NotImplementedException();
        public Task<IEnumerable<SessionInfo>> GetUserSessionsAsync(string userId) => throw new NotImplementedException();
        public Task UpdateSessionTitleAsync(string sessionId, string title) => throw new NotImplementedException();
        public Task DeleteSessionAsync(string sessionId) => throw new NotImplementedException();
        public Task<IEnumerable<SessionMessage>> GetMessagesAsync(string sessionId, int? limit = null) => throw new NotImplementedException();
        public Task ClearMessagesAsync(string sessionId) => throw new NotImplementedException();
        public Task<SessionStats> GetSessionStatsAsync(string sessionId) => throw new NotImplementedException();
        public Task SetCurrentSessionAsync(string sessionId) => throw new NotImplementedException();
        public Task ClearAllSessionsAsync() => throw new NotImplementedException();
        public Task<GlobalSessionStats> GetGlobalStatsAsync(int? days = null) => throw new NotImplementedException();
    }

    private sealed class StubChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "这是摘要")));
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private sealed class TestableProvider(ISessionManager sm, IChatClient client, int target = 20, int threshold = 10)
        : SessionChatHistoryProvider(sm, client, target, threshold)
    {
        public ValueTask<IEnumerable<ChatMessage>> PublicProvide(ChatHistoryProvider.InvokingContext ctx, CancellationToken ct = default)
            => ProvideChatHistoryAsync(ctx, ct);
        public ValueTask PublicStore(ChatHistoryProvider.InvokedContext ctx, CancellationToken ct = default)
            => StoreChatHistoryAsync(ctx, ct);
    }

    private static AgentClient CreateAgent(IChatClient client) => new(client);

    private static ChatHistoryProvider.InvokingContext Invoking(AgentClient agent)
        => new(agent, session: null, Array.Empty<ChatMessage>());

    private static ChatHistoryProvider.InvokedContext Invoked(AgentClient agent, string userText, string responseText)
        => new(agent, session: null,
            new[] { new ChatMessage(ChatRole.User, userText) },
            new[] { new ChatMessage(ChatRole.Assistant, responseText) });

    [TestMethod]
    public async Task Provide_LoadsActiveMessages_AsUserAssistant()
    {
        var sm = new FakeSessionManager();
        await sm.CreateSessionAsync();
        sm.Seed(("user", "你好"), ("assistant", "你好！"));

        var provider = new TestableProvider(sm, new StubChatClient());
        var history = (await provider.PublicProvide(Invoking(CreateAgent(new StubChatClient())))).ToList();

        Assert.AreEqual(2, history.Count);
        Assert.AreEqual(ChatRole.User, history[0].Role);
        Assert.AreEqual(ChatRole.Assistant, history[1].Role);
    }

    [TestMethod]
    public async Task Provide_SummaryMessage_BecomesSystemWithPrefix()
    {
        var sm = new FakeSessionManager();
        await sm.CreateSessionAsync();
        sm.Seed(("summary", "之前聊了天气"), ("user", "继续说"), ("assistant", "好的"));

        var provider = new TestableProvider(sm, new StubChatClient());
        var history = (await provider.PublicProvide(Invoking(CreateAgent(new StubChatClient())))).ToList();

        Assert.AreEqual(ChatRole.System, history[0].Role);
        StringAssert.StartsWith(history[0].Text, "[对话摘要]");
    }

    [TestMethod]
    public async Task Store_PersistsUserAndAssistantMessages()
    {
        var sm = new FakeSessionManager();
        await sm.CreateSessionAsync();
        var agent = CreateAgent(new StubChatClient());
        var provider = new TestableProvider(sm, new StubChatClient());

        await provider.PublicStore(Invoked(agent, "查一下D盘", "D盘有3个目录"));

        var active = (await sm.GetActiveMessagesAsync("s1")).ToList();
        Assert.AreEqual(2, active.Count);
        Assert.AreEqual("user", active[0].Role);
        Assert.AreEqual("assistant", active[1].Role);
        Assert.IsTrue(active[0].Tokens > 0);
    }

    [TestMethod]
    public async Task Store_ConsecutiveDuplicateUserMessages_BothPersisted()
    {
        var sm = new FakeSessionManager();
        await sm.CreateSessionAsync();
        var agent = CreateAgent(new StubChatClient());
        var provider = new TestableProvider(sm, new StubChatClient());

        await provider.PublicStore(Invoked(agent, "继续", "好的"));
        await provider.PublicStore(Invoked(agent, "继续", "好的"));

        var userMessages = (await sm.GetActiveMessagesAsync("s1")).Where(m => m.Role == "user").ToList();
        Assert.AreEqual(2, userMessages.Count);
    }

    [TestMethod]
    public async Task Store_RequestIncludesHistory_OnlyNewInputPersisted()
    {
        var sm = new FakeSessionManager();
        await sm.CreateSessionAsync();
        sm.Seed(("user", "历史问题"), ("assistant", "历史回答"));
        var agent = CreateAgent(new StubChatClient());
        var provider = new TestableProvider(sm, new StubChatClient());

        var ctx = new ChatHistoryProvider.InvokedContext(agent, session: null,
            new[]
            {
                new ChatMessage(ChatRole.User, "历史问题"),
                new ChatMessage(ChatRole.Assistant, "历史回答"),
                new ChatMessage(ChatRole.User, "新问题")
            },
            new[] { new ChatMessage(ChatRole.Assistant, "新回答") });
        await provider.PublicStore(ctx);

        var userMessages = (await sm.GetActiveMessagesAsync("s1")).Where(m => m.Role == "user").ToList();
        Assert.AreEqual(2, userMessages.Count);
        Assert.AreEqual("新问题", userMessages[1].Content);
    }

    [TestMethod]
    public async Task Provide_OverThreshold_CompactsAndPersistsSummary()
    {
        var sm = new FakeSessionManager();
        await sm.CreateSessionAsync();
        var seed = Enumerable.Range(1, 35)
            .Select(i => (i % 2 == 1 ? "user" : "assistant", $"消息{i}"))
            .ToArray();
        sm.Seed(seed);

        var provider = new TestableProvider(sm, new StubChatClient(), target: 20, threshold: 10);
        var history = (await provider.PublicProvide(Invoking(CreateAgent(new StubChatClient())))).ToList();

        Assert.AreEqual(ChatRole.System, history[0].Role);
        StringAssert.StartsWith(history[0].Text, "[对话摘要]");
        Assert.IsTrue(history.Count <= 22, $"Expected history.Count <= 22, but got {history.Count}");
        Assert.IsTrue(sm.CompactedIds.Count >= 14, $"Expected CompactedIds.Count >= 14, but got {sm.CompactedIds.Count}");
        var active = (await sm.GetActiveMessagesAsync("s1")).ToList();
        Assert.AreEqual("summary", active[0].Role);
    }
}
