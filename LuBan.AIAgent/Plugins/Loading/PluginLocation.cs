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
/// <b>插件只有应用级</b>：目录固定为宿主目录下的 <c>&lt;AppContext.BaseDirectory&gt;/plugins/&lt;plugin-id&gt;/</c>
/// （首版即 <c>luban-agent-codex/plugins/</c>）。用户级（<c>~/.luban-agent/plugins/</c>）覆盖机制本期不实现。
/// <para>
/// 注意区分<b>两个作用域</b>：插件程序集与清单是<b>应用级</b>；而插件贡献的 Skill / MCP
/// 落在<b>工作区级</b>（<c>.luban-agent/skills|mcps</c>），由 <see cref="IPluginHost"/> 的宿主实现
/// 按工作区作用域写入并即时重载（PRD §3.11）。两者不要混淆。
/// </para>
/// <para>结构：<c>plugin.json</c> + <c>lib/</c>（程序集，含 <c>entryAssembly</c> 及其依赖）+ <c>assets/</c>。</para>
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
    /// 插件根目录（应用级，唯一）。
    /// </summary>
    public static string BuiltinRoot =>
        Path.Combine(AppContext.BaseDirectory, "plugins");

    /// <summary>
    /// 返回插件根目录列表（只含真实存在的目录）。
    /// </summary>
    /// <returns>有序目录列表；根目录不存在时为空列表。</returns>
    public static IReadOnlyList<string> ResolveDirectories()
    {
        var root = BuiltinRoot;
        return Directory.Exists(root) ? new[] { root } : Array.Empty<string>();
    }

    /// <summary>
    /// 在给定根目录集合中定位插件目录（返回首个含 <c>plugin.json</c> 的目录）。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="roots">根目录列表；为空时使用 <see cref="ResolveDirectories"/>。</param>
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