using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 日志聊天回合中间件，用于记录聊天回合的执行情况
/// </summary>
/// <param name="logger">日志记录器</param>
/// <param name="options">日志中间件选项</param>
public sealed class LoggingChatTurnMiddleware(
    ILogger<LoggingChatTurnMiddleware>? logger = null,
    LoggingMiddlewareOptions? options = null) : IChatTurnMiddleware
{
    private readonly ILogger<LoggingChatTurnMiddleware>? _logger = logger;
    private readonly LoggingMiddlewareOptions _options = options ?? new LoggingMiddlewareOptions();

    /// <summary>
    /// 异步调用中间件
    /// </summary>
    /// <param name="context">聊天回合执行上下文</param>
    /// <param name="next">下一个中间件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    public async Task<ChatTurnResult> InvokeAsync(
        ChatTurnExecutionContext context,
        Func<Task<ChatTurnResult>> next,
        CancellationToken cancellationToken = default)
    {
        LogStart(context);

        try
        {
            var result = await next();
            LogCompletion(context, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Chat turn failed. Kind={Kind}, ModelId={ModelId}",
                context.Kind,
                context.ModelId);
            throw;
        }
    }

    /// <summary>
    /// 记录聊天回合开始
    /// </summary>
    /// <param name="context">聊天回合执行上下文</param>
    private void LogStart(ChatTurnExecutionContext context)
    {
        if (_options.LogInputs)
        {
            _logger?.LogInformation(
                "Starting chat turn. Kind={Kind}, ModelId={ModelId}, Input={Input}, MessageCount={MessageCount}",
                context.Kind,
                context.ModelId,
                context.Input,
                _options.IncludeMessageCounts ? context.Messages.Count : 0);
            return;
        }

        _logger?.LogInformation(
            "Starting chat turn. Kind={Kind}, ModelId={ModelId}, InputLength={InputLength}, MessageCount={MessageCount}",
            context.Kind,
            context.ModelId,
            context.Input?.Length ?? 0,
            _options.IncludeMessageCounts ? context.Messages.Count : 0);
    }

    /// <summary>
    /// 记录聊天回合完成
    /// </summary>
    /// <param name="context">聊天回合执行上下文</param>
    /// <param name="result">聊天回合结果</param>
    private void LogCompletion(ChatTurnExecutionContext context, ChatTurnResult result)
    {
        _logger?.LogInformation(
            "Completed chat turn. Kind={Kind}, ModelId={ModelId}, Success={Success}, PendingApproval={PendingApproval}, ToolResultCount={ToolResultCount}",
            context.Kind,
            context.ModelId,
            result.Response.IsSuccess,
            result.PendingApprovalRequest != null,
            result.ToolResults?.Count ?? 0);
    }
}
