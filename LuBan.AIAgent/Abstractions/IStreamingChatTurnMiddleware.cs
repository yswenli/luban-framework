namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 流式聊天回合中间件接口，用于处理流式聊天回合执行过程中的逻辑
/// </summary>
public interface IStreamingChatTurnMiddleware
{
    /// <summary>
    /// 异步调用中间件并返回流式更新
    /// </summary>
    /// <param name="context">流式聊天回合执行上下文</param>
    /// <param name="next">下一个中间件的委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    IAsyncEnumerable<ChatTurnStreamUpdate> InvokeAsync(
        StreamingChatTurnExecutionContext context,
        Func<IAsyncEnumerable<ChatTurnStreamUpdate>> next,
        CancellationToken cancellationToken = default);
}
