/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： CustomSkillConfig
*版本号： V1.0.0.0
*唯一标识：19a7607d-97e4-4c62-80c9-cb43d9efc639
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：自定义技能配置
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义技能配置
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 自定义 Skill 配置（提示词模板型）
/// </summary>
public class CustomSkillConfig
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public string Category { get; set; } = "custom";

    /// <summary>
    /// 提示词模板
    /// </summary>
    public string PromptTemplate { get; set; } = "";

    /// <summary>
    /// 示例列表
    /// </summary>
    public List<string> Examples { get; set; } = new();

    /// <summary>
    /// 自动激活触发关键词列表
    /// </summary>
    public List<string> TriggerKeywords { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
