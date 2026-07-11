namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具接口，定义了工具的基本属性和执行方法
/// </summary>
public interface ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 工具描述
    /// </summary>
    string? Description { get; }
    
    /// <summary>
    /// 参数 schema，用于描述工具参数的结构
    /// </summary>
    object? ParametersSchema { get; }
    
    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default);
}
