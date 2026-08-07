using LuBan.AIAgent.Rules;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Sessions
*文件名： SessionChatHistoryProvider
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：基于 ISessionManager 的会话历史提供者
*
*****************************************************************************/
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

    /// <summary>
    /// 创建会话历史提供者
    /// </summary>
    /// <param name="sessionManager">会话管理器</param>
    /// <param name="chatClient">聊天客户端，用于摘要压缩</param>
    /// <param name="targetCount">摘要压缩的目标消息条数</param>
    /// <param name="threshold">触发摘要压缩的冗余阈值</param>
    /// <param name="ruleEngine">规则引擎，用于 context-build 规则注入（可为 null）</param>
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

    /// <summary>
    /// 构建当前会话的聊天历史，包含对话摘要与 context-build 规则注入消息
    /// </summary>
    /// <param name="context">调用上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天消息列表</returns>
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

    /// <summary>
    /// 将本轮对话的用户输入与助手回复持久化到会话
    /// </summary>
    /// <param name="context">调用完成后的上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
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
