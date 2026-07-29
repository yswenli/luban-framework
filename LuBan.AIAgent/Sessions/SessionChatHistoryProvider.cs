/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Sessions
*文件名： SessionChatHistoryProvider
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/29
*描述：基于 ISessionManager 的会话历史提供者
*
*****************************************************************************/

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Sessions;

public class SessionChatHistoryProvider : ChatHistoryProvider
{
    private readonly ISessionManager _sessionManager;
    private readonly IChatClient _chatClient;
    private readonly int _targetCount;
    private readonly int _threshold;

    public SessionChatHistoryProvider(
        ISessionManager sessionManager,
        IChatClient chatClient,
        int targetCount = 20,
        int threshold = 10)
        : base(null, null, null)
    {
        _sessionManager = sessionManager;
        _chatClient = chatClient;
        _targetCount = targetCount;
        _threshold = threshold;
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

        var firstIsSummary = active[0].Role == "summary";

        var history = active
            .Select(m => new ChatMessage(m.Role == "user" ? ChatRole.User : ChatRole.Assistant, m.Content))
            .ToList();

        if (history.Count > _targetCount + _threshold)
        {
#pragma warning disable MEAI001
            var reducer = new SummarizingChatReducer(_chatClient, _targetCount, _threshold);
#pragma warning restore MEAI001
            var reduced = (await reducer.ReduceAsync(history, cancellationToken)).ToList();

            if (reduced.Count < history.Count)
            {
                var keptCount = reduced.Count - 1;
                var compactedIds = active.Take(active.Count - keptCount).Select(m => m.Id).ToList();
                await _sessionManager.MarkMessagesCompactedAsync(sessionId, compactedIds);

                var summaryText = reduced[0].Text ?? "";
                await _sessionManager.AddMessageAsync(sessionId, "summary", summaryText, EstimateTokens(summaryText));

                history = reduced;
                firstIsSummary = true;
            }
        }

        var feed = new List<ChatMessage>(history.Count);
        for (var i = 0; i < history.Count; i++)
        {
            feed.Add(i == 0 && firstIsSummary
                ? new ChatMessage(ChatRole.System, "[对话摘要] " + history[i].Text)
                : history[i]);
        }
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
