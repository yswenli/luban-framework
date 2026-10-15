/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Loading
*文件名： PluginLocation
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件目录解析 - 用户级优先、应用内置回退（PRD §3.3）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Loading;

/// <summary>
/// 插件目录解析（PRD §3.3 v0.16 定案）。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>应用内置</b>：<c>&lt;AppContext.BaseDirectory&gt;/plugins/&lt;plugin-id&gt;/</c>；</item>
/// <item><b>用户级覆盖</b>（开发/调试优先）：<c>%LocalAppData%/LuBan/AIAgent/plugins/&lt;plugin-id&gt;/</c>。</item>
/// </list>
/// 解析顺序：用户级优先，未命中回退应用内置。
/// </remarks>
public static class PluginLocation
{
    /// <summary>
    /// 插件清单文件名。
    /// </summary>
    public const string ManifestFileName = "plugin.json";

    /// <summary>
    /// 插件程序集目录名。
    /// </summary>
    public const string LibraryDirectoryName = "lib";

    /// <summary>
    /// 插件资源目录名。
    /// </summary>
    public const string AssetsDirectoryName = "assets";

    /// <summary>
    /// 应用内置插件根目录。
    /// </summary>
    public static string BuiltinRoot =>
        Path.Combine(AppContext.BaseDirectory, "plugins");

    /// <summary>
    /// 用户级插件根目录。
    /// </summary>
    public static string UserRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LuBan",
            "AIAgent",
            "plugins");

    /// <summary>
    /// 返回「用户级 → 应用内置」的有序插件根目录列表（仅含真实存在的目录）。
    /// </summary>
    /// <returns>有序目录列表（用户级在前）。</returns>
    public static IReadOnlyList<string> ResolveDirectories()
    {
        var directories = new List<string>(2);

        var userRoot = UserRoot;
        if (Directory.Exists(userRoot))
            directories.Add(userRoot);

        var builtinRoot = BuiltinRoot;
        if (Directory.Exists(builtinRoot))
            directories.Add(builtinRoot);

        return directories;
    }

    /// <summary>
    /// 在给定根目录集合中定位插件目录（用户级优先，返回首个含 <c>plugin.json</c> 的目录）。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="roots">有序根目录列表；为空时使用 <see cref="ResolveDirectories"/>。</param>
    /// <returns>插件目录绝对路径；未命中返回 <c>null</c>。</returns>
    public static string? ResolvePluginDirectory(string pluginId, IEnumerable<string>? roots = null)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return null;

        foreach (var root in roots ?? ResolveDirectories())
        {
            if (string.IsNullOrWhiteSpace(root))
                continue;

            var candidate = Path.Combine(root, pluginId);
            if (File.Exists(Path.Combine(candidate, ManifestFileName)))
                return candidate;
        }

        return null;
    }

    /// <summary>
    /// 解析插件目录下的清单文件路径。
    /// </summary>
    /// <param name="pluginDirectory">插件目录。</param>
    /// <returns>清单文件绝对路径。</returns>
    public static string GetManifestPath(string pluginDirectory)
        => Path.Combine(pluginDirectory, ManifestFileName);

    /// <summary>
    /// 解析入口程序集绝对路径（优先 <c>lib/</c>，兼容直接置于插件根目录的开发态）。
    /// </summary>
    /// <param name="pluginDirectory">插件目录。</param>
    /// <param name="entryAssembly">清单声明的入口程序集文件名。</param>
    /// <returns>入口程序集绝对路径；两处都不存在时返回 <c>null</c>。</returns>
    public static string? ResolveEntryAssemblyPath(string pluginDirectory, string? entryAssembly)
    {
        if (string.IsNullOrWhiteSpace(pluginDirectory) || string.IsNullOrWhiteSpace(entryAssembly))
            return null;

        var inLib = Path.Combine(pluginDirectory, LibraryDirectoryName, entryAssembly!);
        if (File.Exists(inLib))
            return inLib;

        var inRoot = Path.Combine(pluginDirectory, entryAssembly!);
        return File.Exists(inRoot) ? inRoot : null;
    }
}