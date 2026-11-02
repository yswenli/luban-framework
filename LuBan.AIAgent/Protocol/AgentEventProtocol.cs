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
*公司名称：Walle
*命名空间：LuBan.AIAgent.Protocol
*文件名： AgentEventProtocol
*版本号： V1.0.0.0
*唯一标识：f8a1d3c7-0b52-4e96-9d4a-6c8f2e5b7a09
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：宿主事件到传输帧的映射（Web/CLI/Codex 共用）
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：宿主事件到传输帧的映射（Web/CLI/Codex 共用）
*
*****************************************************************************/

using System.Text.Json;

namespace LuBan.AIAgent.Protocol;

/// <summary>UI 无关的事件帧：事件名 + 载荷对象。</summary>
/// <param name="Name">SSE 事件名，如 <c>text.delta</c>。</param>
/// <param name="Payload">载荷对象（Web 侧按 camelCase 序列化）。</param>
public sealed record AgentEventFrame(string Name, object Payload);

/// <summary>
/// 把宿主产出的事件映射为传输帧。Web / CLI / Codex 共用同一套事件名，避免各自手写 switch。
/// </summary>
public static class AgentEventProtocol
{
    /// <summary>会话首帧事件名（由传输层构造载荷）。</summary>
    public const string Session = "session";

    /// <summary>结束帧事件名（由传输层构造载荷）。</summary>
    public const string Done = "done";

    /// <summary>错误帧事件名（载荷 <c>{ message }</c>）。</summary>
    public const string Error = "error";

    /// <summary>载荷序列化口径：camelCase、不缩进、不转义中文。</summary>
    public static readonly JsonSerializerOptions PayloadJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>把事件映射成帧；不认识的类型返回 null（调用方忽略）。</summary>
    /// <param name="evt">宿主事件。</param>
    /// <returns>传输帧；不支持的事件返回 null。</returns>
    public static AgentEventFrame? ToFrame(AgentStreamEvent evt) => evt switch
    {
        ThinkingDeltaEvent e => new("think.delta", new { delta = e.Delta, llmRound = e.LlmRound }),
        TextDeltaEvent e => new("text.delta", new { delta = e.Delta, llmRound = e.LlmRound }),
        ToolCallStartedEvent e => new("tool.started", new { callId = e.CallId, name = e.Name, arguments = e.Arguments, llmRound = e.LlmRound }),
        ToolCallCompletedEvent e => new("tool.completed", new { callId = e.CallId, resultSummary = e.ResultSummary, llmRound = e.LlmRound }),
        ToolCallFailedEvent e => new("tool.failed", new { callId = e.CallId, error = e.Error, llmRound = e.LlmRound }),
        ToolConfirmationRequestedEvent e => new("tool.confirm", new { confirmationId = e.ConfirmationId, toolName = e.ToolName, arguments = e.Arguments, timeoutSeconds = e.TimeoutSeconds }),
        AttachmentAddedEvent e => new("attachment.added", new { fileName = e.FileName, mediaType = e.MediaType, fileSize = e.FileSize, source = e.Source }),
        AttachmentSkippedEvent e => new("attachment.skipped", new { source = e.Source, reason = e.Reason }),
        ErrorEvent e => new(Error, new { message = e.Message }),
        _ => null
    };

    /// <summary>序列化载荷（供不需要 DI 的宿主直接用）。</summary>
    /// <remarks>
    /// <c>LuBan.AIAgent</c> 开了 <c>IsTrimmable</c> + <c>EnableTrimAnalyzer</c>，
    /// <c>JsonSerializer.Serialize(object, Type, options)</c> 会报 IL2026/IL3050。
    /// 这里的载荷全是本程序集内的 <c>record</c> 与匿名类型，序列化面在编译期已知，故显式抑制。
    /// </remarks>
    /// <param name="payload">载荷对象。</param>
    /// <returns>JSON 文本。</returns>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "Trimming", "IL2026", Justification = "载荷为本程序集内的 record/匿名类型，无动态反射")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "AOT", "IL3050", Justification = "同 IL2026：序列化面编译期已知")]
    public static string SerializePayload(object payload)
        => JsonSerializer.Serialize(payload, payload.GetType(), PayloadJsonOptions);
}