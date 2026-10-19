/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： PluginAssemblySharing
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：插件 ALC 共享程序集集合（宿主可配置）—— 决定哪些程序集回落默认 ALC
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 插件 ALC 的共享程序集策略：命中前缀 / 精确名的程序集一律回落默认 ALC。
/// </summary>
/// <remarks>
/// <para>
/// 为什么必须共享：同一程序集若在插件 ALC 与默认 ALC 各加载一份，会产生两个 <c>Type</c> 实例，
/// 导致 <c>IPlugin</c> / 契约接口转换失败（PRD §3.4 方案 B）。故契约程序集（<c>LuBan.*</c>）与
/// 运行期基础库（<c>Microsoft.*</c> / <c>System.*</c>）一律共享。
/// </para>
/// <para>
/// <b>为什么可配置</b>：厂商连接器与宿主程序集（如 <c>LubanAgent.*</c>）同样必须共享；它们由产品或
/// 宿主引入，框架无法预知。宿主经
/// <c>LuBanAgent:Plugins:AssemblySharing:AdditionalPrefixes</c> /
/// <c>AdditionalExactNames</c> <b>追加</b>即可。
/// </para>
/// <para>
/// <b>兼容约定</b>：<see cref="Default"/> 的取值与框架历史硬编码集合<b>完全一致</b>，
/// 未配置时行为不变。
/// </para>
/// </remarks>
public sealed class PluginAssemblySharing
{
    /// <summary>
    /// 必备共享前缀（命中即共享）。<b>不可被配置覆盖</b>——契约与运行期基础库必须共享。
    /// </summary>
    public List<string> Prefixes { get; } = new()
    {
        "LuBan.",
        "Microsoft.",
        "System.",
        "netstandard",
        "mscorlib",
        "Avalonia",
        "WindowsBase",
        "System.Runtime"
    };

    /// <summary>
    /// 必备共享精确名（无前缀约定、或需显式列出的程序集）。<b>不可被配置覆盖</b>。
    /// </summary>
    public List<string> ExactNames { get; } = new()
    {
        "LuBan.AIAgent",
        "LuBan.Common",
        "LuBan.Logging",
        "LuBan.DI",
        "LuBan.Orm",
        "LuBan.Threading",
        "Avalonia",
        "Telegram.Bot",
        "CommunityToolkit.Mvvm"
    };

    /// <summary>
    /// 宿主追加的共享前缀（配置项 <c>LuBanAgent:Plugins:AssemblySharing:AdditionalPrefixes</c>）。
    /// </summary>
    /// <remarks>
    /// 厂商连接器与宿主程序集（如 <c>LubanAgent.</c>）必须共享，由宿主在此追加。
    /// </remarks>
    public List<string> AdditionalPrefixes { get; set; } = new();

    /// <summary>
    /// 宿主追加的共享精确名（配置项 <c>LuBanAgent:Plugins:AssemblySharing:AdditionalExactNames</c>）。
    /// </summary>
    public List<string> AdditionalExactNames { get; set; } = new();

    /// <summary>
    /// 默认策略（与框架历史硬编码集合一致，无追加项）。
    /// </summary>
    public static PluginAssemblySharing Default => new();

    /// <summary>
    /// 判断程序集是否必须共享（回落默认 ALC）。
    /// </summary>
    /// <param name="assemblyName">程序集简单名。</param>
    /// <returns>需要共享时返回 <c>true</c>。</returns>
    public bool IsShared(string assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return false;

        if (ExactNames.Contains(assemblyName, StringComparer.OrdinalIgnoreCase) ||
            AdditionalExactNames.Contains(assemblyName, StringComparer.OrdinalIgnoreCase))
            return true;

        return MatchesPrefix(Prefixes) || MatchesPrefix(AdditionalPrefixes);

        bool MatchesPrefix(List<string> prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (!string.IsNullOrWhiteSpace(prefix) &&
                    assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 去重、去空白（忽略大小写）；必备集合原地规范化，追加集合规范化后回写。
    /// </summary>
    public void Normalize()
    {
        Replace(Prefixes);
        Replace(ExactNames);
        AdditionalPrefixes = Clean(AdditionalPrefixes);
        AdditionalExactNames = Clean(AdditionalExactNames);

        void Replace(List<string> target)
        {
            var cleaned = Clean(target);
            target.Clear();
            target.AddRange(cleaned);
        }

        static List<string> Clean(List<string> source) => source
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}