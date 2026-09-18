namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 附件元数据（纯数据，UI 与 framework 共用）。
/// </summary>
/// <param name="FileName">文件名（不含目录）。</param>
/// <param name="MediaType">MIME 类型。</param>
/// <param name="FileSize">原始文件字节数。</param>
/// <param name="Kind">附件类型。</param>
/// <param name="SourcePath">源文件绝对路径，用于历史重建。</param>
public record AttachmentInfo(
    string FileName,
    string MediaType,
    long FileSize,
    AttachmentKind Kind,
    string SourcePath);