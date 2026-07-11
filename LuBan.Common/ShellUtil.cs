/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
    /// linux 系统命令
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [SupportedOSPlatform("linux")]
    public static string Bash(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Dispose();
        return result;
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
            return Task.FromResult(() =>
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
