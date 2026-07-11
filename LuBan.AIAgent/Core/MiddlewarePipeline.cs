using System.Collections.Generic;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 中间件管道，用于执行中间件链
/// </summary>
internal static class MiddlewarePipeline
{
    /// <summary>
    /// 异步执行中间件管道
    /// </summary>
    /// <typeparam name="TMiddleware">中间件类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="middlewares">中间件集合</param>
    /// <param name="context">上下文</param>
    /// <param name="invoke">调用中间件的委托</param>
    /// <param name="terminal">终端委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    public static Task<TResult> ExecuteAsync<TMiddleware, TContext, TResult>(
        IEnumerable<TMiddleware> middlewares,
        TContext context,
        Func<TMiddleware, TContext, Func<Task<TResult>>, CancellationToken, Task<TResult>> invoke,
        Func<Task<TResult>> terminal,
        CancellationToken cancellationToken)
    {
        var pipeline = middlewares.Reverse()
            .Aggregate(terminal, (next, middleware) => () => invoke(middleware, context, next, cancellationToken));

        return pipeline();
    }

    /// <summary>
    /// 流式执行中间件管道
    /// </summary>
    /// <typeparam name="TMiddleware">中间件类型</typeparam>
    /// <typeparam name="TContext">上下文类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="middlewares">中间件集合</param>
    /// <param name="context">上下文</param>
    /// <param name="invoke">调用中间件的委托</param>
    /// <param name="terminal">终端委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果的异步枚举</returns>
    public static IAsyncEnumerable<TResult> ExecuteStreaming<TMiddleware, TContext, TResult>(
        IEnumerable<TMiddleware> middlewares,
        TContext context,
        Func<TMiddleware, TContext, Func<IAsyncEnumerable<TResult>>, CancellationToken, IAsyncEnumerable<TResult>> invoke,
        Func<IAsyncEnumerable<TResult>> terminal,
        CancellationToken cancellationToken)
    {
        var pipeline = middlewares.Reverse()
            .Aggregate(terminal, (next, middleware) => () => invoke(middleware, context, next, cancellationToken));

        return pipeline();
    }
}
