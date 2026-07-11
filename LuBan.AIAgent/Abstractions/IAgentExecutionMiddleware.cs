namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 代理执行中间件接口，用于处理代理执行过程中的逻辑
/// </summary>
public interface IAgentExecutionMiddleware
{
    /// <summary>
    /// 异步调用中间件
    /// </summary>
    /// <param name="context">代理执行上下文</param>
    /// <param name="next">下一个中间件的委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>代理执行结果</returns>
    Task<AgentResult> InvokeAsync(
        AgentExecutionContext context,
        Func<Task<AgentResult>> next,
        CancellationToken cancellationToken = default);
}
