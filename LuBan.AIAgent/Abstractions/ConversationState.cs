namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 对话状态记录，包含对话的会话ID、历史记录、活跃技能等信息
/// </summary>
public record ConversationState
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
    
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage> History { get; init; } = [];
    
    /// <summary>
    /// 活跃技能
    /// </summary>
    public string? ActiveSkill { get; init; }
    
    /// <summary>
    /// 元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
