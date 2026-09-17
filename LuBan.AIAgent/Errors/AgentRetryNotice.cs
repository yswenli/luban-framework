/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Errors
*文件名： AgentRetryNotice
*唯一标识：自动重试通知
*创建时间：2026/9/17
*描述：自动重试通知（由 LuBanAgentOptions.OnApiRetry 投递给宿主）
*
*****************************************************************************/
namespace LuBan.AIAgent.Errors;

/// <summary>
/// 自动重试通知。回调在调用线程执行，宿主需自行编组到 UI 线程。
/// </summary>
/// <param name="Category">触发重试的错误类别。</param>
/// <param name="Attempt">即将进行的重试序号（1 表示第一次重试）。</param>
/// <param name="MaxAttempts">最大尝试次数（含首次调用）。</param>
/// <param name="Delay">本次重试前的等待时长。</param>
public sealed record AgentRetryNotice(
    AgentApiErrorCategory Category,
    int Attempt,
    int MaxAttempts,
    TimeSpan Delay);