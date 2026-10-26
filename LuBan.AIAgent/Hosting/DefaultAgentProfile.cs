namespace LuBan.AIAgent.Hosting;

/// <summary>产品中立的默认 Profile：通用助手提示词 + 全部工具组。</summary>
public sealed class DefaultAgentProfile : AgentProfileBase
{
    /// <inheritdoc />
    public override string SystemPrompt
        => "你是一个通用 AI 助手。优先使用工具完成任务，回答简洁准确。";

    /// <inheritdoc />
    public override string[]? ToolGroups => null;

    /// <inheritdoc />
    public override string? RetrievalMode => null;
}