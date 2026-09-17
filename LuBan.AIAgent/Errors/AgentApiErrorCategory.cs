/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Errors
*文件名： AgentApiErrorCategory
*唯一标识：LLM/API 错误类别
*创建时间：2026/9/17
*描述：LLM/API 错误类别
*
*****************************************************************************/
namespace LuBan.AIAgent.Errors;

/// <summary>
/// LLM/API 错误类别。
/// </summary>
public enum AgentApiErrorCategory
{
    /// <summary>未识别错误。</summary>
    Unknown = 0,
    /// <summary>宿主主动取消（Esc）。</summary>
    Canceled,
    /// <summary>认证失败（401，Key 无效/过期）。</summary>
    Authentication,
    /// <summary>余额不足（402）。</summary>
    PaymentRequired,
    /// <summary>权限不足（403）。</summary>
    PermissionDenied,
    /// <summary>模型不存在或已下线（404/410）。</summary>
    ModelNotFound,
    /// <summary>请求参数或格式错误（400 等）。</summary>
    BadRequest,
    /// <summary>上下文超出模型长度上限。</summary>
    ContextLengthExceeded,
    /// <summary>模型不支持请求的能力（工具调用/多模态等）。</summary>
    UnsupportedCapability,
    /// <summary>触发速率限制（429 rate-limit）。</summary>
    RateLimited,
    /// <summary>配额用尽（429 insufficient_quota 等）。</summary>
    QuotaExhausted,
    /// <summary>服务端错误（5xx）。</summary>
    ServerError,
    /// <summary>网络错误（DNS/连接/TLS）。</summary>
    NetworkError,
    /// <summary>请求超时（非用户取消）。</summary>
    Timeout,
    /// <summary>流式响应中途中断（已输出部分内容）。</summary>
    StreamInterrupted,
    /// <summary>内容被风控拦截。</summary>
    ContentFiltered
}