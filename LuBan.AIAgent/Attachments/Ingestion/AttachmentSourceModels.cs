/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments.Ingestion
*文件名： AttachmentSourceModels
*版本号： V1.0.0.0
*唯一标识：c1f7a4e8-29b3-4d6c-8e01-7a5b2f9d3c61
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件来源解析的上下文与结果模型。
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件来源解析的上下文与结果模型。
*
*****************************************************************************/

namespace LuBan.AIAgent.Attachments.Ingestion;

/// <summary>附件来源解析上下文。</summary>
/// <param name="WorkspaceRoot">当前用户工作区根（URL 落盘与相对路径第一基准）。</param>
/// <param name="ContentRoot">站点/程序内容根（相对路径第二基准；CLI 传 null）。</param>
/// <param name="SessionId">会话标识（决定落盘子目录）。</param>
public sealed record AttachmentSourceContext(string WorkspaceRoot, string? ContentRoot, string SessionId);

/// <summary>解析成功的附件。</summary>
/// <param name="Source">调用方给的原始来源。</param>
/// <param name="LocalPath">本地绝对路径。</param>
/// <param name="FileName">文件名。</param>
/// <param name="MediaType">MIME 类型。</param>
/// <param name="FileSize">字节数。</param>
public sealed record ResolvedAttachment(
    string Source, string LocalPath, string FileName, string MediaType, long FileSize);

/// <summary>单个来源解析失败。</summary>
/// <param name="Source">调用方给的原始来源。</param>
/// <param name="Reason">中文原因。</param>
public sealed record AttachmentSourceFailure(string Source, string Reason);

/// <summary>整批解析结果。</summary>
/// <param name="Resolved">解析成功的附件。</param>
/// <param name="Failed">解析失败的来源与原因。</param>
public sealed record AttachmentSourceResult(
    IReadOnlyList<ResolvedAttachment> Resolved,
    IReadOnlyList<AttachmentSourceFailure> Failed);