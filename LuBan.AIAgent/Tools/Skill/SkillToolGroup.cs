/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Skill
*文件名： SkillToolGroup
*版本号： V1.0.0.0
*唯一标识：9789632d-f785-4972-a242-30881f85de84
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Skill 消费工具实现（ListSkillsAsync / UseSkillAsync）
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.Skill;

/// <summary>
/// Skill 消费工具实现。
/// </summary>
public class SkillToolGroup
{
    private readonly SkillRegistry _registry;
    private readonly SkillToolOptions _options;
    private readonly IAgentReloadTrigger? _reloadTrigger;

    /// <summary>
    /// 创建 SkillToolGroup 实例。
    /// </summary>
    public SkillToolGroup(SkillRegistry registry, SkillToolOptions options, IAgentReloadTrigger? reloadTrigger = null)
    {
        _registry = registry;
        _options = options;
        _reloadTrigger = reloadTrigger;
    }

    /// <summary>
    /// 列出当前可用的 Skill。
    /// </summary>
    /// <param name="keyword">搜索关键词，为空时列出全部。</param>
    /// <returns>Skill 列表。</returns>
    public ToolResult<List<SkillInfo>> ListSkillsAsync(string? keyword = null)
    {
        var skills = string.IsNullOrWhiteSpace(keyword)
            ? _registry.GetAll()
            : _registry.Search(keyword);

        var result = skills
            .OrderBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
            .Select(s => new SkillInfo
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Category = s.Category,
                IsBuiltin = _registry.IsBuiltin(s.Id)
            })
            .ToList();

        return ToolResult.Ok(result, $"找到 {result.Count} 个 Skill。");
    }

    /// <summary>
    /// 使用指定 Skill，返回该 Skill 的详细指令模板。
    /// </summary>
    /// <param name="id">Skill Id。</param>
    /// <returns>Skill 指令模板。</returns>
    public ToolResult<SkillDetail> UseSkillAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ToolResult.Fail<SkillDetail>("Skill Id 不能为空。");

        var skill = _registry.Get(id);
        if (skill == null)
            return ToolResult.Fail<SkillDetail>($"未找到 Skill: {id}");

        var template = skill.PromptTemplate;
        if (string.IsNullOrEmpty(template))
            return ToolResult.Fail<SkillDetail>($"Skill '{id}' 无可用指令模板。");

        // 替换 {input} 占位符为提示语
        var usableTemplate = template.Replace("{input}", "<用户问题>");

        return ToolResult.Ok(new SkillDetail
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description,
            PromptTemplate = usableTemplate
        }, $"已加载 Skill '{skill.Name}'，请按模板指令执行。");
    }
}

/// <summary>Skill 简要信息。</summary>
public class SkillInfo
{
    /// <summary>Skill Id。</summary>
    public string Id { get; set; } = "";
    /// <summary>Skill 名称。</summary>
    public string Name { get; set; } = "";
    /// <summary>Skill 描述。</summary>
    public string Description { get; set; } = "";
    /// <summary>分类。</summary>
    public string Category { get; set; } = "";
    /// <summary>是否为内置 Skill。</summary>
    public bool IsBuiltin { get; set; }
}

/// <summary>Skill 详细信息（含指令模板）。</summary>
public class SkillDetail
{
    /// <summary>Skill Id。</summary>
    public string Id { get; set; } = "";
    /// <summary>Skill 名称。</summary>
    public string Name { get; set; } = "";
    /// <summary>Skill 描述。</summary>
    public string Description { get; set; } = "";
    /// <summary>提示词模板（已替换占位符）。</summary>
    public string PromptTemplate { get; set; } = "";
}