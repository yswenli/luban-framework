using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 进程执行服务，用于执行外部命令并返回结果
/// </summary>
public sealed class ProcessExecutionService
{
    /// <summary>
    /// 异步执行命令
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="workingDirectory">工作目录</param>
    /// <param name="timeoutMs">超时时间（毫秒）</param>
    /// <param name="shell"> shell</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>进程执行结果</returns>
    public async Task<ProcessExecutionResult> ExecuteAsync(
        string command,
        string? workingDirectory,
        int timeoutMs,
        string? shell,
        CancellationToken cancellationToken)
    {
        var (fileName, arguments, resolvedShell) = ResolveShell(shell, command);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? AppContext.BaseDirectory : workingDirectory!
            }
        };

        var startedAt = DateTimeOffset.UtcNow;
        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeoutMs);

        var timedOut = false;
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                    await process.WaitForExitAsync(cancellationToken);
                }
            }
            catch
            {
            }
        }

        var stdout = await stdOutTask;
        var stderr = await stdErrTask;
        var completedAt = DateTimeOffset.UtcNow;
        return new ProcessExecutionResult(
            resolvedShell,
            command,
            timedOut ? -1 : process.ExitCode,
            Normalize(stdout),
            Normalize(stderr),
            (int)(completedAt - startedAt).TotalMilliseconds,
            timedOut);
    }

    /// <summary>
    /// 解析Shell
    /// </summary>
    /// <param name="requestedShell">请求的Shell</param>
    /// <param name="command">命令</param>
    /// <returns>Shell信息元组</returns>
    private static (string FileName, string Arguments, string ResolvedShell) ResolveShell(string? requestedShell, string command)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (string.Equals(requestedShell, "cmd", StringComparison.OrdinalIgnoreCase))
            {
                return ("cmd.exe", $"/c {command}", "cmd");
            }

            return ("pwsh", $"-NoLogo -NoProfile -Command {command}", "pwsh");
        }

        if (string.Equals(requestedShell, "sh", StringComparison.OrdinalIgnoreCase) || !File.Exists("/bin/bash"))
        {
            return ("/bin/sh", $"-lc \"{EscapeForPosix(command)}\"", "sh");
        }

        return ("/bin/bash", $"-lc \"{EscapeForPosix(command)}\"", "bash");
    }

    /// <summary>
    /// 为POSIX系统转义命令
    /// </summary>
    /// <param name="value">命令值</param>
    /// <returns>转义后的命令</returns>
    private static string EscapeForPosix(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>
    /// 规范化输出
    /// </summary>
    /// <param name="value">输出值</param>
    /// <returns>规范化后的输出</returns>
    private static string Normalize(string value) => value.Replace("\r\n", "\n");
}

/// <summary>
/// 进程执行结果
/// </summary>
/// <param name="Shell">使用的Shell</param>
/// <param name="Command">执行的命令</param>
/// <param name="ExitCode">退出代码</param>
/// <param name="StandardOutput">标准输出</param>
/// <param name="StandardError">标准错误</param>
/// <param name="DurationMs">执行时长（毫秒）</param>
/// <param name="TimedOut">是否超时</param>
public sealed record ProcessExecutionResult(
    string Shell,
    string Command,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    int DurationMs,
    bool TimedOut);
