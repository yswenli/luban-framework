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
*命名空间：LuBan.AIAgent.Hosting
*文件名： IUserQuestionBroker
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：跨请求的用户提问登记簿接口
*
*****************************************************************************/

namespace LuBan.AIAgent.Hosting;

/// <summary>一次待回答的提问。</summary>
/// <param name="QuestionId">提问标识（推给前端，用于回传）。</param>
/// <param name="SessionId">所属会话。</param>
/// <param name="Questions">待回答的问题列表。</param>
/// <param name="TimeoutSeconds">等待上限（秒）。</param>
public sealed record PendingQuestion(
    string QuestionId,
    string SessionId,
    IReadOnlyList<UserQuestion> Questions,
    int TimeoutSeconds);

/// <summary>
/// 跨请求的用户提问登记簿：SSE 推提问事件，另一个 HTTP 请求回答案。
/// </summary>
/// <remarks>进程内单例；多副本部署需换 Redis 等共享设施（与工具确认 broker 同一约束）。</remarks>
public interface IUserQuestionBroker
{
    /// <summary>等待上限（秒）。</summary>
    int TimeoutSeconds { get; }

    /// <summary>登记一次提问（尚未开始等待）。</summary>
    /// <param name="sessionId">所属会话。</param>
    /// <param name="questions">待回答的问题列表。</param>
    /// <returns>登记项。</returns>
    PendingQuestion Register(string sessionId, IReadOnlyList<UserQuestion> questions);

    /// <summary>等待结果；超时/取消返回「用户取消」，并移除登记项。</summary>
    /// <param name="pending">登记项。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>用户作答或取消结果。</returns>
    Task<UserQuestionResult> WaitAsync(PendingQuestion pending, CancellationToken ct);

    /// <summary>由宿主的回答接口调用；不存在/已过期/会话不符返回 false。</summary>
    /// <param name="sessionId">所属会话。</param>
    /// <param name="questionId">提问标识。</param>
    /// <param name="answers">逐题回答。</param>
    /// <returns>是否成功写入结果。</returns>
    bool TryResolve(string sessionId, string questionId, IReadOnlyList<UserQuestionAnswer> answers);

    /// <summary>用户主动取消提问（关闭弹窗/ESC）；按「已取消」结束等待，不影响节点成败。</summary>
    /// <param name="sessionId">所属会话。</param>
    /// <param name="questionId">提问标识。</param>
    /// <returns>是否成功取消；不存在/已过期/会话不符返回 false。</returns>
    bool TryCancel(string sessionId, string questionId);
}