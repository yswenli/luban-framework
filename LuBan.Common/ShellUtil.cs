/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common
*文件名： ShellUtil
*版本号： V1.0.0.0
*唯一标识：c4617f44-821f-40fd-99a7-66f67eb6a5be
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 15:53:50
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/5 15:53:50
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using System.Runtime.Versioning;

namespace LuBan.Common;

/// <summary>
/// ShellUtil
/// </summary>
public static class ShellUtil
{
    /// <summary>
    /// 在 linux 上通过 /bin/bash -c 执行命令。
    /// <para>安全警告：command 会原样传入 shell。若包含不可信输入，存在命令注入风险
    /// （如分号、$(...)、反引号等可突破当前引号转义）。仅应传入受信任的命令；
    /// 执行外部程序请改用参数化进程调用（Cmd / ProcessUtil.Start）。</para>
    /// </summary>
    /// <param name="command">要执行的 shell 命令（必须受信任）</param>
    /// <returns>标准输出内容</returns>
    [SupportedOSPlatform("linux")]
    public static string Bash(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return string.Empty;

        var escapedArgs = command.Replace("\"", "\\\"");
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        if (!process.Start())
            return string.Empty;

        // 并行读取 stdout/stderr，避免任一管道被写满导致死锁
        var outTask = process.StandardOutput.ReadToEndAsync();
        var errTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        Task.WaitAll(outTask, errTask);
        process.Dispose();
        return outTask.Result;
    }

    /// <summary>
    /// windows系统命令
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string Cmd(string fileName, string args)
    {
        string output = string.Empty;

        var info = new ProcessStartInfo();
        info.FileName = fileName;
        info.Arguments = args;
        info.RedirectStandardOutput = true;

        using (var process = Process.Start(info))
        {
            if (process == null) return string.Empty;
            output = process.StandardOutput.ReadToEnd();
        }
        return output;
    }


    /// <summary>
    /// 打开浏览器
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static Task OpenAsync(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 原实现返回 Task.FromResult(() => {...})，实际返回的是 Task<Action>，
            // lambda 永不会执行。改为 Task.Run 真正异步打开浏览器。
            return Task.Run(() =>
            {
                try
                {
                    Process.Start("explorer", url);
                }
                catch { }
            });
        }
        return Task.CompletedTask;
    }
}
