using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 日志流式聊天回合中间件，用于记录流式聊天回合的执行情况
/// </summary>
/// <param name="logger">日志记录器</param>
/// <param name="options">日志中间件选项</param>
public sealed class LoggingStreamingChatTurnMiddleware(
    ILogger<LoggingStreamingChatTurnMiddleware>? logger = null,
    LoggingMiddlewareOptions? options = null) : IStreamingChatTurnMiddleware
{
    private readonly ILogger<LoggingStreamingChatTurnMiddleware>? _logger = logger;
    private readonly LoggingMiddlewareOptions _options = options ?? new LoggingMiddlewareOptions();

    /// <summary>
    /// 异步调用中间件
    /// </summary>
    /// <param name="context">流式聊天回合执行上下文</param>
    /// <param name="next">下一个中间件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    public async IAsyncEnumerable<ChatTurnStreamUpdate> InvokeAsync(
        StreamingChatTurnExecutionContext context,
        Func<IAsyncEnumerable<ChatTurnStreamUpdate>> next,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_options.LogInputs)
        {
            _logger?.LogInformation(
                "Starting streaming chat turn. Kind={Kind}, ModelId={ModelId}, Input={Input}, MessageCount={MessageCount}",
                context.Kind,
                context.ModelId,
                context.Input,
                _options.IncludeMessageCounts ? context.Messages.Count : 0);
        }
        else
        {
            _logger?.LogInformation(
                "Starting streaming chat turn. Kind={Kind}, ModelId={ModelId}, InputLength={InputLength}, MessageCount={MessageCount}",
                context.Kind,
                context.ModelId,
                context.Input?.Length ?? 0,
                _options.IncludeMessageCounts ? context.Messages.Count : 0);
        }

        await foreach (var update in StreamUpdatesAsync(context, next, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 流式处理更新
    /// </summary>
    /// <param name="context">流式聊天回合执行上下文</param>
    /// <param name="next">下一个中间件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    private async IAsyncEnumerable<ChatTurnStreamUpdate> StreamUpdatesAsync(
        StreamingChatTurnExecutionContext context,
        Func<IAsyncEnumerable<ChatTurnStreamUpdate>> next,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var enumerator = next().GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            ChatTurnStreamUpdate update;
            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    yield break;
                }

                update = enumerator.Current;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Streaming chat turn failed. Kind={Kind}, ModelId={ModelId}",
                    context.Kind,
                    context.ModelId);
                throw;
            }

            if (update is ChatTurnCompleted completed)
            {
                _logger?.LogInformation(
                    "Completed streaming chat turn. Kind={Kind}, ModelId={ModelId}, Success={Success}, ToolResultCount={ToolResultCount}",
                    context.Kind,
                    context.ModelId,
                    completed.Response.IsSuccess,
                    completed.ToolResults?.Count ?? 0);
            }
            else if (update is ChatTurnPendingApproval pendingApproval)
            {
                _logger?.LogWarning(
                    "Streaming chat turn pending approval. Kind={Kind}, ModelId={ModelId}, ToolName={ToolName}",
                    context.Kind,
                    context.ModelId,
                    pendingApproval.PendingApprovalRequest.ToolName);
            }
            else if (update is ChatTurnError error)
            {
                _logger?.LogError(
                    "Streaming chat turn emitted error. Kind={Kind}, ModelId={ModelId}, Error={Error}",
                    context.Kind,
                    context.ModelId,
                    error.ErrorMessage);
            }

            yield return update;
        }
    }
}
