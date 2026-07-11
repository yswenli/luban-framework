/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ProcessUtil
*版本号： V1.0.0.0
*唯一标识：4b380328-fb11-4ab2-b8cc-be6887f03379
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/3/22 11:11:24
*描述：进程工具类
*
*=====================================================================
*修改标记
*修改时间：2022/3/22 11:11:24
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：进程工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 进程工具类
/// </summary>
public static class ProcessUtil
{
    /// <summary>
    /// 启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="args"></param>
    /// <param name="output"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool Start(string fileName, List<string> args, out string output, out string error)
    {
        output = "";
        error = "";
        if (string.IsNullOrEmpty(fileName)) throw new Exception("文件名不能为空,fileName:" + fileName);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,  // 已安装，可直接调用
            Arguments = string.Join(" ", args),  // 拼接命令参数
            RedirectStandardOutput = true,  // 重定向输出流（便于调试）
            RedirectStandardError = true,   // 重定向错误流（捕获错误）
            UseShellExecute = false,        // 非 Shell 执行（必填，否则无法重定向流）
            CreateNoWindow = true           // 不创建窗口（后台执行）
        };
        using var process = new Process { StartInfo = processStartInfo };
        try
        {
            process.Start();
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("ProcessUtil.Start", ex, fileName, args);
        }
        return false;
    }

    /// <summary>
    /// 关闭进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool Stop(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !FileUtil.Exists(fileName)) throw new Exception("文件地址不能为空或文件不存在,fileName:" + fileName);

        try
        {
            var processes = Process.GetProcesses();

            foreach (var item in processes)
            {
                try
                {
                    if (item.MainModule?.FileName == fileName)
                    {
                        item.Kill();
                    }

                }
                catch { }
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("ProcessUtil.Stop", ex, fileName);
        }
        return false;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="inputStr"></param>
    /// <param name="outputStr"></param>
    /// <returns></returns>
    public static bool Execute(string inputStr, out string outputStr)
    {
        var p = new Process();
        try
        {
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine($"{inputStr}&exit");
            p.StandardInput.AutoFlush = true;
            outputStr = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return true;
        }
        catch (Exception ex)
        {
            outputStr = ex.Message;
        }
        p.Close();
        return false;
    }

    /// <summary>
    /// 打开浏览器
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static void Navigate(string url)
    {
        if (!RuntimeUtil.IsWindows()) return;
        if (url.IsNullOrEmpty())
        {
            return;
        }
        url = url.Replace("*", "localhost").Replace("127.0.0.1", "localhost");
        if (url.IsNullOrEmpty())
        {
            return;
        }
        Process.Start("explorer", url);
    }
}
