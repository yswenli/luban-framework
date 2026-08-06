using LuBan.AIAgent.Rules;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Sessions;

/// <summary>
/// 基于 ISessionManager 的会话历史提供者，支持摘要压缩与 context-build 规则注入
/// </summary>
public class SessionChatHistoryProvider : ChatHistoryProvider
{
    private readonly ISessionManager _sessionManager;
    private readonly IChatClient _chatClient;
    private readonly int _targetCount;
    private readonly int _threshold;
    private readonly RuleEngine? _ruleEngine;

    public SessionChatHistoryProvider(
        ISessionManager sessionManager,
        IChatClient chatClient,
        int targetCount = 20,
        int threshold = 10,
        RuleEngine? ruleEngine = null)
        : base(null, null, null)
    {
        _sessionManager = sessionManager;
        _chatClient = chatClient;
        _targetCount = targetCount;
        _threshold = threshold;
        _ruleEngine = ruleEngine;
    }

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context, CancellationToken cancellationToken = default)
    {
        var sessionId = _sessionManager.CurrentSession?.SessionId;
        if (string.IsNullOrEmpty(sessionId))
            return Array.Empty<ChatMessage>();

        var active = (await _sessionManager.GetActiveMessagesAsync(sessionId)).ToList();
        if (active.Count == 0)
            return Array.Empty<ChatMessage>();

        // 分离摘要消息与正文消息（摘要在库中按 CreateTime 排序，位置不固定）
        var summaries = active.Where(m => m.Role == "summary").OrderByDescending(m => m.Id).ToList();
        var messages = active.Where(m => m.Role != "summary").ToList();
        SessionMessage? latestSummary = summaries.FirstOrDefault();

        // context-build 规则注入（如记忆召回），仅消费 Inject，忽略 Allow
        var recallMessages = new List<ChatMessage>();
        if (_ruleEngine != null)
        {
            var lastUserText = context.RequestMessages?.LastOrDefault(m => m.Role == ChatRole.User)?.Text;
            if (!string.IsNullOrWhiteSpace(lastUserText))
            {
                try
                {
                    var eval = await _ruleEngine.EvaluateAsync(new RuleContext
                    {
                        ActionType = "context-build",
                        UserInput = lastUserText
                    });
                    recallMessages = eval.Inject
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => new ChatMessage(ChatRole.System, s))
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error("context-build 规则评估失败", ex, sessionId);
                }
            }
        }

        var history = messages
            .Select(m => new ChatMessage(m.Role == "user" ? ChatRole.User : ChatRole.Assistant, m.Content))
            .ToList();

        if (history.Count > _targetCount + _threshold)
        {
#pragma warning disable MEAI001
            var reducer = new SummarizingChatReducer(_chatClient, _targetCount, _threshold);
#pragma warning restore MEAI001
            var reduced = (await reducer.ReduceAsync(history, cancellationToken)).ToList();

            if (reduced.Count > 0 && reduced.Count < history.Count)
            {
                var keptCount = Math.Min(reduced.Count - 1, history.Count);
                var keptTail = messages.Skip(messages.Count - keptCount).ToList();
                var compactedIds = messages.Take(messages.Count - keptCount).Select(m => m.Id)
                    .Concat(summaries.Select(s => s.Id))
                    .ToList();
                await _sessionManager.MarkMessagesCompactedAsync(sessionId, compactedIds);

                var summaryText = reduced[0].Text ?? "";
                await _sessionManager.AddMessageAsync(sessionId, "summary", summaryText, EstimateTokens(summaryText));

                latestSummary = new SessionMessage { Id = long.MaxValue, Role = "summary", Content = summaryText };
                history = keptTail
                    .Select(m => new ChatMessage(m.Role == "user" ? ChatRole.User : ChatRole.Assistant, m.Content))
                    .ToList();
            }
        }

        var feed = new List<ChatMessage>();
        if (latestSummary != null && !string.IsNullOrWhiteSpace(latestSummary.Content))
            feed.Add(new ChatMessage(ChatRole.System, "[对话摘要] " + latestSummary.Content));
        feed.AddRange(recallMessages);
        feed.AddRange(history);
        return feed;
    }

    protected override async ValueTask StoreChatHistoryAsync(
        InvokedContext context, CancellationToken cancellationToken = default)
    {
        var sessionId = _sessionManager.CurrentSession?.SessionId;
        if (string.IsNullOrEmpty(sessionId))
            return;

        // RequestMessages 包含历史消息 + 本轮新输入，历史已在之前轮次持久化，
        // 仅持久化最后一条 user 消息（本轮新输入）
        var newUserText = context.RequestMessages
            .LastOrDefault(m => m.Role == ChatRole.User)?.Text;
        if (!string.IsNullOrWhiteSpace(newUserText))
        {
            await _sessionManager.AddMessageAsync(sessionId, "user", newUserText, EstimateTokens(newUserText));
        }

        if (context.ResponseMessages == null) return;

        var responseText = string.Concat(context.ResponseMessages
            .SelectMany(m => m.Contents?.OfType<TextContent>() ?? Enumerable.Empty<TextContent>())
            .Select(c => c.Text));
        if (!string.IsNullOrWhiteSpace(responseText))
        {
            await _sessionManager.AddMessageAsync(sessionId, "assistant", responseText, EstimateTokens(responseText));
        }
    }

    private static int EstimateTokens(string text) => Math.Max(1, text.Length / 4);
}
