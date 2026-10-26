namespace LuBan.AIAgent.Hosting;

/// <summary>Agent 配置：系统提示词、工具组、检索模式与创建流程。</summary>
public interface IAgentProfile
{
    /// <summary>系统提示词。</summary>
    string SystemPrompt { get; }

    /// <summary>启用的工具组；null 表示全部。</summary>
    string[]? ToolGroups { get; }

    /// <summary>检索模式；null 表示不检索，<c>auto</c> 表示自动注入。</summary>
    string? RetrievalMode { get; }

    /// <summary>在给定会话 scope 上创建 Agent。</summary>
    /// <param name="factory">Agent 工厂。</param>
    /// <param name="modelName">模型限定名（<c>provider:model</c>）。</param>
    /// <param name="workspace">工作区描述。</param>
    /// <param name="ruleEngine">规则引擎。</param>
    /// <param name="pluginRegistry">工具插件注册表。</param>
    /// <param name="skillRegistry">Skill 注册表。</param>
    /// <param name="mcpRegistry">MCP 注册表。</param>
    /// <param name="ct">取消令牌。</param>
    Task<LuBanAgent> CreateAgentAsync(
        ILuBanAgentFactory factory,
        string modelName,
        WorkspaceDescriptor workspace,
        RuleEngine ruleEngine,
        ToolPluginRegistry pluginRegistry,
        SkillRegistry skillRegistry,
        MCPRegistry mcpRegistry,
        CancellationToken ct = default);
}