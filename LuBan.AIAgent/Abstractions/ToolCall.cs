namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具调用记录，包含工具调用的ID、名称和参数
/// </summary>
public record ToolCall
{
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// 工具参数
    /// </summary>
    public string Arguments { get; init; } = string.Empty;
}
