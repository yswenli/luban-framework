/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： XmlExtractor
*版本号： V1.0.0.0
*唯一标识：75ddaed3-c7e4-4359-a911-7c1787f74bfb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：XML / XAML / SVG 提取器（原样返回）
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>XML / XAML / SVG 提取器：原样返回（保留结构供 LLM 阅读）。</summary>
public class XmlExtractor : ISourceExtractor
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".xml", ".xaml", ".svg" };

    /// <inheritdoc />
    public async Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var text = await File.ReadAllTextAsync(filePath, cancellationToken);
        var truncated = text.Length > TextExtractor.MaxChars;
        if (truncated) text = text[..TextExtractor.MaxChars];
        return new ExtractedSource { FilePath = filePath, Text = text, Markdown = text, Truncated = truncated };
    }
}
