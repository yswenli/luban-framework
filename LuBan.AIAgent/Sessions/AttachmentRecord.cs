namespace LuBan.AIAgent.Sessions;

/// <summary>历史附件重建所需的最小记录。</summary>
public sealed class AttachmentRecord
{
    /// <summary>文件名。</summary>
    public string FileName { get; set; } = "";

    /// <summary>MIME 类型。</summary>
    public string MediaType { get; set; } = "";

    /// <summary>原始字节数。</summary>
    public long FileSize { get; set; }

    /// <summary>源文件绝对路径。</summary>
    public string SourcePath { get; set; } = "";

    /// <summary>"Image" 或 "TextFile"。</summary>
    public string Kind { get; set; } = "";

    /// <summary>文本附件快照内容（大文本为指引文本）；图片为 null。</summary>
    public string? TextPayload { get; set; }

    /// <summary>图片处理结果（缩放/重编码后）临时文件路径；文本为 null。</summary>
    public string? ProcessedPath { get; set; }

    /// <summary>文本是否为大文件（快照仅为指引）。</summary>
    public bool IsLargeText { get; set; }
}