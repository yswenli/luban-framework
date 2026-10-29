/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Hosting
*文件名： AttachmentPreparation
*版本号： V1.0.0.0
*唯一标识：a7f3c9e5-2b64-4d08-9f31-6e5a2c8b0d74
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件前置结果与事件投影
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件前置结果与事件投影
*
*****************************************************************************/

using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Attachments.Ingestion;

namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 附件前置结果：真正进入处理管线的附件 + 需要立刻回传给前端的事件。
/// </summary>
/// <param name="Attached">进入本轮对话的附件（图片 / 内联文本）。</param>
/// <param name="Events">按顺序下发的事件（<c>attachment.added</c> / <c>attachment.skipped</c>）。</param>
public sealed record AttachmentPreparation(
    IReadOnlyList<ProcessedAttachment> Attached,
    IReadOnlyList<AgentStreamEvent> Events)
{
    /// <summary>无附件时的空结果。</summary>
    public static AttachmentPreparation Empty { get; } = new([], []);
}

/// <summary>附件投影结果：留下的附件与被跳过的项。</summary>
/// <param name="Attached">保留的附件。</param>
/// <param name="Skipped">跳过事件（已带原始来源与原因）。</param>
public sealed record AttachmentProjection(
    IReadOnlyList<ProcessedAttachment> Attached,
    IReadOnlyList<AttachmentSkippedEvent> Skipped);

/// <summary>
/// 把「来源解析失败 + 处理阶段失败 + 大文本降级」折算成统一的跳过语义。
/// </summary>
/// <remarks>
/// 单独抽成纯函数是为了能在不打桩整个宿主的前提下覆盖
/// 「非流式禁用工具时大文本必须跳过」这条容易回退的规则。
/// </remarks>
public static class AttachmentEventProjector
{
    /// <summary>非流式（无工具）跳过超大文本时的固定文案（与设计 §5.1 一致）。</summary>
    public const string LargeTextWithoutToolsReason =
        "文件过大（超过 50KB），非流式接口不支持，请改用 chatstream 或分段提问";

    /// <summary>投影。</summary>
    /// <param name="processed">处理成功的附件。</param>
    /// <param name="failures">来源解析/处理阶段的失败项。</param>
    /// <param name="enableTools">本轮是否装配工具（false = 非流式）。</param>
    /// <returns>投影结果。</returns>
    public static AttachmentProjection Project(
        IReadOnlyList<ProcessedAttachment> processed,
        IReadOnlyList<AttachmentSourceFailure> failures,
        bool enableTools)
    {
        var attached = new List<ProcessedAttachment>(processed.Count);
        var skipped = new List<AttachmentSkippedEvent>(failures.Count);

        foreach (var attachment in processed)
        {
            // 大文本附件只是「让模型自己用 ReadFileAsync 读」的指引；没有工具时这条指引无法执行
            if (!enableTools && attachment.IsLargeText)
            {
                skipped.Add(new AttachmentSkippedEvent(
                    attachment.Source ?? attachment.Info.SourcePath, LargeTextWithoutToolsReason));
                continue;
            }

            attached.Add(attachment);
        }

        foreach (var failure in failures)
            skipped.Add(new AttachmentSkippedEvent(failure.Source, failure.Reason));

        return new AttachmentProjection(attached, skipped);
    }
}