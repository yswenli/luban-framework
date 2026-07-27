namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// 文件路径安全守卫
/// </summary>
public sealed class PathGuard
{
    private readonly FileSystemToolOptions _options;

    /// <summary>
    /// 创建 PathGuard 实例
    /// </summary>
    /// <param name="options">LuBan Agent 配置选项</param>
    public PathGuard(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.FileSystem;
    }

    /// <summary>
    /// 检查路径是否允许访问
    /// </summary>
    /// <param name="path">要检查的路径</param>
    /// <returns>是否允许</returns>
    public bool IsAllowed(string path)
    {
        if (_options.AllowedRoots == null || _options.AllowedRoots.Count == 0)
            return true;

        try
        {
            var fullPath = Path.GetFullPath(path);
            var normalizedPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return _options.AllowedRoots.Any(root =>
            {
                var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar) ||
                       normalizedPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查路径是否包含路径穿越
    /// </summary>
    /// <param name="path">要检查的路径</param>
    /// <returns>是否安全</returns>
    public static bool IsPathSafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (path.Contains(".."))
            return false;

        try
        {
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
