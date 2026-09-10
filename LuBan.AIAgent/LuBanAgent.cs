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
    private readonly Orchestration.AutoOrchestrationMiddleware? _autoOrchestration;
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
    /// <param name="autoOrchestration">自动编排中间件（可选）</param>
    public LuBanAgent(
        ChatClientAgent innerAgent,
        Retrieval.IRetrievalService? retrievalService = null,
        string? retrievalMode = null,
        Orchestration.AutoOrchestrationMiddleware? autoOrchestration = null,
        Sessions.ISessionManager? sessionManager = null,
        Sessions.SessionChatHistoryProvider? historyProvider = null)
    {
        ArgumentNullException.ThrowIfNull(innerAgent);
        _innerAgent = innerAgent;
        _retrievalService = retrievalService;
        _retrievalMode = retrievalMode;
        _autoOrchestration = autoOrchestration;
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
        // 编排判定基于原始用户输入；命中编排则跳过 RAG 注入，直接返回编排结果
        if (await TryOrchestrateAsync(input, cancellationToken) is { } orchestratedResponse)
        {
            return orchestratedResponse;
        }

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
        // 编排判定基于原始用户输入；命中编排则跳过 RAG 注入，直接产出编排结果并写 session
        if (await TryOrchestrateAsync(input, cancellationToken) is { } orchestratedResponse)
        {
            yield return new AgentResponseUpdate
            {
                Contents = [new TextContent(orchestratedResponse.Text ?? "编排节点已完成")]
            };
            yield break;
        }

        var session = await GetOrCreateSessionAsync(cancellationToken);
        input = await PreProcessInputAsync(input, cancellationToken);
        await foreach (var update in _innerAgent.RunStreamingAsync(input, session, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 自动编排前哨：基于原始用户输入判定是否为复合任务并执行编排。
    /// 命中编排时返回编排结果；未命中或未启用编排时返回 null（调用方继续走 RAG + 主 Agent）。
    /// </summary>
    private async Task<AgentResponse?> TryOrchestrateAsync(string input, CancellationToken cancellationToken)
    {
        if (_autoOrchestration == null)
            return null;

        var shouldOrchestrate = await _autoOrchestration.ShouldOrchestrateAsync(input, cancellationToken);
        if (!shouldOrchestrate)
            return null;

        var result = await _autoOrchestration.RunAsync(input, cancellationToken);
        var output = result.FinalOutput ?? "编排节点已完成";

        // 编排分支显式写入 session，保证多轮上下文连续（用户消息 + 编排结果）
        await PersistTurnAsync(input, output, cancellationToken);

        return new AgentResponse
        {
            Messages =
            [
                new ChatMessage(ChatRole.Assistant, new List<AIContent> { new TextContent(output) })
            ]
        };
    }

    /// <summary>
    /// 将用户输入与助手输出持久化到当前会话（用于编排命中时 session 未走 innerAgent 持久化通道的场景）。
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
            Logger.Warn($"编排分支持久化 session 失败: {ex.Message}", ex);
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