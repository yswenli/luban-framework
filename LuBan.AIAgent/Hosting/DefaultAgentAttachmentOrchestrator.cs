/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： DefaultAgentAttachmentOrchestrator
*版本号： V1.0.0.0
*唯一标识：ab9e8f01-133a-4a50-b6f7-e5fc19864be5
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：默认附件编排：引用解析、逐文件容错处理、能力门禁与视觉探测
*
*****************************************************************************/

using LuBan.AIAgent.Attachments;
using LuBan.AIAgent.Attachments.Ingestion;

using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Hosting;

/// <summary>默认附件编排：只做处理与门禁，不解析 @引用（交宿主/产品实现）。</summary>
public sealed class DefaultAgentAttachmentOrchestrator(
    IAttachmentProcessor processor,
    IModelCapabilityGate gate,
    IOptions<LuBanAgentOptions> options) : IAgentAttachmentOrchestrator
{
    /// <inheritdoc />
    public (string Text, IReadOnlyList<string> Paths) ExtractReferences(string input)
        => (input, Array.Empty<string>());

    /// <inheritdoc />
    public async Task<AttachmentProcessingResult> ProcessAsync(
        IReadOnlyList<string> paths, CancellationToken ct = default)
    {
        if (paths.Count == 0)
            return new AttachmentProcessingResult(
                Array.Empty<ProcessedAttachment>(), Array.Empty<AttachmentSourceFailure>());

        var maxCount = options.Value.Attachments.MaxCount;
        if (maxCount > 0 && paths.Count > maxCount)
            throw new InvalidOperationException($"最多附加 {maxCount} 个文件");

        var processed = new List<ProcessedAttachment>(paths.Count);
        var failed = new List<AttachmentSourceFailure>();

        foreach (var path in paths)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var item = await processor.ProcessAsync(path, generateThumbnail: false, ct).ConfigureAwait(false);
                processed.Add(item);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // fail-soft：单个文件坏掉不该让整轮对话失败
                failed.Add(new AttachmentSourceFailure(path, $"处理失败: {ex.Message}"));
            }
        }

        return new AttachmentProcessingResult(processed, failed);
    }

    /// <inheritdoc />
    public string? Validate(string modelId, IReadOnlyList<ProcessedAttachment> attachments)
        => attachments.Count == 0 || gate.CanAttach(modelId, attachments)
            ? null
            : gate.GetUnsupportedMessage(modelId, attachments);

    /// <inheritdoc />
    public IModelCapabilityProbe? CreateProbe(string modelId, bool hasImage)
        => gate.CreateProbe(modelId, hasImage);
}