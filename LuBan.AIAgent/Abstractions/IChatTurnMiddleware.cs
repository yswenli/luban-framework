namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天回合中间件接口，用于处理聊天回合执行过程中的逻辑
/// </summary>
public interface IChatTurnMiddleware
{
    /// <summary>
    /// 异步调用中间件
    /// </summary>
    /// <param name="context">聊天回合执行上下文</param>
    /// <param name="next">下一个中间件的委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    Task<ChatTurnResult> InvokeAsync(
        ChatTurnExecutionContext context,
        Func<Task<ChatTurnResult>> next,
        CancellationToken cancellationToken = default);
}
