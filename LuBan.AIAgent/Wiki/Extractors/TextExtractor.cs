/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： TextExtractor
*版本号： V1.0.0.0
*唯一标识：b2f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f71
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：纯文本提取器（同时作为兜底）
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>纯文本提取器，同时作为未命中扩展名的兜底。</summary>
public class TextExtractor : ISourceExtractor
{
    /// <summary>单文件最大读取字符数。</summary>
    public const int MaxChars = 200_000;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } =
        new[] { ".txt", ".log", ".ini", ".cfg", ".toml", ".properties" };

    /// <inheritdoc />
    public async Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var text = await File.ReadAllTextAsync(filePath, cancellationToken);
        var truncated = text.Length > MaxChars;
        if (truncated) text = text[..MaxChars];
        return new ExtractedSource { FilePath = filePath, Text = text, Markdown = text, Truncated = truncated };
    }
}
