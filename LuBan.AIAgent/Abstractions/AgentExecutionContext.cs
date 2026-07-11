namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 代理执行上下文，包含代理执行过程中的相关信息
/// </summary>
public record AgentExecutionContext
{
    /// <summary>
    /// 原始代理请求
    /// </summary>
    public AgentRequest OriginalRequest { get; init; } = null!;

    /// <summary>
    /// 当前代理请求
    /// </summary>
    public AgentRequest Request { get; init; } = null!;

    /// <summary>
    /// 模型ID
    /// </summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>
    /// 会话状态
    /// </summary>
    public ConversationState? SessionState { get; init; }

    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }

    /// <summary>
    /// 上下文项字典，用于存储执行过程中的临时数据
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
}
