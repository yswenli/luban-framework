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
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
