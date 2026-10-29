/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Hosting
*文件名： IToolConfirmationBroker
*版本号： V1.0.0.0
*唯一标识：b6f2d8c0-4a17-4e95-8c3b-1d9f7a2e5c48
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：跨请求的工具确认登记簿接口
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：跨请求的工具确认登记簿接口
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Hosting;

/// <summary>一次待确认的工具调用。</summary>
/// <param name="ConfirmationId">确认标识（推给前端，用于回传）。</param>
/// <param name="SessionId">所属会话。</param>
/// <param name="ToolName">工具方法名。</param>
/// <param name="Arguments">工具参数。</param>
public sealed record PendingConfirmation(
    string ConfirmationId,
    string SessionId,
    string ToolName,
    IReadOnlyDictionary<string, object?> Arguments);

/// <summary>
/// 跨请求的工具确认登记簿：SSE 推确认事件，另一个 HTTP 请求回结果。
/// </summary>
/// <remarks>进程内单例；多副本部署需换 Redis 等共享设施（设计 D19）。</remarks>
public interface IToolConfirmationBroker
{
    /// <summary>确认等待上限（秒）。</summary>
    int TimeoutSeconds { get; }

    /// <summary>登记一次确认请求（尚未开始等待）。</summary>
    /// <param name="sessionId">所属会话。</param>
    /// <param name="toolName">工具方法名。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>登记项。</returns>
    PendingConfirmation Register(
        string sessionId, string toolName, IReadOnlyDictionary<string, object?> arguments);

    /// <summary>等待结果；超时/取消返回 <see cref="ConfirmResult.Deny"/>，并移除登记项。</summary>
    /// <param name="pending">登记项。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>用户选择。</returns>
    Task<ConfirmResult> WaitAsync(PendingConfirmation pending, CancellationToken ct);

    /// <summary>由宿主的确认接口调用；不存在/已过期/会话不符返回 false。</summary>
    /// <param name="sessionId">所属会话。</param>
    /// <param name="confirmationId">确认标识。</param>
    /// <param name="result">用户选择。</param>
    /// <returns>是否成功写入结果。</returns>
    bool TryResolve(string sessionId, string confirmationId, ConfirmResult result);
}