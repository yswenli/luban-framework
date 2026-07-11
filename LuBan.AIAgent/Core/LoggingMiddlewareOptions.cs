namespace LuBan.AIAgent.Core;

/// <summary>
/// 日志中间件选项
/// </summary>
public sealed class LoggingMiddlewareOptions
{
    /// <summary>
    /// 是否记录输入内容，默认为false
    /// </summary>
    public bool LogInputs { get; set; }

    /// <summary>
    /// 是否记录工具参数，默认为false
    /// </summary>
    public bool LogToolArguments { get; set; }

    /// <summary>
    /// 是否记录工具结果，默认为false
    /// </summary>
    public bool LogToolResults { get; set; }

    /// <summary>
    /// 是否包含消息计数，默认为true
    /// </summary>
    public bool IncludeMessageCounts { get; set; } = true;
}
