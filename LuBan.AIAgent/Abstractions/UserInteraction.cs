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
*命名空间：LuBan.AIAgent.Abstractions
*文件名： UserInteraction
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：Agent 向用户提问的入参/出参模型与交互服务接口
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 一次提问的结果档位。与工具确认（<see cref="ConfirmResult"/>）刻意分开：
/// 提问是获取信息，不涉及授权，取消也不等于拒绝执行。
/// </summary>
public enum UserQuestionOutcome
{
    /// <summary>用户已作答。</summary>
    Answered = 0,

    /// <summary>用户主动取消（关闭/ESC）；不是失败，也不是拒绝。</summary>
    Cancelled = 1,

    /// <summary>当前宿主没有交互通道（插件宿主、broker 关闭、A2A 远端无处理器等）。</summary>
    Unavailable = 2
}

/// <summary>
/// 单个选项。
/// </summary>
/// <param name="Label">选项文案（用户看到的短标签）。</param>
/// <param name="Description">选项说明；宿主可省略展示。</param>
public sealed record UserQuestionOption(string Label, string? Description = null);

/// <summary>
/// 一个待用户回答的问题。
/// </summary>
/// <param name="Question">问题正文。</param>
/// <param name="Options">候选项（2～4 个）。宿主会自动追加「其它」自由输入，故此处不需要包含该项。</param>
/// <param name="Header">短标题（≤12 字），供界面标签页/角标使用。</param>
/// <param name="MultiSelect">是否允许多选。</param>
public sealed record UserQuestion(
    string Question,
    IReadOnlyList<UserQuestionOption> Options,
    string? Header = null,
    bool MultiSelect = false);

/// <summary>
/// 单个问题的回答。
/// </summary>
/// <param name="Question">对应的问题正文（按正文回链，不依赖索引顺序）。</param>
/// <param name="Labels">用户选中的选项文案，必须属于该题 <see cref="UserQuestion.Options"/>；多选时为多项。</param>
/// <param name="FreeText">「其它」自由输入的原样文本；与选项可并存，未填写为 null。</param>
public sealed record UserQuestionAnswer(
    string Question,
    IReadOnlyList<string> Labels,
    string? FreeText = null);

/// <summary>
/// 一次提问的完整结果。
/// </summary>
/// <param name="Outcome">结果档位。</param>
/// <param name="Answers">已作答时的逐题回答；未作答时为 null。</param>
/// <param name="Message">补充说明（取消原因/通道不可用原因）；可为 null。</param>
public sealed record UserQuestionResult(
    UserQuestionOutcome Outcome,
    IReadOnlyList<UserQuestionAnswer>? Answers = null,
    string? Message = null)
{
    /// <summary>构造「用户取消」结果。</summary>
    /// <param name="message">补充说明。</param>
    /// <returns>取消结果。</returns>
    public static UserQuestionResult Cancelled(string? message = null)
        => new(UserQuestionOutcome.Cancelled, null,
            message ?? "用户取消了本次提问，未提供答案。请不要重复追问同一问题。");

    /// <summary>构造「宿主无交互通道」结果。</summary>
    /// <param name="message">通道不可用原因。</param>
    /// <returns>不可用结果。</returns>
    public static UserQuestionResult Unavailable(string message)
        => new(UserQuestionOutcome.Unavailable, null, message);

    /// <summary>构造「已作答」结果。</summary>
    /// <param name="answers">逐题回答。</param>
    /// <returns>作答结果。</returns>
    public static UserQuestionResult Answered(IReadOnlyList<UserQuestionAnswer> answers)
        => new(UserQuestionOutcome.Answered, answers);
}

/// <summary>
/// 用户交互服务：向用户提问并等待回答。
/// </summary>
/// <remarks>
/// 与工具确认共用 <see cref="ToolConfirmationContext.UiGate"/> 串行门：各宿主的交互 UI 都是单槽，
/// 同一会话同一时刻只允许一个确认或一个问题在途，避免后到请求覆盖先到导致前者永久等待。
/// </remarks>
public interface IUserInteractionService
{
    /// <summary>当前回合是否具备提问通道（宿主已装配回调）。</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// 向用户提问并等待回答；无通道时返回 <see cref="UserQuestionOutcome.Unavailable"/>，不抛异常。
    /// </summary>
    /// <param name="questions">问题列表（1～4 个）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>逐题回答或取消/不可用结果。</returns>
    Task<UserQuestionResult> AskAsync(IReadOnlyList<UserQuestion> questions, CancellationToken ct = default);
}