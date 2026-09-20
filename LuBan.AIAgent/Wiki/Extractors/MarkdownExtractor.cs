/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： MarkdownExtractor
*版本号： V1.0.0.0
*唯一标识：c3f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f72
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：Markdown 提取器
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>Markdown 提取器：原样返回。</summary>
public class MarkdownExtractor : ISourceExtractor
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".md", ".markdown" };

    /// <inheritdoc />
    public async Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var text = await File.ReadAllTextAsync(filePath, cancellationToken);
        var truncated = text.Length > TextExtractor.MaxChars;
        if (truncated) text = text[..TextExtractor.MaxChars];
        return new ExtractedSource { FilePath = filePath, Text = text, Markdown = text, Truncated = truncated };
    }
}
