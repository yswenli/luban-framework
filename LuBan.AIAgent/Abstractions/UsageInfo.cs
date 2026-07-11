namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 使用信息记录，包含AI模型使用的令牌数量
/// </summary>
public record UsageInfo
{
    /// <summary>
    /// 提示令牌数
    /// </summary>
    public int? PromptTokens { get; init; }
    
    /// <summary>
    /// 完成令牌数
    /// </summary>
    public int? CompletionTokens { get; init; }
    
    /// <summary>
    /// 总令牌数
    /// </summary>
    public int? TotalTokens { get; init; }
}
