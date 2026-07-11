namespace LuBan.AIAgent.Core;

/// <summary>
/// 工具策略选项，用于配置工具的执行权限
/// </summary>
public sealed class ToolPolicyOptions
{
    /// <summary>
    /// 允许执行的工具名称集合，若为null则允许所有工具
    /// </summary>
    public IReadOnlyCollection<string>? AllowedToolNames { get; set; }

    /// <summary>
    /// 拒绝执行的工具名称集合
    /// </summary>
    public IReadOnlyCollection<string>? DeniedToolNames { get; set; }

    /// <summary>
    /// 拒绝执行时的消息
    /// </summary>
    public string? DenialMessage { get; set; }
}
