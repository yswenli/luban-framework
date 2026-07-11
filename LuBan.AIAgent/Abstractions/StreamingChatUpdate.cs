namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 流式聊天更新抽象记录，用于表示聊天流中的不同类型的更新
/// </summary>
public abstract record StreamingChatUpdate;

/// <summary>
/// 文本增量更新，包含文本的增量内容
/// </summary>
/// <param name="Delta">文本增量</param>
public record TextDeltaUpdate(string Delta) : StreamingChatUpdate;

/// <summary>
/// 工具调用增量更新，包含工具调用的增量内容
/// </summary>
/// <param name="ToolCallId">工具调用ID</param>
/// <param name="NameDelta">工具名称增量</param>
/// <param name="ArgumentsDelta">工具参数增量</param>
public record ToolCallDeltaUpdate(string ToolCallId, string? NameDelta, string? ArgumentsDelta) : StreamingChatUpdate;

/// <summary>
/// 使用信息更新，包含使用信息
/// </summary>
/// <param name="Usage">使用信息</param>
public record UsageUpdate(UsageInfo Usage) : StreamingChatUpdate;

/// <summary>
/// 完成更新，包含完成原因
/// </summary>
/// <param name="FinishReason">完成原因</param>
public record CompletedUpdate(string? FinishReason) : StreamingChatUpdate;

/// <summary>
/// 错误更新，包含错误消息
/// </summary>
/// <param name="ErrorMessage">错误消息</param>
public record ErrorUpdate(string ErrorMessage) : StreamingChatUpdate;
