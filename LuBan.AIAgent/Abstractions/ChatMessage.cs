namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天消息记录，包含消息的角色、内容和工具调用等信息
/// </summary>
public record ChatMessage
{
    /// <summary>
    /// 消息角色
    /// </summary>
    public ChatRole Role { get; init; }
    
    /// <summary>
    /// 文本内容
    /// </summary>
    public string? TextContent { get; init; }
    
    /// <summary>
    /// 内容部分列表
    /// </summary>
    public IReadOnlyList<ContentPart>? ContentParts { get; init; }
    
    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string? ToolCallId { get; init; }
    
    /// <summary>
    /// 工具调用列表
    /// </summary>
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }

    /// <summary>
    /// 从文本创建聊天消息
    /// </summary>
    /// <param name="role">消息角色</param>
    /// <param name="text">文本内容</param>
    /// <returns>聊天消息</returns>
    public static ChatMessage FromText(ChatRole role, string text) => new()
    {
        Role = role,
        TextContent = text
    };

    /// <summary>
    /// 从内容部分创建聊天消息
    /// </summary>
    /// <param name="role">消息角色</param>
    /// <param name="parts">内容部分数组</param>
    /// <returns>聊天消息</returns>
    public static ChatMessage FromParts(ChatRole role, params ContentPart[] parts) => new()
    {
        Role = role,
        ContentParts = parts
    };

    /// <summary>
    /// 创建系统消息
    /// </summary>
    /// <param name="text">消息内容</param>
    /// <returns>系统消息</returns>
    public static ChatMessage System(string text) => FromText(ChatRole.System, text);
    
    /// <summary>
    /// 创建用户消息
    /// </summary>
    /// <param name="text">消息内容</param>
    /// <returns>用户消息</returns>
    public static ChatMessage User(string text) => FromText(ChatRole.User, text);
    
    /// <summary>
    /// 创建用户消息（包含内容部分）
    /// </summary>
    /// <param name="parts">内容部分数组</param>
    /// <returns>用户消息</returns>
    public static ChatMessage User(params ContentPart[] parts) => FromParts(ChatRole.User, parts);
    
    /// <summary>
    /// 创建助手消息
    /// </summary>
    /// <param name="text">消息内容</param>
    /// <returns>助手消息</returns>
    public static ChatMessage Assistant(string text) => FromText(ChatRole.Assistant, text);
}
