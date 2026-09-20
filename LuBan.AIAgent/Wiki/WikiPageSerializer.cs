/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： WikiPageSerializer
*版本号： V1.0.0.0
*唯一标识：a1c9f5d8-6e2b-47a0-8c31-5f0b9d4e7a26
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：WikiPage 与 markdown 的互转
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：WikiPage 与 markdown 的互转
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki;

/// <summary>WikiPage 与 markdown 的互转。</summary>
public static class WikiPageSerializer
{
    /// <summary>解析 markdown 为页面。</summary>
    public static WikiPage Parse(string relativePath, string markdown)
    {
        var (fields, body) = WikiFrontmatter.Parse(markdown);
        return new WikiPage
        {
            RelativePath = relativePath,
            Title = fields.GetValueOrDefault("title", Path.GetFileNameWithoutExtension(relativePath)),
            Type = fields.GetValueOrDefault("type", "source"),
            Tags = ParseList(fields.GetValueOrDefault("tags")),
            Sources = ParseList(fields.GetValueOrDefault("sources")),
            Created = ParseDate(fields.GetValueOrDefault("created")),
            Updated = ParseDate(fields.GetValueOrDefault("updated")),
            Body = body
        };
    }

    /// <summary>渲染页面为 markdown。</summary>
    public static string Render(WikiPage page)
    {
        var fields = new Dictionary<string, string>
        {
            ["title"] = page.Title,
            ["type"] = page.Type,
            ["tags"] = "[" + string.Join(", ", page.Tags) + "]",
            ["sources"] = "[" + string.Join(", ", page.Sources) + "]",
            ["created"] = (page.Created ?? DateTime.Today).ToString("yyyy-MM-dd"),
            ["updated"] = (page.Updated ?? DateTime.Today).ToString("yyyy-MM-dd")
        };
        return WikiFrontmatter.Render(fields, page.Body);
    }

    private static List<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        var v = value.Trim();
        if (v.StartsWith('[') && v.EndsWith(']')) v = v[1..^1];
        return v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static DateTime? ParseDate(string? value)
        => DateTime.TryParse(value, out var d) ? d.Date : null;
}