/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： AppConfig
*版本号： V1.0.0.0
*唯一标识：4f4ebb0a-2243-460c-8f80-23d3168f5b17
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：应用配置模型
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：应用配置模型
*
*****************************************************************************/
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