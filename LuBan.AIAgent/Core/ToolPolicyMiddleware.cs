using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 工具策略中间件，用于根据策略控制工具的执行权限
/// </summary>
public sealed class ToolPolicyMiddleware : IToolExecutionMiddleware
{
    private readonly HashSet<string>? _allowedToolNames;
    private readonly HashSet<string> _deniedToolNames;
    private readonly string? _denialMessage;
    private readonly ILogger<ToolPolicyMiddleware>? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">工具策略选项</param>
    /// <param name="logger">日志记录器</param>
    public ToolPolicyMiddleware(
        ToolPolicyOptions? options = null,
        ILogger<ToolPolicyMiddleware>? logger = null)
    {
        _logger = logger;
        _denialMessage = options?.DenialMessage;
        _allowedToolNames = options?.AllowedToolNames == null
            ? null
            : new HashSet<string>(options.AllowedToolNames, StringComparer.OrdinalIgnoreCase);
        _deniedToolNames = options?.DeniedToolNames == null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(options.DeniedToolNames, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 异步调用中间件
    /// </summary>
    /// <param name="context">工具执行中间件上下文</param>
    /// <param name="next">下一个中间件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public Task<ToolExecutionOutcome> InvokeAsync(
        ToolExecutionMiddlewareContext context,
        Func<Task<ToolExecutionOutcome>> next,
        CancellationToken cancellationToken = default)
    {
        if (IsDenied(context.Tool.Name))
        {
            _logger?.LogWarning(
                "Tool execution denied by policy. Tool={ToolName}, ToolCallId={ToolCallId}",
                context.Tool.Name,
                context.ExecutionContext.ToolCall.Id);

            return Task.FromResult(new ToolExecutionOutcome
            {
                Result = new ToolResult
                {
                    ToolCallId = context.ExecutionContext.ToolCall.Id,
                    Content = _denialMessage ?? $"Execution of tool '{context.Tool.Name}' was denied by policy.",
                    IsSuccess = false,
                    Status = ToolExecutionStatus.Denied
                }
            });
        }

        return next();
    }

    /// <summary>
    /// 检查工具是否被拒绝
    /// </summary>
    /// <param name="toolName">工具名称</param>
    /// <returns>是否被拒绝</returns>
    private bool IsDenied(string toolName)
    {
        if (_deniedToolNames.Contains(toolName))
        {
            return true;
        }

        if (_allowedToolNames is { Count: > 0 } && !_allowedToolNames.Contains(toolName))
        {
            return true;
        }

        return false;
    }
}
