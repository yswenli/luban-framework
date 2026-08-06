/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Infrastructure
*文件名： ProcessRunner
*版本号： V1.0.0.0
*唯一标识：ac994282-d7e3-43e7-bb44-e1d47c4d34e0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：进程运行器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：进程运行器
*
*****************************************************************************/
namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// 进程执行器，用于执行外部命令
/// </summary>
public sealed class ProcessRunner
{
    /// <summary>
    /// 异步执行命令
    /// </summary>
    /// <param name="executable">可执行文件</param>
    /// <param name="arguments">命令行参数</param>
    /// <param name="workingDir">工作目录</param>
    /// <param name="stdin">标准输入</param>
    /// <param name="timeoutMs">超时时间（毫秒）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果</returns>
    public async Task<ProcessResult> RunAsync(
        string executable,
        string arguments,
        string? workingDir = null,
        string? stdin = null,
        int timeoutMs = 120000,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = stdin != null,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDir ?? AppContext.BaseDirectory
            }
        };

        var startedAt = DateTimeOffset.UtcNow;

        // 进程启动失败（如可执行文件不存在）会抛 Win32Exception，需捕获避免终止 agent 循环
        try
        {
            process.Start();
            Logger.Debug($"进程已启动: {executable} {arguments}");
        }
        catch (Win32Exception ex)
        {
            Logger.Error("进程启动失败：可执行文件不存在或无法启动", ex, executable, arguments);
            return new ProcessResult(
                Executable: executable,
                Arguments: arguments,
                ExitCode: -1,
                StandardOutput: "",
                StandardError: $"可执行文件不存在或无法启动: {executable}（{ex.Message}）",
                DurationMs: 0,
                TimedOut: false);
        }
        catch (Exception ex)
        {
            Logger.Error("进程启动失败", ex, executable, arguments);
            return new ProcessResult(
                Executable: executable,
                Arguments: arguments,
                ExitCode: -1,
                StandardOutput: "",
                StandardError: $"进程启动失败: {ex.Message}",
                DurationMs: 0,
                TimedOut: false);
        }

        try
        {
            if (stdin != null)
            {
                await process.StandardInput.WriteAsync(stdin);
                process.StandardInput.Close();
            }

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
                catch { }
            }

            var stdout = await stdOutTask;
            var stderr = await stdErrTask;
            var completedAt = DateTimeOffset.UtcNow;

            return new ProcessResult(
                Executable: executable,
                Arguments: arguments,
                ExitCode: timedOut ? -1 : process.ExitCode,
                StandardOutput: Normalize(stdout),
                StandardError: Normalize(stderr),
                DurationMs: (int)(completedAt - startedAt).TotalMilliseconds,
                TimedOut: timedOut);
        }
        catch (Exception ex)
        {
            Logger.Error("进程执行失败", ex, executable, arguments);
            return new ProcessResult(
                Executable: executable,
                Arguments: arguments,
                ExitCode: -1,
                StandardOutput: "",
                StandardError: $"进程执行失败: {ex.Message}",
                DurationMs: 0,
                TimedOut: false);
        }
    }

    private static string Normalize(string value) => value.Replace("\r\n", "\n");
}

/// <summary>
/// 进程执行结果
/// </summary>
/// <param name="Executable">可执行文件</param>
/// <param name="Arguments">命令行参数</param>
/// <param name="ExitCode">退出代码</param>
/// <param name="StandardOutput">标准输出</param>
/// <param name="StandardError">标准错误</param>
/// <param name="DurationMs">执行时长（毫秒）</param>
/// <param name="TimedOut">是否超时</param>
public sealed record ProcessResult(
    string Executable,
    string Arguments,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    int DurationMs,
    bool TimedOut);
