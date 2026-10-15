/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： PluginCommandDescriptor
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：命令描述 - 插件贡献的命令
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 命令描述：插件贡献的命令（PRD §3.4「命令描述」）。
/// </summary>
/// <remarks>
/// PRD §3.8 v0.15 定案：宿主 <c>CommandCatalog</c> 为 <c>sealed</c> + <c>static readonly</c>，
/// 运行时无写入口，故斜杠命令<b>本期不做</b>，本类型仅作为契约占位，
/// 命令语义改由 Skill 的 <c>TriggerKeywords</c> + 自然语言触发承载。
/// </remarks>
public sealed class PluginCommandDescriptor
{
    /// <summary>
    /// 命令名。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 命令说明。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 参数列表。
    /// </summary>
    public List<PluginCommandArgument> Arguments { get; set; } = new();

    /// <summary>
    /// 处理入口（宿主可解析的标识，如插件内方法名）。
    /// </summary>
    public string? Handler { get; set; }
}

/// <summary>
/// 命令参数描述。
/// </summary>
public sealed class PluginCommandArgument
{
    /// <summary>
    /// 参数名。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 参数说明。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否必填。
    /// </summary>
    public bool Required { get; set; }
}