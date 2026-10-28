/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Errors
*文件名： AgentApiErrorInfo
*版本号： V1.0.0.0
*唯一标识：01a6557a-6a0f-4b1f-9222-a780d6d17340
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/17
*描述：LLM/API 错误分类结果
*
*****************************************************************************/

namespace LuBan.AIAgent.Errors;

/// <summary>
/// LLM/API 错误的分类结果。
/// </summary>
/// <param name="Category">错误类别。</param>
/// <param name="StatusCode">HTTP 状态码（无结构化信息时为 null）。</param>
/// <param name="ErrorCode">服务端错误码（如 insufficient_quota；无则为 null）。</param>
/// <param name="FriendlyMessage">面向用户的友好文案（唯一展示内容，原始异常只进日志）。</param>
/// <param name="IsRetryable">是否属于可自动重试的瞬时错误。</param>
public sealed record AgentApiErrorInfo(
    AgentApiErrorCategory Category,
    int? StatusCode,
    string? ErrorCode,
    string FriendlyMessage,
    bool IsRetryable);