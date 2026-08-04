/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： FindSkillsSkill
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-e5f6-7890-abcd-ef1234567006
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/3
*描述：技能发现 Skill - 搜索本地 skill、推荐适用场景、引导安装和创建
*
*=================================================
*修改标记
*修改时间：2026/8/3
*修改人： yswenli
*版本号： V1.0.0.0
*描述：技能发现 Skill - 搜索本地 skill、推荐适用场景、引导安装和创建
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// 技能发现 Skill - 搜索本地 skill、推荐适用场景、引导安装和创建
/// </summary>
public class FindSkillsSkill : SkillBase
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public override string Id => "find-skills";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "技能发现";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "发现和查找可用的 Skill：列出本地 skill、按需求推荐、引导从 skills.sh 安装或创建自定义 skill";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "productivity";

    /// <summary>
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "帮我找一个能做代码审查的 skill",
        "有什么 skill 可以帮我写测试",
        "列出所有可用的 skill"
    };

    /// <summary>
    /// 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "find skill",
        "找 skill",
        "技能",
        "skill",
        "有什么 skill",
        "列出 skill"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        // 从 ServiceProvider 获取 SkillRegistry
        var registry = context.ServiceProvider?.GetService(typeof(SkillRegistry)) as SkillRegistry;

        if (registry == null)
        {
            return SkillResult.Fail("无法获取 SkillRegistry，技能发现功能不可用");
        }

        var allSkills = registry.GetAll();

        // 无输入时：列出所有本地 skill
        if (string.IsNullOrWhiteSpace(input) ||
            input.Trim().Equals("list", StringComparison.OrdinalIgnoreCase) ||
            input.Trim().Equals("ls", StringComparison.OrdinalIgnoreCase))
        {
            return ListLocalSkills(allSkills);
        }

        // 有输入时：AI 分析 + 本地匹配 + 市场引导
        UpdateStatus(context, "正在匹配 Skill...");

        var localMatches = registry.Search(input);
        var localSkillInfo = FormatLocalSkillsForPrompt(allSkills);

        var systemPrompt = $@"你是一个 Skill 推荐专家。用户想找一个能满足需求的 Skill。

## 当前本地已安装的 Skill

{localSkillInfo}

## 你的任务

1. **分析需求**：理解用户想要什么样的能力
2. **本地匹配**：从已安装的 Skill 中找出最匹配的，说明为什么匹配
3. **市场推荐**：如果本地没有完美匹配的，建议用户去 skills.sh 技能市场搜索
4. **创建引导**：如果市场上也找不到合适的，建议用户使用 /skill -a 创建自定义 Skill

## skills.sh 技能市场
- 网站地址：https://www.skills.sh
- 搜索方式：访问 https://www.skills.sh/search?q=关键词
- 安装方式：在项目根目录运行 npx skills add <owner/repo>

请用以下格式输出：

🔍 **需求分析**：
> 用户想要...

📦 **本地匹配**：
（如果有匹配的本地 skill）
- **skill-id**: 名称 — 为什么匹配
（如果没有匹配的）
- 本地暂无完全匹配的 Skill

🌐 **市场推荐**：
如果本地匹配不足，建议访问 skills.sh 搜索：
- 搜索链接：https://www.skills.sh/search?q={Uri.EscapeDataString(input.Trim())}
- 安装方式：npx skills add <owner/repo>

🛠️ **自定义创建**：
如果市场和本地都没有合适的，可以使用以下命令创建自定义 Skill：
- /skill -a — 交互式创建自定义 Skill

请根据用户需求智能推荐，优先推荐本地已有的 Skill。";

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n用户需求：{input}");

        return SkillResult.Ok(result ?? "");
    }

    /// <summary>
    /// 列出所有本地 skill
    /// </summary>
    private static SkillResult ListLocalSkills(IReadOnlyList<ISkill> skills)
    {
        if (skills.Count == 0)
        {
            return SkillResult.Ok("暂无可用 Skill。\n\n使用 /skill -a 创建自定义 Skill，或访问 https://www.skills.sh 查找技能市场。");
        }

        var sb = new StringBuilder();
        sb.AppendLine("📋 已安装的 Skill 列表");
        sb.AppendLine();

        var categories = skills.Select(s => s.Category).Distinct().ToList();
        foreach (var category in categories)
        {
            sb.AppendLine($"[{category}]");
            sb.AppendLine();

            foreach (var skill in skills.Where(s => s.Category == category))
            {
                sb.AppendLine($"  {skill.Id,-20} - {skill.Name}");
                sb.AppendLine($"  {"",-20}   {skill.Description}");

                if (skill.Examples.Any())
                {
                    sb.AppendLine($"  {"",-20}   示例: {skill.Examples.First()}");
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine("💡 使用方式：/skill <skill-id> 调用指定 Skill");
        sb.AppendLine("🌐 更多 Skill：访问 https://www.skills.sh 搜索技能市场");
        sb.AppendLine("🛠️ 创建 Skill：使用 /skill -a 创建自定义 Skill");

        return SkillResult.Ok(sb.ToString());
    }

    /// <summary>
    /// 格式化本地 skill 信息供 AI 提示词使用
    /// </summary>
    private static string FormatLocalSkillsForPrompt(IReadOnlyList<ISkill> skills)
    {
        if (skills.Count == 0)
            return "（暂无已安装的 Skill）";

        var sb = new StringBuilder();
        foreach (var skill in skills)
        {
            sb.AppendLine($"- `{skill.Id}`: {skill.Name} — {skill.Description} [分类: {skill.Category}]");
        }
        return sb.ToString();
    }
}
