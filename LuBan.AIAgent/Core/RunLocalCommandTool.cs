using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 运行本地命令工具，用于在用户批准后在主机上运行本地shell命令
/// </summary>
/// <param name="processExecutionService">进程执行服务</param>
public sealed class RunLocalCommandTool(ProcessExecutionService processExecutionService) : ITool, IApprovalAwareTool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "run_local_command";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Run a local shell command on the host machine after user approval.";

    /// <summary>
    /// 批准模式
    /// </summary>
    public ToolApprovalMode ApprovalMode => ToolApprovalMode.PerExecution;

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            shell = new { type = "string", description = "Optional shell override: auto, pwsh, cmd, bash, or sh." },
            command = new { type = "string", description = "Exact command string to execute." },
            workingDirectory = new { type = "string", description = "Optional working directory for the command." },
            timeoutMs = new { type = "integer", description = "Optional timeout in milliseconds." }
        },
        required = new[] { "command" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public async Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<RunLocalCommandRequest>(context.ToolCall.Arguments, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Invalid run_local_command arguments.");

        var result = await processExecutionService.ExecuteAsync(
            request.Command,
            request.WorkingDirectory,
            request.TimeoutMs is > 0 ? request.TimeoutMs.Value : 120000,
            request.Shell,
            cancellationToken);

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            IsSuccess = result.ExitCode == 0 && !result.TimedOut,
            Status = result.ExitCode == 0 && !result.TimedOut ? ToolExecutionStatus.Completed : ToolExecutionStatus.Failed,
            Content = JsonSerializer.Serialize(new
            {
                shell = result.Shell,
                command = result.Command,
                exitCode = result.ExitCode,
                stdout = result.StandardOutput,
                stderr = result.StandardError,
                durationMs = result.DurationMs,
                timedOut = result.TimedOut
            }),
            Data = result
        };
    }

    /// <summary>
    /// 运行本地命令请求
    /// </summary>
    /// <param name="Command">命令</param>
    /// <param name="WorkingDirectory">工作目录</param>
    /// <param name="TimeoutMs">超时时间（毫秒）</param>
    /// <param name="Shell">Shell</param>
    private sealed record RunLocalCommandRequest(string Command, string? WorkingDirectory, int? TimeoutMs, string? Shell);
}
