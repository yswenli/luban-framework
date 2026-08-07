/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： FileSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：文件级 Skill 适配器，将 SKILL.md 文件包装为 Skill
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills;

/// <summary>
/// 文件级 Skill 适配器，将 SKILL.md 文件包装为 Skill（纯提示词模板）
/// </summary>
public class FileSkill : SkillBase
{
    private readonly FileSkillConfig _config;

    /// <summary>
    /// 创建 FileSkill 实例
    /// </summary>
    /// <param name="config">文件级 Skill 配置</param>
    public FileSkill(FileSkillConfig config)
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
    public override string Name => _config.Name;
    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => _config.Description;
    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => _config.Category;
    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => _config.Examples;
    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => _config.TriggerKeywords;
    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public override string PromptTemplate => _config.PromptTemplate;
    /// <summary>
    /// Skill 对应的 SKILL.md 文件路径
    /// </summary>
    public string SourcePath => _config.SourcePath;
}

/// <summary>
/// 文件级 Skill 配置数据，由 SkillMdParser 解析生成
/// </summary>
public class FileSkillConfig
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
    /// Skill 分类，默认 "custom"
    /// </summary>
    public string Category { get; set; } = "custom";
    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public string PromptTemplate { get; set; } = "";
    /// <summary>
    /// Skill 使用示例列表
    /// </summary>
    public List<string> Examples { get; set; } = new();
    /// <summary>
    /// Skill 自动激活触发关键词列表
    /// </summary>
    public List<string> TriggerKeywords { get; set; } = new();
    /// <summary>
    /// Skill 对应的 SKILL.md 文件路径
    /// </summary>
    public string SourcePath { get; set; } = "";
}