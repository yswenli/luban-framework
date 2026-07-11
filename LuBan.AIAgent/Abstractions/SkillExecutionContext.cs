namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能执行上下文，包含执行技能所需的所有信息
/// </summary>
public record SkillExecutionContext
{
    /// <summary>
    /// 代理请求
    /// </summary>
    public AgentRequest Request { get; init; } = null!;
    
    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }
    
    /// <summary>
    /// 模型ID
    /// </summary>
    public string? ModelId { get; init; }
    
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage>? History { get; init; }
    
    /// <summary>
    /// 技能根目录
    /// </summary>
    public string? SkillRootDirectory { get; init; }
    
    /// <summary>
    /// 技能Markdown文件路径
    /// </summary>
    public string? SkillMarkdownPath { get; init; }
    
    /// <summary>
    /// 上下文项字典
    /// </summary>
    public IDictionary<string, object?> Items { get; init; } = new Dictionary<string, object?>();
}
