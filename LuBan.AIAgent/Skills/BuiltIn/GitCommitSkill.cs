/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： GitCommitSkill
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-e5f6-7890-abcd-ef1234567005
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/3
*描述：Git提交 Skill - 根据 diff 生成 Conventional Commits 格式的提交信息
*
*=================================================
*修改标记
*修改时间：2026/8/3
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Git提交 Skill - 根据 diff 生成 Conventional Commits 格式的提交信息
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// Git提交 Skill - 根据 diff 生成 Conventional Commits 格式的提交信息
/// </summary>
public class GitCommitSkill : SkillBase
{
    /// <summary>
    /// Skill ID
    /// </summary>
    public override string Id => "git-commit";

    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "Git提交";

    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "根据 git diff 生成 Conventional Commits 规范的提交信息，自动分析变更类型和影响范围";

    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "productivity";

    /// <summary>
    /// 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "帮我生成提交信息",
        "看看这次改了什么，写个 commit message",
        "git diff 生成提交描述"
    };

    /// <summary>
    /// 执行 Skill
    /// </summary>
    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在分析代码变更...");

        var systemPrompt = @"你是一个 Git 提交信息生成专家。请根据用户提供的 git diff 或变更描述，生成符合 Conventional Commits 规范的提交信息：

## 提交信息格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

## 类型（type）
- feat: 新功能
- fix: 修复 bug
- docs: 文档变更
- style: 代码格式（不影响功能）
- refactor: 重构（既不是新功能也不是修 bug）
- perf: 性能优化
- test: 测试相关
- build: 构建系统或外部依赖变更
- ci: CI 配置变更
- chore: 杂项（不修改 src 或 test）
- revert: 回滚提交

## 规则
1. subject 不超过 50 个字符，使用祈使句（英文）或动宾短语（中文）
2. body 解释为什么这样做，而不是做了什么，每行不超过 72 个字符
3. 如果有破坏性变更，在 footer 标注 BREAKING CHANGE
4. 如果关联 issue，在 footer 标注 Closes #issue

请用以下格式输出：
📝 **变更摘要**：
- 变更文件数：
- 主要变更内容：

💬 **提交信息**：
```
<type>(<scope>): <subject>

<body>
```

📋 **变更详情**：
- ...";

        var result = await CallAgentAsync(context, $"{systemPrompt}\n\n{input}");

        return SkillResult.Ok(result ?? "");
    }
}
