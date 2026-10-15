/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： SkillDefinition
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：技能定义 - 插件贡献的技能描述（PRD §4.10.1）
*
*****************************************************************************/
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 技能定义：插件贡献的技能描述（PRD §4.10.1）。
/// </summary>
/// <remarks>
/// <c>Id</c> 必须满足框架 <c>SkillManageToolGroup</c> 的校验正则
/// <c>^[a-z0-9][a-z0-9_-]{0,63}$</c>（<b>不含点号</b>，命名用中划线，如 <c>newproject-create</c>）。
/// </remarks>
public sealed class SkillDefinition
{
    /// <summary>
    /// 技能标识（小写字母 / 数字 / 下划线 / 中划线）。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 技能名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 技能说明。
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 插件包内说明文件（SKILL.md）相对路径。
    /// </summary>
    [JsonPropertyName("entry")]
    public string? Entry { get; set; }

    /// <summary>
    /// 技能分类（可选）。
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// 触发关键词（可选，供自然语言触发，替代斜杠命令）。
    /// </summary>
    [JsonPropertyName("triggerKeywords")]
    public List<string>? TriggerKeywords { get; set; }

    /// <summary>
    /// 所需权限；为空时默认继承插件 <c>permissions</c>（PRD §4.10.1）。
    /// </summary>
    [JsonPropertyName("permissions")]
    public PluginPermissions? Permissions { get; set; }
}