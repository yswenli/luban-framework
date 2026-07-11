namespace LuBan.AIAgent.Extensions.FileSystem;

/// <summary>
/// 文件系统路径守卫，用于解析和验证文件系统路径，防止路径逃逸
/// </summary>
/// <param name="options">文件系统工具选项</param>
public class FileSystemPathGuard(FileSystemToolOptions options)
{
    /// <summary>
    /// 文件系统根路径
    /// </summary>
    public string RootPath { get; } = string.IsNullOrWhiteSpace(options.RootPath)
        ? throw new InvalidOperationException("FileSystemToolOptions.RootPath is required.")
        : Path.GetFullPath(options.RootPath);

    /// <summary>
    /// 解析请求的路径，确保路径在配置的文件系统根目录内
    /// </summary>
    /// <param name="requestedPath">请求的路径</param>
    /// <returns>解析后的完整路径</returns>
    public string ResolvePath(string requestedPath)
    {
        if (string.IsNullOrWhiteSpace(requestedPath))
        {
            throw new InvalidOperationException("A root-relative path is required.");
        }

        var sanitized = requestedPath.Replace('/', Path.DirectorySeparatorChar).Trim();
        var combined = Path.IsPathRooted(sanitized)
            ? Path.GetFullPath(sanitized)
            : Path.GetFullPath(Path.Combine(RootPath, sanitized));

        var rootWithSeparator = RootPath.EndsWith(Path.DirectorySeparatorChar)
            ? RootPath
            : RootPath + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(combined, RootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path escapes the configured filesystem root and is not allowed.");
        }

        return combined;
    }

    /// <summary>
    /// 将完整路径转换为相对于根路径的路径
    /// </summary>
    /// <param name="fullPath">完整路径</param>
    /// <returns>相对路径</returns>
    public string ToRelativePath(string fullPath)
        => Path.GetRelativePath(RootPath, fullPath).Replace(Path.DirectorySeparatorChar, '/');
}
