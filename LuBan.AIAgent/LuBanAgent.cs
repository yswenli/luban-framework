/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： GloabUsing
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
    private AgentSession? _session;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    /// <summary>
    /// Represents a LuBan agent that wraps a ChatClientAgent and manages its session.
    /// </summary>
    /// <param name="innerAgent"></param>
    public LuBanAgent(ChatClientAgent innerAgent)
    {
        ArgumentNullException.ThrowIfNull(innerAgent);
        _innerAgent = innerAgent;
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
        await foreach (var update in _innerAgent.RunStreamingAsync(input, session, cancellationToken: cancellationToken))
        {
            yield return update;
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
}