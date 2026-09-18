namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 单个文件→附件的处理器。
/// </summary>
public interface IAttachmentProcessor
{
    /// <summary>处理指定文件。</summary>
    Task<ProcessedAttachment> ProcessAsync(string filePath, bool generateThumbnail = false, CancellationToken ct = default);

    /// <summary>该路径是否受支持。</summary>
    bool IsSupported(string filePath);

    /// <summary>根据扩展名解析 MIME。</summary>
    string ResolveMediaType(string filePath);

    /// <summary>支持的扩展名（小写，含点）。</summary>
    IReadOnlyList<string> SupportedExtensions { get; }
}