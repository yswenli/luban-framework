/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments.Ingestion
*文件名： IAgentAttachmentSourceResolver
*版本号： V1.0.0.0
*唯一标识：a8e2c5d1-6f47-4b93-9c08-3e5a1d7f2b64
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件来源解析接口。
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件来源解析接口。
*
*****************************************************************************/

namespace LuBan.AIAgent.Attachments.Ingestion;

/// <summary>把调用方给的附件来源（URL / 本地路径 / 相对路径）解析成本地文件。</summary>
public interface IAgentAttachmentSourceResolver
{
    /// <summary>逐项解析；单项失败只记入 <c>Failed</c>，不影响其它项。</summary>
    /// <param name="sources">调用方给的来源列表。</param>
    /// <param name="context">解析上下文。</param>
    /// <param name="ct">取消令牌。</param>
    Task<AttachmentSourceResult> ResolveAsync(
        IReadOnlyList<string> sources,
        AttachmentSourceContext context,
        CancellationToken ct = default);
}