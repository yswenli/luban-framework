/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments
*文件名： ProcessedAttachment
*版本号： V1.0.0.0
*唯一标识：0713e52f-f21c-461a-855d-33b1c34c595a
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/18
*描述：处理后的附件，携带可直接放入 <see cref="ChatMessage"/> 的 <see cref="AIContent"/>。
*
*=================================================
*修改标记
*修改时间：2026/9/18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：处理后的附件，携带可直接放入 <see cref="ChatMessage"/> 的 <see cref="AIContent"/>。
*
*****************************************************************************/

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

    /// <summary>
    /// 调用方给的原始附件来源（URL 或路径）；由来源解析阶段回填，用于事件与响应回传。
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// 回填调用方给的原始来源（URL 或路径）。来源解析完成后由宿主调用。
    /// </summary>
    /// <param name="source">原始来源；null 表示未知。</param>
    /// <returns>带来源的副本。</returns>
    public ProcessedAttachment WithSource(string? source) => new()
    {
        Info = Info,
        Content = Content,
        IsLargeText = IsLargeText,
        ThumbnailPath = ThumbnailPath,
        PixelSize = PixelSize,
        Source = source
    };
}