namespace LuBan.AIAgent.Configuration;

/// <summary>
/// LuBan Agent 配置选项
/// </summary>
public class LuBanAgentOptions
{
    /// <summary>
    /// 默认模型名称，格式 "provider:model"
    /// </summary>
    public string? DefaultModel { get; set; }

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Agent 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 最大工具循环迭代次数
    /// </summary>
    public int MaxToolLoopIterations { get; set; } = 10;

    /// <summary>
    /// 模型端点配置字典
    /// </summary>
    public Dictionary<string, ModelEndpointOptions> Models { get; set; } = new();

    /// <summary>
    /// 工具组配置
    /// </summary>
    public ToolGroupOptions Tools { get; set; } = new();

    /// <summary>
    /// 外部插件程序集列表
    /// </summary>
    public List<string> ExternalPlugins { get; set; } = new();
}