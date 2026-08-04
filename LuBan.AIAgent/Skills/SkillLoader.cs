namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 文件加载器，从 SKILL.md 文件加载 Skill 定义。
/// 扫描顺序：项目级目录 > 用户级目录，同名 Skill 高优先级覆盖低优先级。
/// </summary>
public class SkillLoader
{
    private static readonly string UserSkillsRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LuBan", "AIAgent", "skills");

    /// <summary>
    /// 从指定工作区目录和用户目录加载所有 SKILL.md 文件。
    /// </summary>
    /// <param name="workspaceSkillsDir">工作区级 skills 目录（如 &lt;RootPath&gt;/.luban-agent/skills），可为 null</param>
    /// <returns>加载的 FileSkillConfig 列表（已去重，项目级优先）</returns>
    public static List<FileSkillConfig> LoadAll(string? workspaceSkillsDir)
    {
        var result = new Dictionary<string, FileSkillConfig>(StringComparer.OrdinalIgnoreCase);

        // 1. 用户级（低优先级，先加载）
        if (Directory.Exists(UserSkillsRoot))
        {
            foreach (var cfg in ScanDirectory(UserSkillsRoot))
                result[cfg.Id] = cfg;
        }

        // 2. 项目级（高优先级，后加载覆盖）
        if (!string.IsNullOrEmpty(workspaceSkillsDir) && Directory.Exists(workspaceSkillsDir))
        {
            foreach (var cfg in ScanDirectory(workspaceSkillsDir))
                result[cfg.Id] = cfg;
        }

        return result.Values.ToList();
    }

    /// <summary>
    /// 获取用户级 skills 目录路径
    /// </summary>
    public static string GetUserSkillsRoot() => UserSkillsRoot;

    private static List<FileSkillConfig> ScanDirectory(string rootDir)
    {
        var configs = new List<FileSkillConfig>();

        foreach (var skillDir in Directory.GetDirectories(rootDir))
        {
            var skillFile = Path.Combine(skillDir, "SKILL.md");
            if (!File.Exists(skillFile))
                continue;

            try
            {
                var content = File.ReadAllText(skillFile);
                var config = ParseSkillMd(content, Path.GetFileName(skillDir), skillFile);
                if (config != null)
                    configs.Add(config);
            }
            catch
            {
                // 忽略单个文件解析失败
            }
        }

        return configs;
    }

    /// <summary>
    /// 解析 SKILL.md 文件内容。格式：
    /// ---
    /// name: xxx
    /// description: xxx
    /// category: xxx
    /// ---
    /// # 正文内容（PromptTemplate）
    /// </summary>
    internal static FileSkillConfig? ParseSkillMd(string content, string fallbackId, string sourcePath)
    {
        var config = new FileSkillConfig
        {
            Id = fallbackId.ToLowerInvariant(),
            SourcePath = sourcePath
        };

        var trimmed = content.TrimStart();

        // 解析 YAML frontmatter
        if (trimmed.StartsWith("---"))
        {
            var endIndex = trimmed.IndexOf("---", 3, StringComparison.Ordinal);
            if (endIndex > 0)
            {
                var frontmatter = trimmed.Substring(3, endIndex - 3).Trim();
                var body = trimmed.Substring(endIndex + 3).Trim();

                foreach (var line in frontmatter.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex <= 0) continue;

                    var key = line.Substring(0, colonIndex).Trim().ToLowerInvariant();
                    var value = line.Substring(colonIndex + 1).Trim().Trim('"', '\'');

                    switch (key)
                    {
                        case "name":
                            config.Name = value;
                            break;
                        case "description":
                            config.Description = value;
                            break;
                        case "category":
                            config.Category = value;
                            break;
                    }
                }

                config.PromptTemplate = body;
            }
            else
            {
                // 没有闭合的 frontmatter，整段作为正文
                config.PromptTemplate = trimmed;
            }
        }
        else
        {
            config.PromptTemplate = trimmed;
        }

        // 如果 name 为空，使用 id 作为 name
        if (string.IsNullOrEmpty(config.Name))
            config.Name = fallbackId;

        // 如果 description 为空，从 PromptTemplate 第一行提取
        if (string.IsNullOrEmpty(config.Description))
        {
            var firstLine = config.PromptTemplate
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?
                .TrimStart('#', ' ') ?? "";
            config.Description = firstLine.Length > 100 ? firstLine.Substring(0, 100) + "..." : firstLine;
        }

        return config;
    }
}
