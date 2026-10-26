using LuBan.AIAgent.Attachments;

namespace LuBan.AIAgent.Hosting;

/// <summary>默认附件编排：只做处理与门禁，不解析 @引用（交宿主/产品实现）。</summary>
public sealed class DefaultAgentAttachmentOrchestrator(
    IAttachmentProcessor processor,
    IModelCapabilityGate gate) : IAgentAttachmentOrchestrator
{
    /// <summary>每消息最大附件数。</summary>
    public const int MaxAttachmentsPerMessage = 5;

    /// <inheritdoc />
    public (string Text, IReadOnlyList<string> Paths) ExtractReferences(string input)
        => (input, Array.Empty<string>());

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProcessedAttachment>> ProcessAsync(
        IReadOnlyList<string> paths, CancellationToken ct = default)
    {
        if (paths.Count == 0)
            return Array.Empty<ProcessedAttachment>();

        if (paths.Count > MaxAttachmentsPerMessage)
            throw new InvalidOperationException($"最多附加 {MaxAttachmentsPerMessage} 个文件");

        var result = new List<ProcessedAttachment>(paths.Count);
        foreach (var path in paths)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(await processor.ProcessAsync(path, generateThumbnail: false, ct).ConfigureAwait(false));
        }
        return result;
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