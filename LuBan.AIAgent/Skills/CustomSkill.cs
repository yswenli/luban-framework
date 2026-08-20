/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： CustomSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：自定义 Skill 适配器，将 CustomSkillConfig 配置包装为 Skill
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Skills;

/// <summary>
/// 自定义 Skill 适配器，将 CustomSkillConfig 包装为 Skill（纯提示词模板）
/// </summary>
public class CustomSkill : SkillBase
{
    private readonly CustomSkillConfig _config;

    /// <summary>
    /// 创建 CustomSkill 实例
    /// </summary>
    /// <param name="config">自定义 Skill 配置</param>
    public CustomSkill(CustomSkillConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public override string Id => _config.Id;
    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => _config.Name ?? "";
    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => _config.Description ?? "";
    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => _config.Category ?? "";
    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => _config.Examples ?? Enumerable.Empty<string>();
    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => _config.TriggerKeywords ?? Enumerable.Empty<string>();
    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public override string PromptTemplate => _config.PromptTemplate ?? "";
}