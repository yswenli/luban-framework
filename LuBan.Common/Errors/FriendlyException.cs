/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：System
*文件名： FriendlyException.cs
*版本号： V1.0.0.0
*唯一标识：667fdb20-83c2-45c8-9d82-89b47af4e29e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：FriendlyException 异常类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FriendlyException 异常类
*
*****************************************************************************/

namespace System;

/// <summary>
/// 业务友好异常。抛出后由全局异常处理中间件自动捕获，
/// 返回对应的错误码、消息和 HTTP 状态码。
/// </summary>
public class FriendlyException : Exception
{
    /// <summary>
    /// 异常参数字典，支持在抛出异常时传递额外的上下文信息。
    /// 由 lambda 表达式自动捕获变量名和运行时值。
    /// </summary>
    public Dictionary<string, string>? Args { get; set; }

    /// <summary>
    /// 使用错误描述符创建异常
    /// </summary>
    /// <param name="error">错误描述符</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public FriendlyException(ErrorDescriptor error, params Expression<Func<object?>>[] captures)
        : base(error.Message)
    {
        Error = error;
        Args = DictionaryUtil.CaptureDictionary(captures);
        HttpStatusCode = error.HttpStatusCode;
    }

    /// <summary>
    /// 使用自定义消息和错误描述符创建异常
    /// </summary>
    /// <param name="customMessage">自定义错误消息</param>
    /// <param name="error">错误描述符（提供错误码和分类）</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public FriendlyException(string customMessage, ErrorDescriptor error, params Expression<Func<object?>>[] captures)
        : base(customMessage)
    {
        Error = error;
        Args = DictionaryUtil.CaptureDictionary(captures);
        HttpStatusCode = error.HttpStatusCode;
    }

    /// <summary>
    /// 使用纯文本消息创建异常（临时/快速抛出场景）
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="category">错误分类，默认 Business（HTTP 422）</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public FriendlyException(string message, ErrorCategory category = ErrorCategory.Business, params Expression<Func<object?>>[] captures)
        : base(message)
    {
        Error = new ErrorDescriptor(0, message, category);
        Args = DictionaryUtil.CaptureDictionary(captures);
        HttpStatusCode = category.ToHttpStatus();
    }

    /// <summary>
    /// 使用消息和内部异常创建
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    /// <param name="category">错误分类，默认 System（HTTP 500）</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public FriendlyException(string message, Exception innerException, ErrorCategory category = ErrorCategory.System, params Expression<Func<object?>>[] captures)
        : base(message, innerException)
    {
        Error = new ErrorDescriptor(0, message, category);
        Args = DictionaryUtil.CaptureDictionary(captures);
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
}
