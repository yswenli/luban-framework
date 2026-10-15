/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： PluginSidebarEntry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：侧栏条目 - 左侧栏显示的一项
*
*****************************************************************************/
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 侧栏条目：插件贡献的左侧栏入口（对应 <c>plugin.json</c> 的 <c>uiContributions</c> 元素，PRD §3.2）。
/// </summary>
public sealed class PluginSidebarEntry
{
    /// <summary>
    /// 条目标识（同一插件内唯一，也用作面板注册键）。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 显示标题（如「新建项目」）。
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 图标标识（宿主自行解析，如 <c>project</c>）。
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// 面板类型名（对应契约 <see cref="IPluginPanel"/> 实现，宿主按需解析）。
    /// </summary>
    [JsonPropertyName("panelType")]
    public string? PanelType { get; set; }

    /// <summary>
    /// 来源插件标识。由宿主在加载时回填，不来自 <c>plugin.json</c>。
    /// </summary>
    [JsonIgnore]
    public string PluginId { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString() => $"{Label}({Id})";
}