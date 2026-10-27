/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： DefaultAgentProfile
*版本号： V1.0.0.0
*唯一标识：默认通用助手 Profile
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：默认通用助手 Profile
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

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