namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具定义记录，包含工具的名称、描述、参数模式和审批模式
/// </summary>
public record ToolDefinition
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// 工具描述
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// 参数模式，用于描述工具参数的结构
    /// </summary>
    public object? ParametersSchema { get; init; }
    
    /// <summary>
    /// 工具审批模式
    /// </summary>
    public ToolApprovalMode ApprovalMode { get; init; } = ToolApprovalMode.None;
}
