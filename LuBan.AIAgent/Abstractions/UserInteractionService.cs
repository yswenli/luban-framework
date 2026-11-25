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
*文件名： UserInteractionService
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：用户交互服务实现：读取本轮装配的提问回调，与工具确认共用串行门
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 用户交互服务实现。回调由宿主在每轮对话开始时经
/// <c>ToolConfirmationContextExtensions.ConfigureForTurnAsync</c> 装配到 <see cref="ToolConfirmationContext"/>。
/// </summary>
/// <param name="context">会话级确认/交互上下文（DI 注入）。</param>
public sealed class UserInteractionService(ToolConfirmationContext context) : IUserInteractionService
{
    private readonly ToolConfirmationContext _context = context
        ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public bool IsAvailable => _context.AskQuestions is not null;

    /// <inheritdoc />
    public async Task<UserQuestionResult> AskAsync(
        IReadOnlyList<UserQuestion> questions, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(questions);
        if (questions.Count == 0)
            return UserQuestionResult.Unavailable("没有要提问的内容。");

        // 本轮已取消：直接按取消返回，不再弹窗
        if (_context.CancellationToken.IsCancellationRequested)
            return UserQuestionResult.Cancelled();

        var callback = _context.AskQuestions;
        if (callback is null)
            return UserQuestionResult.Unavailable(UnavailableMessage);

        // 与工具确认共用同一把串行门：宿主交互 UI 为单槽，两者同时只会互相覆盖
        try
        {
            await _context.UiGate.WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return UserQuestionResult.Cancelled();
        }

        try
        {
            // 排队期间回合可能已收尾或取消：重新取回调，避免调用过期回调
            var active = _context.AskQuestions;
            if (active is null)
                return UserQuestionResult.Unavailable(UnavailableMessage);
            if (_context.CancellationToken.IsCancellationRequested || ct.IsCancellationRequested)
                return UserQuestionResult.Cancelled();

            return await active(questions, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return UserQuestionResult.Cancelled();
        }
        finally
        {
            _context.UiGate.Release();
        }
    }

    /// <summary>无提问通道时返回给模型的固定说明。</summary>
    private const string UnavailableMessage =
        "当前宿主不支持交互提问（未装配提问通道）。请基于已有信息继续，不要重复调用本工具。";
}