/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： IAgentProfile
*版本号： V1.0.0.0
*唯一标识：35cb64eb-22e7-4c96-8325-7858c9da4212
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：Agent 配置契约：系统提示词、工具组、检索模式与建 Agent 流程
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

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