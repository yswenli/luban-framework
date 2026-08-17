namespace System;

using LuBan.Common.Errors;

/// <summary>
/// 业务友好异常。抛出后由全局异常处理中间件自动捕获，
/// 返回对应的错误码、消息和 HTTP 状态码。
/// </summary>
public class FriendlyException : Exception
{
    /// <summary>
    /// 使用错误描述符创建异常，支持消息模板参数
    /// </summary>
    /// <param name="error">错误描述符</param>
    /// <param name="args">消息模板参数（用于 string.Format）</param>
    public FriendlyException(ErrorDescriptor error, params object[] args)
        : base(FormatMessage(error.Message, args))
    {
        Error = error;
        HttpStatusCode = error.HttpStatusCode;
    }

    /// <summary>
    /// 使用自定义消息和错误描述符创建异常
    /// </summary>
    /// <param name="customMessage">自定义错误消息</param>
    /// <param name="error">错误描述符（提供错误码和分类）</param>
    /// <param name="args">保留参数</param>
    public FriendlyException(string customMessage, ErrorDescriptor error, params object[] args)
        : base(customMessage)
    {
        Error = error;
        HttpStatusCode = error.HttpStatusCode;
    }

    /// <summary>
    /// 使用纯文本消息创建异常（临时/快速抛出场景）
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="category">错误分类，默认 Business</param>
    public FriendlyException(string message, ErrorCategory category = ErrorCategory.Business)
        : base(message)
    {
        Error = new ErrorDescriptor(0, message, category);
        HttpStatusCode = category.ToHttpStatus();
    }

    /// <summary>
    /// 使用消息和内部异常创建
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    /// <param name="category">错误分类，默认 System</param>
    public FriendlyException(string message, Exception innerException, ErrorCategory category = ErrorCategory.System)
        : base(message, innerException)
    {
        Error = new ErrorDescriptor(0, message, category);
        HttpStatusCode = category.ToHttpStatus();
    }

    /// <summary>
    /// 错误描述符（包含错误码、分类等信息）
    /// </summary>
    public ErrorDescriptor Error { get; }

    /// <summary>
    /// HTTP 响应状态码，由 Error.Category 自动推导，可通过 SetStatusCode 扩展方法覆盖
    /// </summary>
    public int HttpStatusCode { get; set; }

    private static string FormatMessage(string template, object[] args)
    {
        if (args == null || args.Length == 0) return template;
        try { return string.Format(template, args); }
        catch { return template; }
    }
}
