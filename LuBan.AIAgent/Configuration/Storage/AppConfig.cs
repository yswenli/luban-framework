namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Agent 应用配置
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Provider 配置列表
    /// </summary>
    public List<ProviderConfig> Providers { get; set; } = new();

    /// <summary>
    /// 当前选择的模型（格式: provider:model）
    /// </summary>
    public string? SelectedModel { get; set; }

    /// <summary>
    /// 自定义 Skill 列表
    /// </summary>
    public List<CustomSkillConfig> CustomSkills { get; set; } = new();

    /// <summary>
    /// 自定义规则列表
    /// </summary>
    public List<CustomRuleConfig> CustomRules { get; set; } = new();

    /// <summary>
    /// 外部 MCP 服务器列表
    /// </summary>
    public List<McpServerConfig> McpServers { get; set; } = new();

    /// <summary>
    /// 内置 Skill 禁用列表（按 Id）
    /// </summary>
    public List<string> DisabledBuiltinSkills { get; set; } = new();

    /// <summary>
    /// 内置规则禁用列表（按 Id）
    /// </summary>
    public List<string> DisabledBuiltinRules { get; set; } = new();

    /// <summary>
    /// 内置 MCP 客户端禁用列表（按 Name）
    /// </summary>
    public List<string> DisabledBuiltinMcpClients { get; set; } = new();
}