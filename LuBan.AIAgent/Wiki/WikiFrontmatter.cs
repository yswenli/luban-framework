/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： WikiFrontmatter
*版本号： V1.0.0.0
*唯一标识：7d4b1e02-3a6c-4f58-9b90-2e1f7a6c4d33
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：YAML frontmatter 的最小解析/渲染
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：YAML frontmatter 的最小解析/渲染
*
*****************************************************************************/
using System.Text;

namespace LuBan.AIAgent.Wiki;

/// <summary>YAML frontmatter 的最小解析/渲染（只支持字符串键值与行内数组）。</summary>
public static class WikiFrontmatter
{
    /// <summary>解析 markdown 首部 frontmatter，返回字段与正文。</summary>
    public static (Dictionary<string, string> Fields, string Body) Parse(string markdown)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(markdown)) return (fields, "");
        var normalized = markdown.Replace("\r\n", "\n");
        if (!normalized.StartsWith("---\n")) return (fields, markdown);

        var end = normalized.IndexOf("\n---", 4, StringComparison.Ordinal);
        if (end < 0) return (fields, markdown);

        var header = normalized.Substring(4, end - 4);
        var body = normalized[(end + 4)..].TrimStart('\n');
        foreach (var line in header.Split('\n'))
        {
            var idx = line.IndexOf(':');
            if (idx <= 0) continue;
            fields[line[..idx].Trim()] = Unquote(line[(idx + 1)..].Trim());
        }
        return (fields, body);
    }

    /// <summary>渲染 frontmatter + 正文。</summary>
    public static string Render(IReadOnlyDictionary<string, string> fields, string body)
    {
        var sb = new StringBuilder();
        sb.Append("---\n");
        foreach (var kv in fields) sb.Append(kv.Key).Append(": ").Append(kv.Value).Append('\n');
        sb.Append("---\n\n").Append(body);
        return sb.ToString();
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            return value[1..^1];
        return value;
    }
}