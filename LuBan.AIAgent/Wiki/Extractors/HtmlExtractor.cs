/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： HtmlExtractor
*版本号： V1.0.0.0
*唯一标识：abdb5595-f913-4726-8ecd-3b16d9d02c77
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：HTML 提取器（正则去标签，降级纯文本）
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>
/// HTML 提取器：移除 script/style/标签，降级为纯文本。
/// 不引入 HtmlAgilityPack，不承诺完整 markdown 结构（spec Q16）。
/// </summary>
public class HtmlExtractor : ISourceExtractor
{
    private static readonly Regex ScriptStyle = new(
        @"<(script|style)[^>]*>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex Tag = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex Blank = new(@"\n{3,}", RegexOptions.Compiled);

    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".html", ".htm" };

    /// <inheritdoc />
    public async Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var html = await File.ReadAllTextAsync(filePath, cancellationToken);
        var text = ScriptStyle.Replace(html, " ");
        text = Tag.Replace(text, "\n");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = Blank.Replace(text, "\n\n").Trim();

        var truncated = text.Length > TextExtractor.MaxChars;
        if (truncated) text = text[..TextExtractor.MaxChars];
        return new ExtractedSource { FilePath = filePath, Text = text, Markdown = text, Truncated = truncated };
    }
}
