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
*文件名： IModelCapabilityGate
*版本号： V1.0.0.0
*唯一标识：9ea78496-59c4-48cc-9496-ce22e91f56f2
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：模型对附件的能力判定与单轮视觉能力探针
*
*****************************************************************************/
using LuBan.AIAgent.Attachments;

namespace LuBan.AIAgent.Hosting;

/// <summary>模型对附件的支持判定（产品按模型能力缓存实现）。</summary>
public interface IModelCapabilityGate
{
    /// <summary>该模型能否承载这些附件。</summary>
    bool CanAttach(string modelId, IReadOnlyList<ProcessedAttachment> attachments);

    /// <summary>不支持时的用户提示；支持时返回空串。</summary>
    string GetUnsupportedMessage(string modelId, IReadOnlyList<ProcessedAttachment> attachments);

    /// <summary>
    /// 创建本轮视觉能力探针：仅当本轮带图片且模型视觉能力未知时需要探测；
    /// 无需探测时返回 null。
    /// </summary>
    IModelCapabilityProbe? CreateProbe(string modelId, bool hasImage);
}

/// <summary>
/// 单轮视觉能力探针：宿主把本轮事件与终态回灌给实现，由实现决定是否回填能力记录。
/// </summary>
public interface IModelCapabilityProbe
{
    /// <summary>本轮是否需要探测。</summary>
    bool IsProbing { get; }

    /// <summary>观察一条错误文案（宿主对每个错误事件调用）。</summary>
    void Observe(string? text);

    /// <summary>
    /// 结束探测并回填能力记录。
    /// </summary>
    /// <param name="error">本轮终止异常；为 null 表示正常完成。</param>
    /// <param name="completed">流是否正常走到结束（未被提前 break/取消）。</param>
    void Complete(Exception? error = null, bool completed = true);
}

/// <summary>默认门禁：一律允许，不探测。</summary>
public sealed class AllowAllModelCapabilityGate : IModelCapabilityGate
{
    /// <inheritdoc />
    public bool CanAttach(string modelId, IReadOnlyList<ProcessedAttachment> attachments) => true;

    /// <inheritdoc />
    public string GetUnsupportedMessage(string modelId, IReadOnlyList<ProcessedAttachment> attachments) => string.Empty;

    /// <inheritdoc />
    public IModelCapabilityProbe? CreateProbe(string modelId, bool hasImage) => null;
}