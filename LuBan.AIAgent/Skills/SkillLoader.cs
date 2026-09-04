/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillLoader
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：Skill 文件加载器，从 SKILL.md 文件加载 Skill 定义
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 文件加载器，从 SKILL.md 文件加载 Skill 定义。
/// 扫描顺序：项目级目录 > 用户级目录，同名 Skill 高优先级覆盖低优先级。
/// </summary>
public class SkillLoader
{
    // 规范用户级目录：~/.luban-agent/skills（与框架 Playwright 驱动缓存 ~/.luban-agent 约定一致）
    private static readonly string UserSkillsRoot = GlobalLubanAgentPath.SkillsDir;

    // 遗留用户级目录：%LocalAppData%/LuBanFramework/AIAgent/skills（旧版 CLI 写入位置，保留作兼容读取）
    private static readonly string LegacyUserSkillsRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LuBanFramework", "AIAgent", "skills");

    /// <summary>
    /// 从指定工作区目录和用户目录加载所有 SKILL.md 文件。
    /// 加载顺序（后者覆盖前者）：遗留用户级 → 规范用户级（~/.luban-agent/skills）→ 项目级（工作区 .luban-agent/skills）。
    /// </summary>
    /// <param name="workspaceSkillsDir">工作区级 skills 目录（如 &lt;RootPath&gt;/.luban-agent/skills），可为 null</param>
    /// <returns>加载的 FileSkillConfig 列表（已去重，项目级优先）</returns>
    public static List<FileSkillConfig> LoadAll(string? workspaceSkillsDir)
    {
        var result = new Dictionary<string, FileSkillConfig>(StringComparer.OrdinalIgnoreCase);

        // 1. 遗留用户级（最低优先级，先加载）
        if (Directory.Exists(LegacyUserSkillsRoot))
        {
            foreach (var cfg in ScanDirectory(LegacyUserSkillsRoot))
                result[cfg.Id] = cfg;
        }

        // 2. 规范用户级 ~/.luban-agent/skills（覆盖遗留）
        if (Directory.Exists(UserSkillsRoot))
        {
            foreach (var cfg in ScanDirectory(UserSkillsRoot))
                result[cfg.Id] = cfg;
        }

        // 3. 项目级（最高优先级，后加载覆盖）
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
    /// <returns>用户级 skills 目录的绝对路径</returns>
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

    internal static FileSkillConfig? ParseSkillMd(string content, string fallbackId, string sourcePath)
        => SkillMdParser.Parse(content, fallbackId, sourcePath);
}
