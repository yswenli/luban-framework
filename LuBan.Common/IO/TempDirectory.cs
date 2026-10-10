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
*描述：统一的临时目录与临时文件路径管理，支持按作用域 Push 覆盖临时目录（如会话级工作区
*      .luban-agent/temp），生成带时间戳的临时文件名，便于追踪与清理。
*
*****************************************************************************/
namespace LuBan.Common.IO;

/// <summary>
/// 临时目录提供者：解析临时目录并生成临时文件路径。
/// </summary>
public interface ITempDirectoryProvider
{
    /// <summary>解析当前应使用的临时目录（并确保目录存在）。</summary>
    string GetTempDir();

    /// <summary>生成带时间戳的临时文件完整路径（不创建文件）。</summary>
    string GetTempFilePath(string? prefix = null, string? extension = null);
}

/// <summary>
/// 固定目录提供者：返回指定目录；目录为空或不可用时回退系统临时目录。
/// </summary>
public sealed class StaticTempDirectoryProvider(string? dir) : ITempDirectoryProvider
{
    /// <inheritdoc/>
    public string GetTempDir()
    {
        if (string.IsNullOrEmpty(dir)) return Path.GetTempPath();
        try
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir!;
        }
        catch
        {
            // 自定义目录不可用（如工作区在只读/网络驱动器），回退到系统临时目录
            return Path.GetTempPath();
        }
    }

    /// <inheritdoc/>
    public string GetTempFilePath(string? prefix = null, string? extension = null)
        => TempDirectory.BuildTempFilePath(GetTempDir(), prefix, extension);
}

/// <summary>
/// 默认提供者：始终使用系统临时目录。
/// </summary>
internal sealed class DefaultTempDirectoryProvider : ITempDirectoryProvider
{
    /// <inheritdoc/>
    public string GetTempDir() => Path.GetTempPath();

    /// <inheritdoc/>
    public string GetTempFilePath(string? prefix = null, string? extension = null)
        => TempDirectory.BuildTempFilePath(GetTempDir(), prefix, extension);
}

/// <summary>
/// 临时目录管理：统一解析临时目录并生成带时间戳的临时文件路径。
/// 通过 <see cref="Push"/> 在容器作用域内覆盖临时目录（如会话级工作区目录），
/// 使用 <see cref="AsyncLocal{T}"/> 承载当前提供者，支持嵌套与恢复。
/// </summary>
public static class TempDirectory
{
    private static readonly AsyncLocal<ITempDirectoryProvider?> _current = new();
    private static readonly ITempDirectoryProvider _default = new DefaultTempDirectoryProvider();

    /// <summary>
    /// 当前作用域的临时目录提供者；未 Push 时使用系统临时目录。
    /// </summary>
    public static ITempDirectoryProvider Current => _current.Value ?? _default;

    /// <summary>
    /// 在当前异步作用域内推入一个临时目录提供者，Dispose 时恢复上一个提供者。
    /// </summary>
    /// <param name="provider">临时目录提供者</param>
    /// <returns>释放后恢复上一提供者的句柄</returns>
    public static IDisposable Push(ITempDirectoryProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var prev = _current.Value;
        _current.Value = provider;
        return new Restore(() => _current.Value = prev);
    }

    /// <summary>
    /// 解析当前应使用的临时目录，并确保目录存在。
    /// </summary>
    /// <returns>临时目录绝对路径</returns>
    public static string GetTempDir() => Current.GetTempDir();

    /// <summary>
    /// 生成带时间戳的临时文件完整路径（不创建文件）。
    /// 文件名格式：{prefix}_{yyyyMMddHHmmssfff}_{8位guid}{extension}
    /// </summary>
    /// <param name="prefix">文件名前缀（如 ragflow、script），为空时使用 temp</param>
    /// <param name="extension">扩展名（含点，如 .tmp、.py），为空时默认 .tmp</param>
    /// <returns>临时文件完整路径</returns>
    public static string GetTempFilePath(string? prefix = null, string? extension = null)
        => Current.GetTempFilePath(prefix, extension);

    /// <summary>
    /// 清理当前临时目录中超过指定时长的文件。
    /// 仅清理自定义目录（非系统临时目录），跳过正在被其他进程占用的文件，避免误删。
    /// </summary>
    /// <param name="maxAge">文件最大保留时长</param>
    public static void Cleanup(TimeSpan maxAge)
    {
        try
        {
            var dir = Current.GetTempDir();
            // 仅清理自定义目录，避免误删系统临时目录中的其他文件
            if (IsSystemTemp(dir) || !Directory.Exists(dir)) return;
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
    /// 生成 {prefix}_{yyyyMMddHHmmssfff}_{8位guid}{ext} 路径（不创建文件）。
    /// </summary>
    internal static string BuildTempFilePath(string dir, string? prefix, string? extension)
    {
        var safePrefix = string.IsNullOrWhiteSpace(prefix) ? "temp" : prefix;
        var ext = string.IsNullOrWhiteSpace(extension) ? ".tmp" : extension;
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var shortId = Guid.NewGuid().ToString("N").Substring(0, 8);
        return Path.Combine(dir, $"{safePrefix}_{stamp}_{shortId}{ext}");
    }

    private static bool IsSystemTemp(string dir)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
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

    private sealed class Restore(Action action) : IDisposable
    {
        private Action? _action = action;
        public void Dispose()
        {
            _action?.Invoke();
            _action = null;
        }
    }
}