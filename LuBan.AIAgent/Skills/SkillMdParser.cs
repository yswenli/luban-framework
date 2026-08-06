namespace LuBan.AIAgent.Skills;

public static class SkillMdParser
{
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
