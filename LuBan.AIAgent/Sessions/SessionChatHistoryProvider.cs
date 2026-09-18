using LuBan.AIAgent.Rules;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
    private readonly ContextInjectBuilder _contextInjectBuilder;
    private string? _pendingRawUserInput;
    private IReadOnlyList<Attachments.ProcessedAttachment>? _pendingAttachments;

    /// <summary>历史重建时，最近多少轮 user 消息的附件会被重新附加（图片与文本同规则）。</summary>
    private const int RecentImageReattachTurns = 3;

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
        _contextInjectBuilder = new ContextInjectBuilder(ruleEngine);
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

        // 分离摘要消息与正文消息（摘要在库中按 CreateTime 排序，位置不固定）
        var summaries = active.Where(m => m.Role == "summary").OrderByDescending(m => m.Id).ToList();
        var messages = active.Where(m => m.Role != "summary").ToList();
        SessionMessage? latestSummary = summaries.FirstOrDefault();

        // context-build 规则注入（如记忆召回），仅消费 Inject，忽略 Allow。
        // 必须先于空历史判断：新会话首轮无消息时也要能召回长期记忆。
        var lastUserText = context.RequestMessages?.LastOrDefault(m => m.Role == ChatRole.User)?.Text;
        var recallMessages = (await _contextInjectBuilder.BuildAsync(lastUserText))
            .Select(s => new ChatMessage(ChatRole.System, s))
            .ToList();

        // 无历史消息：仍返回召回内容
        if (active.Count == 0)
            return recallMessages;

        var history = await BuildHistoryAsync(messages, cancellationToken);

        if (history.Count > _targetCount + _threshold)
        {
            try
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

                    // 先写摘要、后归档：反序执行时若归档成功而写摘要失败，历史会被永久裁掉且无摘要兜底。
                    var summaryText = reduced[0].Text ?? "";
                    await _sessionManager.AddMessageAsync(sessionId, "summary", summaryText, EstimateTokens(summaryText));
                    await _sessionManager.MarkMessagesCompactedAsync(sessionId, compactedIds);

                    latestSummary = new SessionMessage { Id = long.MaxValue, Role = "summary", Content = summaryText };
                    history = await BuildHistoryAsync(keptTail, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                var info = ApiErrorClassifier.Classify(ex, cancellationToken.IsCancellationRequested);
                if (info.Category == AgentApiErrorCategory.Canceled)
                    throw;

                // 压缩失败不打断主对话：跳过压缩，用未压缩历史继续
                Logger.Warn($"历史摘要压缩失败（{info.Category}），已跳过压缩继续对话", ex);
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

        // RequestMessages 仅含本轮新输入（框架默认存储过滤器已排除 ChatHistory 来源的历史消息），
        // 仅持久化最后一条 user 消息（本轮新输入）
        // 若外层已设置原始输入（RAG 注入场景），优先持久化原始输入，避免膨胀串污染历史
        var newUserText = _pendingRawUserInput;
        _pendingRawUserInput = null;
        newUserText ??= context.RequestMessages
            .LastOrDefault(m => m.Role == ChatRole.User)?.Text;

        var pending = _pendingAttachments;
        _pendingAttachments = null;
        string? attachmentsJson = null;
        if (pending is { Count: > 0 })
        {
            var records = pending.Select(a => new AttachmentRecord
            {
                FileName = a.Info.FileName,
                MediaType = a.Info.MediaType,
                FileSize = a.Info.FileSize,
                SourcePath = a.Info.SourcePath,
                Kind = a.Info.Kind.ToString()
            }).ToList();
            attachmentsJson = System.Text.Json.JsonSerializer.Serialize(records);
        }

        if (!string.IsNullOrWhiteSpace(newUserText) || attachmentsJson != null)
        {
            await _sessionManager.AddMessageAsync(sessionId, "user", newUserText ?? "", EstimateTokens(newUserText ?? ""), attachments: attachmentsJson);
        }

        if (context.ResponseMessages == null) return;

        var responseText = string.Concat(context.ResponseMessages
            .SelectMany(m => m.Contents?.OfType<TextContent>() ?? Enumerable.Empty<TextContent>())
            .Select(c => c.Text));
        // 提取 AI 思考内容（reasoning），持久化以便切换会话后恢复展示
        var thinkingText = string.Concat(context.ResponseMessages
            .SelectMany(m => m.Contents?.OfType<TextReasoningContent>() ?? Enumerable.Empty<TextReasoningContent>())
            .Select(c => c.Text));
        if (!string.IsNullOrWhiteSpace(responseText))
        {
            await _sessionManager.AddMessageAsync(sessionId, "assistant", responseText, EstimateTokens(responseText), thinkingText);
        }
    }

    /// <summary>
    /// 设置本轮待持久化的原始用户输入。RAG 注入场景下由外层调用，
    /// 覆盖膨胀后的输入，确保历史库仅保存用户原始问题。
    /// 消费完后自动清空。
    /// </summary>
    /// <param name="rawUserInput">用户原始输入。</param>
    public void SetPendingRawUserInput(string? rawUserInput)
    {
        _pendingRawUserInput = rawUserInput;
    }

    /// <summary>
    /// 设置本轮待持久化的附件（由 <c>LuBanAgent</c> 在发起请求前注入），消费后自动清空。
    /// </summary>
    /// <param name="attachments">已处理的附件列表；传空列表表示清除残留状态。</param>
    public void SetPendingAttachments(IReadOnlyList<Attachments.ProcessedAttachment> attachments)
    {
        _pendingAttachments = attachments.Count > 0 ? attachments : null;
    }

    /// <summary>
    /// 按“从最新往旧”的 user 轮次计数重建历史；最近 N 轮的附件重新附加，更早降级为占位文本。
    /// </summary>
    private async Task<List<ChatMessage>> BuildHistoryAsync(
        IReadOnlyList<SessionMessage> messages, CancellationToken ct)
    {
        var result = new List<ChatMessage>(messages.Count);
        var userTurnsFromEnd = 0;
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var m = messages[i];
            var isUser = m.Role == "user";
            if (isUser) userTurnsFromEnd++;
            result.Insert(0, await RebuildMessageAsync(m, isUser, userTurnsFromEnd, ct));
        }
        return result;
    }

    private async Task<ChatMessage> RebuildMessageAsync(
        SessionMessage m, bool isUser, int userTurnsFromEnd, CancellationToken ct)
    {
        var role = MapRole(m.Role);
        if (!isUser || string.IsNullOrWhiteSpace(m.Attachments))
            return new ChatMessage(role, m.Content);

        List<AttachmentRecord>? records;
        try { records = System.Text.Json.JsonSerializer.Deserialize<List<AttachmentRecord>>(m.Attachments); }
        catch { records = null; }
        if (records is not { Count: > 0 })
            return new ChatMessage(role, m.Content);

        var contents = new List<AIContent>();
        if (!string.IsNullOrEmpty(m.Content))
            contents.Add(new TextContent(m.Content));

        foreach (var r in records)
        {
            var isImage = string.Equals(r.Kind, "Image", StringComparison.OrdinalIgnoreCase);
            var label = isImage ? "图片" : "附件";

            if (userTurnsFromEnd > RecentImageReattachTurns)
            {
                contents.Add(new TextContent($"[历史{label}: {r.FileName}]"));
                continue;
            }

            if (!File.Exists(r.SourcePath))
            {
                contents.Add(new TextContent($"[附件: {r.FileName} - 文件已不存在]"));
                continue;
            }

            try
            {
                if (isImage)
                    contents.Add(new DataContent(await File.ReadAllBytesAsync(r.SourcePath, ct), r.MediaType));
                else
                    contents.Add(new TextContent(await File.ReadAllTextAsync(r.SourcePath, ct)));
            }
            catch
            {
                contents.Add(new TextContent($"[附件: {r.FileName} - 读取失败]"));
            }
        }

        return new ChatMessage(role, contents);
    }

    private static int EstimateTokens(string text) => Math.Max(1, text.Length / 4);

    private static ChatRole MapRole(string role) => role switch
    {
        "user" => ChatRole.User,
        "assistant" => ChatRole.Assistant,
        "system" => ChatRole.System,
        _ => ChatRole.Assistant
    };
}
