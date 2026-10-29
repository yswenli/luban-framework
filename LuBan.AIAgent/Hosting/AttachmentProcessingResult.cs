/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Hosting
*文件名： AttachmentProcessingResult
*版本号： V1.0.0.0
*唯一标识：f4a9c2e6-1b58-4d07-8e93-2a6f5c0d7b18
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件处理结果（成功项 + 逐项失败原因）。
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件处理结果（成功项 + 逐项失败原因）。
*
*****************************************************************************/

using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Attachments.Ingestion;

namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 附件处理结果：成功项与逐项失败原因。
/// </summary>
/// <param name="Processed">处理成功的附件。</param>
/// <param name="Failed">失败项（<c>Source</c> 为本地路径，调用方负责映射回原始来源）。</param>
public sealed record AttachmentProcessingResult(
    IReadOnlyList<ProcessedAttachment> Processed,
    IReadOnlyList<AttachmentSourceFailure> Failed);