/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillMdParser
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：SKILL.md 内容解析器，解析 frontmatter 元数据与提示词模板
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills;

/// <summary>
/// SKILL.md 内容解析器，负责解析 frontmatter 元数据（名称、描述、分类、触发关键词）与提示词模板正文
/// </summary>
public static class SkillMdParser
{
    /// <summary>
    /// 解析 SKILL.md 文件内容为 FileSkillConfig
    /// </summary>
    /// <param name="content">SKILL.md 文件内容</param>
    /// <param name="fallbackId">当 frontmatter 未提供名称或无法解析时使用的备用 Id 与名称</param>
    /// <param name="sourcePath">SKILL.md 文件路径</param>
    /// <returns>解析后的 FileSkillConfig</returns>
    public static FileSkillConfig? Parse(string content, string fallbackId, string sourcePath)
    {
        var config = new FileSkillConfig
        {
            Id = fallbackId.ToLowerInvariant(),
            SourcePath = sourcePath
        };

        var trimmed = content.TrimStart();

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
                        case "triggers":
                            config.TriggerKeywords = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim()).ToList();
                            break;
                    }
                }

                config.PromptTemplate = body;
            }
            else
            {
                config.PromptTemplate = trimmed;
            }
        }
        else
        {
            config.PromptTemplate = trimmed;
        }

        if (string.IsNullOrEmpty(config.Name))
            config.Name = fallbackId;

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
