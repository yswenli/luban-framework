/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： IAppConfigReader
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：应用配置读取器接口及配置数据结构定义
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 应用配置读取器接口
/// </summary>
public interface IAppConfigReader
{
    /// <summary>
    /// 提供者配置列表
    /// </summary>
    List<ProviderConfigData> Providers { get; }

    /// <summary>
    /// 当前选中的模型名称
    /// </summary>
    string? SelectedModel { get; }

    /// <summary>
    /// 自定义技能配置列表
    /// </summary>
    List<CustomSkillConfig> CustomSkills { get; }

    /// <summary>
    /// 自定义规则配置列表
    /// </summary>
    List<CustomRuleConfig> CustomRules { get; }

    /// <summary>
    /// MCP 服务器配置列表
    /// </summary>
    List<McpServerConfig> McpServers { get; }

    /// <summary>
    /// 禁用的内置技能列表
    /// </summary>
    List<string> DisabledBuiltinSkills { get; }

    /// <summary>
    /// 禁用的内置规则列表
    /// </summary>
    List<string> DisabledBuiltinRules { get; }

    /// <summary>
    /// 禁用的内置 MCP 客户端列表
    /// </summary>
    List<string> DisabledBuiltinMcpClients { get; }
}

/// <summary>
/// 模型提供者配置数据
/// </summary>
public class ProviderConfigData
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// 服务端点地址
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// 支持的模型列表
    /// </summary>
    public List<string> Models { get; set; } = new();
}
