namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天选项记录，包含聊天请求的各种配置参数
/// </summary>
public record ChatOptions
{
    /// <summary>
    /// 温度参数，控制生成文本的随机性
    /// </summary>
    public double? Temperature { get; init; }
    
    /// <summary>
    /// TopP参数，控制生成文本的多样性
    /// </summary>
    public double? TopP { get; init; }
    
    /// <summary>
    /// 最大令牌数，控制生成文本的长度
    /// </summary>
    public int? MaxTokens { get; init; }
    
    /// <summary>
    /// 停止序列，用于控制生成文本的结束
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; }
    
    /// <summary>
    /// 工具定义列表
    /// </summary>
    public IReadOnlyList<ToolDefinition>? Tools { get; init; }
    
    /// <summary>
    /// 提供者特定的选项
    /// </summary>
    public object? ProviderOptions { get; init; }
}
