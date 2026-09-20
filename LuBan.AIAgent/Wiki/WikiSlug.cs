/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： WikiSlug
*版本号： V1.0.0.0
*唯一标识：7b1e5c92-3a48-4d6f-8e02-9c5d4f7a1b63
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 页面 slug 生成
*
*****************************************************************************/
using System.Text;

namespace LuBan.AIAgent.Wiki;

/// <summary>标题 → 文件名 slug。</summary>
public static class WikiSlug
{
    private static readonly char[] Illegal = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    /// <summary>从标题生成 slug：保留中文与常规字符，过滤 Windows 非法字符，空白转 '-'。</summary>
    public static string FromTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "untitled";
        var sb = new StringBuilder();
        foreach (var ch in title.Trim())
        {
            if (Array.IndexOf(Illegal, ch) >= 0 || char.IsControl(ch)) continue;
            if (char.IsWhiteSpace(ch))
            {
                if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
                continue;
            }
            sb.Append(ch);
        }
        var slug = sb.ToString().Trim('-', '.');
        if (slug.Length == 0) slug = "untitled";
        return slug.Length > 80 ? slug[..80] : slug;
    }

    /// <summary>冲突时追加 -2、-3…</summary>
    public static string EnsureUnique(string slug, IReadOnlySet<string> existing)
    {
        if (!existing.Contains(slug)) return slug;
        for (var i = 2; ; i++)
        {
            var candidate = $"{slug}-{i}";
            if (!existing.Contains(candidate)) return candidate;
        }
    }
}