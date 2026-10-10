/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.Common
*文件名： MarkdownUtil
*创建时间：2026/10/9
*描述：HTML 转 Markdown 工具（基于 ReverseMarkdown）
*****************************************************************************/
using ReverseMarkdown;

namespace LuBan.Common;

/// <summary>
/// HTML 转 Markdown 工具。
/// </summary>
public static class MarkdownUtil
{
    /// <summary>
    /// 把 HTML 文本转换为 Markdown。转换失败时抛出异常，交由调用方决定降级策略。
    /// </summary>
    /// <param name="html">待转换的 HTML 文本。</param>
    /// <param name="baseUrl">保留参数，供将来解析相对链接使用；当前实现忽略。</param>
    /// <returns>转换后的 Markdown 文本；输入为空时返回空字符串。</returns>
    public static string HtmlToMarkdown(this string html, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var converter = new Converter(new Config
        {
            UnknownTags = Config.UnknownTagsOption.Bypass,
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        });
        return converter.Convert(html);
    }
}
