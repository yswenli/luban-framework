namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 聊天回合流式更新抽象记录，用于表示聊天回合的实时更新
/// </summary>
public abstract record ChatTurnStreamUpdate;

/// <summary>
/// 文本增量更新，包含新生成的文本片段
/// </summary>
/// <param name="Delta">文本增量</param>
public sealed record ChatTurnTextDelta(string Delta) : ChatTurnStreamUpdate;

/// <summary>
/// 使用信息更新，包含模型使用的令牌信息
/// </summary>
/// <param name="Usage">使用信息</param>
public sealed record ChatTurnUsage(UsageInfo Usage) : ChatTurnStreamUpdate;

/// <summary>
/// 模型响应更新，包含模型的完整响应
/// </summary>
/// <param name="Response">聊天响应</param>
public sealed record ChatTurnModelResponse(ChatResponse Response) : ChatTurnStreamUpdate;

/// <summary>
/// 待批准更新，包含需要用户批准的工具调用
/// </summary>
/// <param name="Response">聊天响应</param>
/// <param name="PendingApprovalRequest">待批准请求</param>
/// <param name="ToolResults">工具执行结果列表</param>
/// <param name="ToolNames">工具名称列表</param>
public sealed record ChatTurnPendingApproval(
    ChatResponse Response,
    ToolApprovalRequest PendingApprovalRequest,
    IReadOnlyList<ToolResult>? ToolResults = null,
    IReadOnlyList<string>? ToolNames = null) : ChatTurnStreamUpdate;

/// <summary>
/// 聊天回合完成更新，包含最终的响应和工具执行结果
/// </summary>
/// <param name="Response">聊天响应</param>
/// <param name="ToolResults">工具执行结果列表</param>
/// <param name="ToolNames">工具名称列表</param>
public sealed record ChatTurnCompleted(
    ChatResponse Response,
    IReadOnlyList<ToolResult>? ToolResults = null,
    IReadOnlyList<string>? ToolNames = null) : ChatTurnStreamUpdate;

/// <summary>
/// 聊天回合错误更新，包含错误信息
/// </summary>
/// <param name="ErrorMessage">错误消息</param>
public sealed record ChatTurnError(string ErrorMessage) : ChatTurnStreamUpdate;
