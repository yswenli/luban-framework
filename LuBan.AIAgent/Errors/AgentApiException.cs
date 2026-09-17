/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Errors
*文件名： AgentApiException
*唯一标识：LLM/API 调用失败异常
*创建时间：2026/9/17
*描述：携带分类结果与友好文案的 LLM/API 调用失败异常
*
*****************************************************************************/
namespace LuBan.AIAgent.Errors;

/// <summary>
///  LLM/API 调用失败异常。宿主直接渲染 <see cref="AgentApiErrorInfo.FriendlyMessage"/> 即可，
/// 原始异常保存在 <see cref="Exception.InnerException"/> 供日志排查。
/// </summary>
public class AgentApiException : Exception
{
    /// <summary>
    /// 分类结果。
    /// </summary>
    public AgentApiErrorInfo Error { get; }

    /// <summary>
    /// 创建异常。
    /// </summary>
    /// <param name="error">分类结果。</param>
    /// <param name="innerException">原始异常（仅用于日志）。</param>
    public AgentApiException(AgentApiErrorInfo error, Exception? innerException = null)
        : base(GetMessage(error), innerException)
    {
        Error = error;
    }

    private static string GetMessage(AgentApiErrorInfo error)
        => (error ?? throw new ArgumentNullException(nameof(error))).FriendlyMessage;
}