namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具执行中间件上下文，包含工具执行中间件所需的所有信息
/// </summary>
public record ToolExecutionMiddlewareContext
{
    /// <summary>
    /// 工具实例
    /// </summary>
    public ITool Tool { get; init; } = null!;
    
    /// <summary>
    /// 工具执行上下文
    /// </summary>
    public ToolExecutionContext ExecutionContext { get; init; } = null!;
    
    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }
    
    /// <summary>
    /// 上下文项字典
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
}
