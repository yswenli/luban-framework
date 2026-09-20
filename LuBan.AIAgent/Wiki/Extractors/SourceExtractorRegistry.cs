/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： SourceExtractorRegistry
*版本号： V1.0.0.0
*唯一标识：d4f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f73
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：按扩展名路由的源文件提取器注册表
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>按扩展名路由到提取器；未命中回落 <see cref="TextExtractor"/>。</summary>
public class SourceExtractorRegistry
{
    private readonly Dictionary<string, ISourceExtractor> _byExtension =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly TextExtractor _fallback = new();

    /// <summary>用给定提取器构建注册表。</summary>
    public SourceExtractorRegistry(IEnumerable<ISourceExtractor> extractors)
    {
        foreach (var extractor in extractors)
            foreach (var ext in extractor.Extensions)
                _byExtension[ext] = extractor;
    }

    /// <summary>构建默认提取器集合。</summary>
    public static SourceExtractorRegistry CreateDefault() => new(new ISourceExtractor[]
    {
        new TextExtractor(),
        new MarkdownExtractor()
    });

    /// <summary>解析扩展名对应的提取器（未命中返回兜底）。</summary>
    public ISourceExtractor Resolve(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return _byExtension.TryGetValue(ext, out var extractor) ? extractor : _fallback;
    }

    /// <summary>是否显式支持该扩展名。</summary>
    public bool Supports(string filePath) => _byExtension.ContainsKey(Path.GetExtension(filePath));

    /// <summary>提取文件。</summary>
    public Task<ExtractedSource> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
        => Resolve(filePath).ExtractAsync(filePath, cancellationToken);
}
