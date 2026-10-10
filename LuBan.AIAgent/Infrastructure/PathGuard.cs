/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Infrastructure
*文件名： PathGuard
*版本号： V1.0.0.0
*唯一标识：f9535f7d-1d98-44e6-acc4-6e41854956a8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：路径安全校验器
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：路径安全校验器
*
*****************************************************************************/
namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// 文件路径安全守卫
/// </summary>
public sealed class PathGuard
{
    private readonly ISessionContext _session;
    private readonly FileSystemToolOptions? _options;

    /// <summary>
    /// 创建 PathGuard 实例
    /// </summary>
    /// <param name="session">会话级上下文（提供允许根，按会话隔离）</param>
    /// <param name="options">LuBan Agent 配置选项（可选，仅非会话场景回退）</param>
    public PathGuard(ISessionContext session, IOptions<LuBanAgentOptions>? options = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _options = options?.Value.Tools.FileSystem;
    }

    /// <summary>
    /// 检查路径是否允许访问
    /// </summary>
    /// <param name="path">要检查的路径</param>
    /// <returns>是否允许</returns>
    public bool IsAllowed(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // 优先使用会话允许根；无会话根时回退全局配置（仅非会话场景）
        IReadOnlyList<string>? roots = _session.AllowedRoots;
        if (roots is not { Count: > 0 })
            roots = _options?.AllowedRoots;
        // 默认拒绝：未配置允许根时禁止任何文件访问，防止越权
        if (roots is not { Count: > 0 })
            return false;

        try
        {
            var fullPath = Path.GetFullPath(path);
            var normalizedPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return roots.Any(root =>
            {
                var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
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
