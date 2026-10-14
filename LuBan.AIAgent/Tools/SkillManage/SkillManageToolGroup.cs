/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.SkillManage
*文件名： SkillManageToolGroup
*描述：Skill 管理工具实现（WriteSkillAsync / DeleteSkillAsync）
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;
using System.Text;
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Tools.SkillManage;

/// <summary>
/// Skill 管理工具实现。
/// </summary>
public class SkillManageToolGroup
{
    private readonly SkillRegistry _registry;
    private readonly SkillManageToolOptions _options;
    private readonly IAgentReloadTrigger? _reloadTrigger;

    // Skill Id 合法性正则：小写字母/数字/下划线/中划线，首字符必须为字母/数字，最长 64
    private static readonly Regex IdPattern = new(@"^[a-z0-9][a-z0-9_-]{0,63}$", RegexOptions.Compiled);

    /// <summary>
    /// 创建 SkillManageToolGroup 实例。
    /// </summary>
    public SkillManageToolGroup(SkillRegistry registry, SkillManageToolOptions options, IAgentReloadTrigger? reloadTrigger = null)
    {
        _registry = registry;
        _options = options;
        _reloadTrigger = reloadTrigger;
    }

    /// <summary>
    /// 创建或覆盖工作区级 Skill。
    /// </summary>
    /// <param name="id">Skill Id（小写字母/数字/下划线/中划线）。</param>
    /// <param name="name">Skill 名称（用于 frontmatter）。</param>
    /// <param name="description">Skill 描述。</param>
    /// <param name="content">SKILL.md 正文内容。</param>
    /// <param name="overwrite">是否覆盖同名 Skill。</param>
    /// <param name="workspaceRoot">工作区根目录（由工具层传入）。</param>
    /// <returns>创建结果。</returns>
    public async Task<ToolResult<string>> WriteSkillAsync(
        string id,
        string name,
        string description,
        string content,
        bool overwrite,
        string workspaceRoot)
    {
        // 校验 Id
        if (string.IsNullOrWhiteSpace(id) || !IdPattern.IsMatch(id))
            return ToolResult.Fail<string>($"Skill Id 不合法，需匹配: {IdPattern}");

        // 内置 Id 不可覆盖
        if (_registry.IsBuiltin(id))
            return ToolResult.Fail<string>($"Skill Id '{id}' 为内置保留，不可覆盖。");

        // 检查同名
        var existing = _registry.Get(id);
        if (existing != null && !overwrite)
            return ToolResult.Fail<string>($"Skill '{id}' 已存在，需设置 overwrite=true 覆盖。");

        // 内容大小限制
        var totalBytes = Encoding.UTF8.GetByteCount(content);
        if (totalBytes > _options.MaxSkillFileBytes)
            return ToolResult.Fail<string>($"Skill 内容超过上限 {_options.MaxSkillFileBytes / 1024}KB。");

        // 清洗 frontmatter（去除换行/冒号）
        name = SanitizeFrontmatterValue(name);
        description = SanitizeFrontmatterValue(description);

        // 构建文件内容
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"name: {name}");
        sb.AppendLine($"description: {description}");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(content.Trim());

        var skillDir = Path.Combine(workspaceRoot, ".luban-agent", "skills", id);
        var skillFile = Path.Combine(skillDir, "SKILL.md");

        try
        {
            Directory.CreateDirectory(skillDir);
            await File.WriteAllTextAsync(skillFile, sb.ToString());

            // 即时重新加载
            _registry.LoadFromWorkspace(workspaceRoot);

            // 标记延迟重载
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(skillFile, $"Skill '{id}' 已写入 {skillFile}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"写入 Skill 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除工作区级 Skill。
    /// </summary>
    /// <param name="id">Skill Id。</param>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <returns>删除结果。</returns>
    public async Task<ToolResult<string>> DeleteSkillAsync(string id, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ToolResult.Fail<string>("Skill Id 不能为空。");

        // 内置不可删
        if (_registry.IsBuiltin(id))
            return ToolResult.Fail<string>($"Skill '{id}' 为内置，不可删除。");

        var skillDir = Path.Combine(workspaceRoot, ".luban-agent", "skills", id);
        if (!Directory.Exists(skillDir))
            return ToolResult.Fail<string>($"Skill '{id}' 不存在。");

        try
        {
            await Task.Run(() => Directory.Delete(skillDir, recursive: true));

            _registry.LoadFromWorkspace(workspaceRoot);
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(skillDir, $"Skill '{id}' 已删除。");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"删除 Skill 失败: {ex.Message}");
        }
    }

    private static string SanitizeFrontmatterValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        // 移除换行与冒号，避免破坏 YAML 结构
        return value.Replace("\r", " ").Replace("\n", " ").Replace(":", " -");
    }
}