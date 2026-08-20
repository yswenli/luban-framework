/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.IO
*文件名： TempDirectory
*版本号： V1.0.0.0
*唯一标识：临时目录管理
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：统一的临时目录与临时文件路径管理，支持自定义目录解析器（如工作区 .luban-agent/temp），
*      生成带时间戳的临时文件名，便于追踪与清理。
*
*****************************************************************************/
namespace LuBan.Common.IO;

/// <summary>
/// 临时目录管理：统一解析临时目录并生成带时间戳的临时文件路径。
/// 上层应用（如 LubanAgent 工作区）可通过设置 <see cref="Resolver"/> 将临时文件重定向到工作区目录。
/// </summary>
public static class TempDirectory
{
    /// <summary>
    /// 自定义临时目录解析器。返回 null 或空字符串时回退到系统临时目录。
    /// </summary>
    public static Func<string?>? Resolver { get; set; }

    /// <summary>
    /// 解析当前应使用的临时目录，并确保目录存在。
    /// </summary>
    /// <returns>临时目录绝对路径</returns>
    public static string GetTempDir()
    {
        var dir = Resolver?.Invoke();
        if (string.IsNullOrEmpty(dir))
        {
            return Path.GetTempPath();
        }
        try
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir!;
        }
        catch
        {
            // 自定义目录不可用（如工作区在只读/网络驱动器），回退到系统临时目录
            return Path.GetTempPath();
        }
    }

    /// <summary>
    /// 生成带时间戳的临时文件完整路径（不创建文件）。
    /// 文件名格式：{prefix}_{yyyyMMddHHmmssfff}_{8位guid}{extension}
    /// </summary>
    /// <param name="prefix">文件名前缀（如 ragflow、script），为空时使用 temp</param>
    /// <param name="extension">扩展名（含点，如 .tmp、.py），为空时默认 .tmp</param>
    /// <returns>临时文件完整路径</returns>
    public static string GetTempFilePath(string? prefix = null, string? extension = null)
    {
        var dir = GetTempDir();
        var safePrefix = string.IsNullOrWhiteSpace(prefix) ? "temp" : prefix;
        var ext = string.IsNullOrWhiteSpace(extension) ? ".tmp" : extension;
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var shortId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var fileName = $"{safePrefix}_{stamp}_{shortId}{ext}";
        return Path.Combine(dir, fileName);
    }

    /// <summary>
    /// 清理临时目录中超过指定时长的文件。
    /// 仅清理 Resolver 指定的目录（工作区临时目录），不清理系统临时目录。
    /// 跳过正在被其他进程占用的文件，避免误删。
    /// </summary>
    /// <param name="maxAge">文件最大保留时长</param>
    public static void Cleanup(TimeSpan maxAge)
    {
        try
        {
            // 仅清理自定义目录，避免误删系统临时目录中的其他文件
            var dir = Resolver?.Invoke();
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
            var cutoff = DateTime.Now - maxAge;
            foreach (var file in Directory.EnumerateFiles(dir))
            {
                try
                {
                    if (File.GetLastWriteTime(file) >= cutoff) continue;

                    // 并发保护：文件被占用时跳过，下次清理再处理
                    if (IsFileInUse(file)) continue;

                    File.Delete(file);
                }
                catch { }
            }
        }
        catch { }
    }

    /// <summary>
    /// 检测文件是否正在被其他进程占用。
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>被占用返回 true，否则返回 false</returns>
    private static bool IsFileInUse(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }
}
