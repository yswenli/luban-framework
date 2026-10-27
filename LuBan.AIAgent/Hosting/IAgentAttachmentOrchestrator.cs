/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentAttachmentOrchestrator
*版本号： V1.0.0.0
*唯一标识：附件编排接口
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：附件编排契约：引用解析 + 处理 + 能力门禁 + 视觉探测
*
*****************************************************************************/using LuBan.AIAgent.Attachments;

namespace LuBan.AIAgent.Hosting;

/// <summary>附件编排：路径解析 + 处理 + 能力门禁 + 视觉能力探测。</summary>
public interface IAgentAttachmentOrchestrator
{
    /// <summary>从输入文本中剥离 @引用，返回（剩余文本, 已识别路径）；默认实现原样返回。</summary>
    (string Text, IReadOnlyList<string> Paths) ExtractReferences(string input);

    /// <summary>处理附件（逐文件，超过上限抛 <see cref="InvalidOperationException"/>）。</summary>
    Task<IReadOnlyList<ProcessedAttachment>> ProcessAsync(
        IReadOnlyList<string> paths, CancellationToken ct = default);

    /// <summary>检查模型能否承载，不能则给出提示；可以则返回 null。</summary>
    string? Validate(string modelId, IReadOnlyList<ProcessedAttachment> attachments);

    /// <summary>创建本轮视觉能力探针；无需探测返回 null。</summary>
    IModelCapabilityProbe? CreateProbe(string modelId, bool hasImage);
}