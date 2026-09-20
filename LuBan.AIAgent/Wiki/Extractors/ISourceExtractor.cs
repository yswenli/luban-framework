/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： ISourceExtractor
*版本号： V1.0.0.0
*唯一标识：a1f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f70
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：源文件提取器接口与提取结果
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>提取结果。</summary>
public class ExtractedSource
{
    /// <summary>源文件绝对路径。</summary>
    public string FilePath { get; set; } = "";
    /// <summary>纯文本（供 LLM 阅读）。</summary>
    public string Text { get; set; } = "";
    /// <summary>尽量保留结构的 markdown（无结构时等于 Text）。</summary>
    public string Markdown { get; set; } = "";
    /// <summary>是否被截断。</summary>
    public bool Truncated { get; set; }
}

/// <summary>源文件提取器。</summary>
public interface ISourceExtractor
{
    /// <summary>支持的扩展名（小写，含点，如 ".txt"）。</summary>
    IReadOnlyCollection<string> Extensions { get; }

    /// <summary>提取文件内容。</summary>
    Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default);
}
