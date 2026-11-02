/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
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
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.1
*描述：超 MaxCount 改为截断 + 跳过项（原实现抛异常会让整轮对话失败）
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

        // 与 AgentHost 同口径：超出上限只跳过，不抛异常（抛异常会让整轮对话失败）
        var maxCount = options.Value.Attachments.MaxCount;
        var overflow = new List<AttachmentSourceFailure>();
        IReadOnlyList<string> effectivePaths = paths;

        if (maxCount > 0 && paths.Count > maxCount)
        {
            var reason = AttachmentEventProjector.MaxCountExceededReason(maxCount);
            overflow.AddRange(paths.Skip(maxCount).Select(path => new AttachmentSourceFailure(path, reason)));
            effectivePaths = paths.Take(maxCount).ToList();
        }

        var processed = new List<ProcessedAttachment>(effectivePaths.Count);
        var failed = new List<AttachmentSourceFailure>(overflow);

        foreach (var path in effectivePaths)
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