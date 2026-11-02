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
*文件名： IAgentAttachmentOrchestrator
*版本号： V1.0.0.0
*唯一标识：ba7e22b6-7cc1-43a3-80e6-ea65901da6b3
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：附件编排契约：引用解析 + 处理 + 能力门禁 + 视觉探测
*
*****************************************************************************/
using LuBan.AIAgent.Attachments;

namespace LuBan.AIAgent.Hosting;

/// <summary>附件编排：路径解析 + 处理 + 能力门禁 + 视觉能力探测。</summary>
public interface IAgentAttachmentOrchestrator
{
    /// <summary>从输入文本中剥离 @引用，返回（剩余文本, 已识别路径）；默认实现原样返回。</summary>
    (string Text, IReadOnlyList<string> Paths) ExtractReferences(string input);

    /// <summary>处理附件（逐文件 fail-soft：单个失败进 <c>Failed</c>，不影响其它项）。</summary>
    /// <param name="paths">本地绝对路径列表。</param>
    /// <param name="ct">取消令牌。</param>
    Task<AttachmentProcessingResult> ProcessAsync(
        IReadOnlyList<string> paths, CancellationToken ct = default);

    /// <summary>检查模型能否承载，不能则给出提示；可以则返回 null。</summary>
    string? Validate(string modelId, IReadOnlyList<ProcessedAttachment> attachments);

    /// <summary>创建本轮视觉能力探针；无需探测返回 null。</summary>
    IModelCapabilityProbe? CreateProbe(string modelId, bool hasImage);
}