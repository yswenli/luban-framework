namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行中间件接口，用于处理工具执行过程中的逻辑
/// </summary>
public interface IToolExecutionMiddleware
{
    /// <summary>
    /// 异步调用中间件
    /// </summary>
    /// <param name="context">工具执行中间件上下文</param>
    /// <param name="next">下一个中间件的委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    Task<ToolExecutionOutcome> InvokeAsync(
        ToolExecutionMiddlewareContext context,
        Func<Task<ToolExecutionOutcome>> next,
        CancellationToken cancellationToken = default);
}
