using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 处理后的附件，携带可直接放入 <see cref="ChatMessage"/> 的 <see cref="AIContent"/>。
/// </summary>
public sealed class ProcessedAttachment
{
    /// <summary>附件元数据。</summary>
    public required AttachmentInfo Info { get; init; }

    /// <summary>图片为 DataContent；文本为 TextContent 或大文件指引。</summary>
    public required AIContent Content { get; init; }

    /// <summary>大文本文件标记：内容未内联，需 Agent 用 ReadFileAsync 读取。</summary>
    public bool IsLargeText { get; init; }

    /// <summary>缩略图路径（仅 Codex 生成；CLI 为 null）。</summary>
    public string? ThumbnailPath { get; init; }

    /// <summary>图片像素尺寸（仅图片；格式 "宽x高"），未知为 null。</summary>
    public string? PixelSize { get; init; }
}