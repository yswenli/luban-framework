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
*文件名： AgentRunRequest
*版本号： V1.0.0.0
*唯一标识：7de55616-8076-454b-abb1-9368822428db
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：一轮对话的入参模型（正文/附件/会话/权限/回调）
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 一轮对话的入参（取代宿主散参数重载）。
/// </summary>
public sealed record AgentRunRequest
{
    /// <summary>用户输入文本。</summary>
    public required string Input { get; init; }

    /// <summary>附件文件绝对路径列表。</summary>
    public IReadOnlyList<string> AttachmentPaths { get; init; } = [];

    /// <summary>
    /// 未解析的附件来源（http(s) URL / 本机绝对路径 / 相对路径）。
    /// 由 <c>IAgentAttachmentSourceResolver</c> 解析后并入 <see cref="AttachmentPaths"/>。
    /// </summary>
    public IReadOnlyList<string> Attachments { get; init; } = [];

    /// <summary>本轮人设/规则覆盖；null 用 Profile 默认提示词。</summary>
    public string? SystemPrompt { get; init; }

    /// <summary>false 时本轮不装配任何工具（非流式接口用）。默认 true。</summary>
    public bool EnableTools { get; init; } = true;

    /// <summary>
    /// 本轮推理强度覆盖；null 表示不覆盖，沿用模型自身默认值。
    /// 设为 <see cref="ReasoningEffort.None"/> 可关闭推理模型的思考，用于要求快速出字的场景。
    /// </summary>
    public ReasoningEffort? ReasoningEffort { get; init; }

    /// <summary>目标会话标识；null 表示当前活动会话。</summary>
    public string? SessionId { get; init; }

    /// <summary>工具权限模式。</summary>
    public ToolPermissionMode PermissionMode { get; init; } = ToolPermissionMode.Default;

    /// <summary>
    /// 工具确认处理器；null 时沿用框架既有语义（未设置确认回调的工具调用一律拒绝）。
    /// </summary>
    public Func<string, IReadOnlyDictionary<string, object?>, ConfirmResult>? ConfirmHandler { get; init; }

    /// <summary>异步确认回调；与 <see cref="ConfirmHandler"/> 同时给出时以本字段为准。</summary>
    public Func<string, IReadOnlyDictionary<string, object?>, CancellationToken, Task<ConfirmResult>>? ConfirmHandlerAsync { get; init; }

    /// <summary>Plan 模式下计划项回调。</summary>
    public Action<string, IReadOnlyDictionary<string, object?>>? OnPlannedAction { get; init; }

    /// <summary>用户明确拒绝工具调用时的回调。</summary>
    public Action<string, IReadOnlyDictionary<string, object?>>? OnUserDenied { get; init; }
}