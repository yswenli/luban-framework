/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments
*文件名： AttachmentInfo
*版本号： V1.0.0.0
*唯一标识：4a02b519-a859-4fe8-ba92-62c1dfe44f38
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/18
*描述：附件元数据（纯数据，UI 与 framework 共用）。
*
*=================================================
*修改标记
*修改时间：2026/9/18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件元数据（纯数据，UI 与 framework 共用）。
*
*****************************************************************************/

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