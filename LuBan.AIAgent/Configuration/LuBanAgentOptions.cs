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

    /// <summary>
    /// Session 历史与压缩配置
    /// </summary>
    public SessionOptions Session { get; set; } = new();
}

/// <summary>
/// Session 历史与压缩配置
/// </summary>
public class SessionOptions
{
    /// <summary>
    /// 压缩后保留的消息数（默认 20）
    /// </summary>
    public int CompactTargetMessages { get; set; } = 20;

    /// <summary>
    /// 超出保留数多少条后触发压缩（默认 10，即超过 30 条触发）
    /// </summary>
    public int CompactThreshold { get; set; } = 10;
}