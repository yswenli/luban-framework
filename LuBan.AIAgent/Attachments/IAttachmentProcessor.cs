/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments
*文件名： IAttachmentProcessor
*版本号： V1.0.0.0
*唯一标识：ab9a8a9c-e0a1-4a3c-a14f-7c7faf5d7bee
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/18
*描述：单个文件→附件的处理器。
*
*=================================================
*修改标记
*修改时间：2026/9/18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：单个文件→附件的处理器。
*
*****************************************************************************/

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