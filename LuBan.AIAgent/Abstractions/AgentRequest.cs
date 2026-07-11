namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 代理请求记录，包含执行代理所需的所有信息
/// </summary>
public record AgentRequest
{
    /// <summary>
    /// 输入文本
    /// </summary>
    public string Input { get; init; } = string.Empty;
    
    /// <summary>
    /// 模型ID
    /// </summary>
    public string? ModelId { get; init; }
    
    /// <summary>
    /// 会话ID
    /// </summary>
    public string? SessionId { get; init; }
    
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage>? History { get; init; }
    
    /// <summary>
    /// 是否启用技能
    /// </summary>
    public bool EnableSkills { get; init; } = true;
    
    /// <summary>
    /// 首选技能
    /// </summary>
    public string? PreferredSkill { get; init; }
    
    /// <summary>
    /// 允许的技能列表
    /// </summary>
    public IReadOnlyList<string>? AllowedSkills { get; init; }
    
    /// <summary>
    /// 元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}
