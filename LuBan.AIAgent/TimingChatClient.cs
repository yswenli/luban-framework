/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*命名空间：LuBan.AIAgent
*文件名： TimingChatClient
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/16
*描述：LLM 往返耗时埋点客户端，用于定位子 Agent 节点超时的时间去向
*
*****************************************************************************/
namespace LuBan.AIAgent;

/// <summary>
/// LLM 往返耗时埋点客户端。
/// 包裹在最内层聊天客户端外侧（<c>FunctionInvokingChatClient</c> 之内），
/// 因此每一次调用即一次 LLM 往返：调用序号等于工具循环轮次，
/// 可据此区分「工具轮次过多」与「单次 LLM 响应过慢」。
/// </summary>
public class TimingChatClient : IChatClient
{
    private readonly IChatClient _inner;
    private readonly string _tag;
    private int _callCount;

    /// <summary>
    /// 创建埋点客户端。
    /// </summary>
    /// <param name="inner">被包装的聊天客户端。</param>
    /// <param name="tag">日志标识，通常为节点 Id。</param>
    public TimingChatClient(IChatClient inner, string tag)
    {
        _inner = inner;
        _tag = tag;
    }

    /// <inheritdoc/>
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var round = Interlocked.Increment(ref _callCount);
        var inputLen = 0;
        foreach (var m in messages)
        {
            foreach (var c in m.Contents)
            {
                if (c is TextContent t) inputLen += t.Text?.Length ?? 0;
            }
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _inner.GetResponseAsync(messages, options, cancellationToken);
            sw.Stop();

            var toolNames = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .Select(t => t.Name)
                .ToList();
            var textLen = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<TextContent>()
                .Sum(t => t.Text?.Length ?? 0);

            Logger.Debug($"[OrchDiag] {_tag} llm#{round} elapsed={sw.Elapsed.TotalSeconds:F1}s inputLen={inputLen} tools={toolNames.Count}[{string.Join(",", toolNames)}] textLen={textLen}");
            return response;
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            Logger.Debug($"[OrchDiag] {_tag} llm#{round} cancelled elapsed={sw.Elapsed.TotalSeconds:F1}s inputLen={inputLen}");
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Logger.Debug($"[OrchDiag] {_tag} llm#{round} failed elapsed={sw.Elapsed.TotalSeconds:F1}s inputLen={inputLen}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var round = Interlocked.Increment(ref _callCount);
        var sw = Stopwatch.StartNew();
        var toolCalls = 0;
        var textLen = 0;
        try
        {
            await foreach (var update in _inner.GetStreamingResponseAsync(messages, options, cancellationToken))
            {
                toolCalls += update.Contents.OfType<FunctionCallContent>().Count();
                textLen += update.Contents.OfType<TextContent>().Sum(t => t.Text?.Length ?? 0);
                yield return update;
            }
        }
        finally
        {
            sw.Stop();
            Logger.Debug($"[OrchDiag] {_tag} llm#{round} (stream) elapsed={sw.Elapsed.TotalSeconds:F1}s tools={toolCalls} textLen={textLen}");
        }
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? key = null)
        => _inner.GetService(serviceType, key);

    /// <inheritdoc/>
    public void Dispose()
    {
        _inner.Dispose();
        GC.SuppressFinalize(this);
    }
}