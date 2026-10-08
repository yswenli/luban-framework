/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： LuBanAgent
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：Represents a LuBan agent that wraps a ChatClientAgent and manages its session.
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:21:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Represents a LuBan agent that wraps a ChatClientAgent and manages its session.
*
*****************************************************************************/
namespace LuBan.AIAgent;

/// <summary>
/// Represents a LuBan agent that wraps a ChatClientAgent and manages its session.
/// </summary>
public class LuBanAgent
{
    private readonly ChatClientAgent _innerAgent;
    private readonly Retrieval.IRetrievalService? _retrievalService;
    private readonly string? _retrievalMode;
    private readonly Sessions.ISessionManager? _sessionManager;
    private readonly Sessions.SessionChatHistoryProvider? _historyProvider;
    private AgentSession? _session;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    /// <summary>
    /// Represents a LuBan agent that wraps a ChatClientAgent and manages its session.
    /// </summary>
    /// <param name="innerAgent">内部 ChatClientAgent</param>
    /// <param name="retrievalService">语义检索服务（可选）</param>
    /// <param name="retrievalMode">检索模式："auto" 启用自动检索注入</param>
    public LuBanAgent(
        ChatClientAgent innerAgent,
        Retrieval.IRetrievalService? retrievalService = null,
        string? retrievalMode = null,
        Sessions.ISessionManager? sessionManager = null,
        Sessions.SessionChatHistoryProvider? historyProvider = null)
    {
        ArgumentNullException.ThrowIfNull(innerAgent);
        _innerAgent = innerAgent;
        _retrievalService = retrievalService;
        _retrievalMode = retrievalMode;
        _sessionManager = sessionManager;
        _historyProvider = historyProvider;
    }

    /// <summary>
    /// 获取 Agent 的唯一标识。
    /// </summary>
    public string Id => _innerAgent.Id;

    /// <summary>
    /// 获取 Agent 的名称。
    /// </summary>
    public string Name => _innerAgent.Name ?? "LuBanAgent";

    /// <summary>
    /// 获取 Agent 的描述。
    /// </summary>
    public string? Description => _innerAgent.Description;

    private async Task<AgentSession> GetOrCreateSessionAsync(CancellationToken cancellationToken)
    {
        if (_session != null) return _session;

        await _sessionLock.WaitAsync(cancellationToken);
        try
        {
            _session ??= await _innerAgent.CreateSessionAsync(cancellationToken);
            return _session;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// 以字符串输入运行 Agent 并返回响应。
    /// </summary>
    /// <param name="input">用户输入内容。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>Agent 响应结果。</returns>
    public async Task<AgentResponse> RunAsync(string input, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        input = await PreProcessInputAsync(input, cancellationToken);
        return await _innerAgent.RunAsync(input, session, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 以聊天消息列表运行 Agent 并返回响应。
    /// </summary>
    /// <param name="messages">聊天消息集合。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>Agent 响应结果。</returns>
    public async Task<AgentResponse> RunAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        return await _innerAgent.RunAsync(messages, session, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 以字符串输入运行 Agent 并返回流式响应更新。
    /// </summary>
    /// <param name="input">用户输入内容。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>流式响应更新序列。</returns>
    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        string input,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        input = await PreProcessInputAsync(input, cancellationToken);
        await foreach (var update in _innerAgent.RunStreamingAsync(input, session, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 运行一轮对话（带附件）。先执行输入预处理（检索增强/上下文注入），但不进入自动编排分支。
    /// </summary>
    /// <param name="input">用户输入文本。</param>
    /// <param name="attachments">已处理的附件列表。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        string input,
        IReadOnlyList<Attachments.ProcessedAttachment> attachments,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        var rawInput = input;
        input = await PreProcessInputAsync(input, cancellationToken);

        // 通知历史 provider：本轮附件需持久化；无条件设置以清除上一轮残留状态
        _historyProvider?.SetPendingAttachments(attachments);
        // 历史库只存用户原始输入：附件正文由 provider 从附件快照重建，
        // 否则内联正文会随 RequestMessages.Text 入库并与回放内容重复。
        _historyProvider?.SetPendingRawUserInput(rawInput);

        var messages = new[] { Attachments.AttachmentMessageBuilder.Build(input, attachments) };

        await foreach (var update in _innerAgent.RunStreamingAsync(messages, session, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 将用户输入与助手输出持久化到当前会话（供会话未走 innerAgent 持久化通道的旁路场景使用）。
    /// </summary>
    private async Task PersistTurnAsync(string userInput, string assistantOutput, CancellationToken cancellationToken)
    {
        if (_sessionManager?.CurrentSession is not { } session)
            return;

        var sessionId = session.SessionId;
        try
        {
            await _sessionManager.AddMessageAsync(sessionId, "user", userInput, Math.Max(1, userInput.Length / 4));
            if (!string.IsNullOrWhiteSpace(assistantOutput))
            {
                await _sessionManager.AddMessageAsync(sessionId, "assistant", assistantOutput, Math.Max(1, assistantOutput.Length / 4));
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"旁路持久化 session 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 以聊天消息列表运行 Agent 并返回流式响应更新。
    /// </summary>
    /// <param name="messages">聊天消息集合。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>流式响应更新序列。</returns>
    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        await foreach (var update in _innerAgent.RunStreamingAsync(messages, session, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 创建新的 Agent 会话。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>新创建的会话。</returns>
    public async ValueTask<AgentSession> CreateSessionAsync(CancellationToken cancellationToken = default)
        => await _innerAgent.CreateSessionAsync(cancellationToken);

    /// <summary>
    /// 输入预处理：按 retrievalMode 执行语义检索并注入检索上下文。
    /// </summary>
    /// <param name="input">用户输入内容。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>可能附带检索上下文的输入。</returns>
    private async Task<string> PreProcessInputAsync(string input, CancellationToken cancellationToken)
    {
        if (_retrievalMode != "auto" || _retrievalService == null)
            return input;

        try
        {
            var hits = await _retrievalService.SearchAsync(input, topK: 5, cancellationToken: cancellationToken);
            if (hits.Count == 0)
                return input;

            var sb = new StringBuilder();
            sb.AppendLine("### 工作区语义检索上下文（供参考）");
            sb.AppendLine();
            foreach (var hit in hits.Take(5))
            {
                sb.AppendLine($"--- {hit.FilePath}:{hit.StartLine}-{hit.EndLine} (score={hit.Score:F3}) ---");
                sb.AppendLine(hit.Content);
                sb.AppendLine();
            }
            sb.AppendLine("### 检索上下文结束");
            sb.AppendLine();
            sb.AppendLine("请结合以上检索上下文回答用户问题。若检索内容与问题无关，可忽略。");
            sb.AppendLine();
            sb.AppendLine($"用户问题：{input}");

            // 通知 provider 仅持久化原始输入，避免膨胀串污染历史
            _historyProvider?.SetPendingRawUserInput(input);

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Logger.Warn($"自动检索失败，跳过检索注入: {ex.Message}", ex);
            return input;
        }
    }
}