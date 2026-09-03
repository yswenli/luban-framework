namespace System;

/// <summary>
/// 友好错误工厂方法。提供多种便捷方式创建 FriendlyException。
/// </summary>
public static class FriendlyError
{
    /// <summary>
    /// 使用错误描述符创建异常
    /// </summary>
    /// <param name="error">错误描述符</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public static FriendlyException Ex(ErrorDescriptor error, params Expression<Func<object?>>[] captures)
        => new(error, captures);

    /// <summary>
    /// 使用自定义消息和错误描述符创建异常
    /// </summary>
    /// <param name="message">自定义错误消息</param>
    /// <param name="error">错误描述符</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public static FriendlyException Ex(string message, ErrorDescriptor error, params Expression<Func<object?>>[] captures)
        => new(message, error, captures);

    /// <summary>
    /// 使用纯文本消息创建异常（临时/快速抛出场景）
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="category">错误分类，默认 Business（HTTP 422）</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public static FriendlyException Ex(string message, ErrorCategory category = ErrorCategory.Business, params Expression<Func<object?>>[] captures)
        => new(message, category, captures);

    /// <summary>
    /// 使用消息和内部异常创建异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="exception">内部异常</param>
    /// <param name="category">错误分类，默认 System（HTTP 500）</param>
    /// <param name="captures">上下文捕获表达式，如 () =&gt; id</param>
    public static FriendlyException Ex(string message, Exception exception, ErrorCategory category = ErrorCategory.System, params Expression<Func<object?>>[] captures)
        => new(message, exception, category, captures);

    /// <summary>
    /// 将任意异常包装为 FriendlyException
    /// </summary>
    /// <param name="exception">源异常</param>
    public static FriendlyException Ex(Exception exception)
        => new(exception.Message, exception, ErrorCategory.System);

    /// <summary>
    /// 覆盖异常的 HTTP 状态码（链式调用）
    /// </summary>
    /// <param name="exception">目标异常</param>
    /// <param name="statusCode">自定义 HTTP 状态码</param>
    /// <returns>同一异常实例（支持链式调用）</returns>
    public static FriendlyException SetStatusCode(this FriendlyException exception, int statusCode)
    {
        exception.HttpStatusCode = statusCode;
        return exception;
    }

    /// <summary>
    /// 为异常附加额外数据（链式调用）
    /// </summary>
    /// <param name="exception">目标异常</param>
    /// <param name="data">附加数据</param>
    /// <returns>同一异常实例（支持链式调用）</returns>
    public static FriendlyException WithData(this FriendlyException exception, params object[] data)
    {
        exception.Data["ErrorData"] = data;
        return exception;
    }
}