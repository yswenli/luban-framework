/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2AAgentDescriptor
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A Agent 描述符（AgentCard 的框架表示），SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 技能描述符（AgentSkill 的框架表示）。
/// </summary>
public class A2ASkillDescriptor
{
    /// <summary>技能标识（内部角色名，如 "analyst"）。</summary>
    public string Id { get; set; } = "";

    /// <summary>技能名称。</summary>
    public string Name { get; set; } = "";

    /// <summary>技能描述。</summary>
    public string? Description { get; set; }

    /// <summary>标签。</summary>
    public List<string>? Tags { get; set; }

    /// <summary>示例输入。</summary>
    public List<string>? Examples { get; set; }
}

/// <summary>
/// A2A Agent 接口（AgentInterface 的框架表示）。
/// </summary>
public class A2AInterface
{
    /// <summary>接口 URL。</summary>
    public string Url { get; set; } = "";

    /// <summary>协议绑定（如 "JSONRPC"）。</summary>
    public string? ProtocolBinding { get; set; }

    /// <summary>协议版本。</summary>
    public string? ProtocolVersion { get; set; }

    /// <summary>租户。</summary>
    public string? Tenant { get; set; }
}

/// <summary>
/// A2A 安全方案（SecurityScheme 的框架表示）。
/// </summary>
public class A2ASecurityScheme
{
    /// <summary>方案类型（如 "apiKey"）。</summary>
    public string Type { get; set; } = "";

    /// <summary>参数名（如 "X-Api-Key"）。</summary>
    public string? Name { get; set; }

    /// <summary>参数位置（如 "header"）。</summary>
    public string? In { get; set; }
}

/// <summary>
/// A2A Agent 描述符。对应协议 <c>AgentCard</c>，框架原生表示；
/// 由 <c>LubanAgentA2A</c> 的映射层转换为 SDK <c>AgentCard</c>。
/// </summary>
public class A2AAgentDescriptor
{
    /// <summary>Agent 名称。</summary>
    public string Name { get; set; } = "";

    /// <summary>Agent 描述。</summary>
    public string? Description { get; set; }

    /// <summary>Agent 版本。</summary>
    public string Version { get; set; } = "";

    /// <summary>接口列表。</summary>
    public List<A2AInterface>? Interfaces { get; set; }

    /// <summary>技能列表。</summary>
    public List<A2ASkillDescriptor>? Skills { get; set; }

    /// <summary>默认输入模式。</summary>
    public List<string>? DefaultInputModes { get; set; }

    /// <summary>默认输出模式。</summary>
    public List<string>? DefaultOutputModes { get; set; }

    /// <summary>安全方案字典（名称 → 方案）。</summary>
    public Dictionary<string, A2ASecurityScheme>? SecuritySchemes { get; set; }

    /// <summary>安全需求（引用 <see cref="SecuritySchemes"/> 中的名称）。</summary>
    public List<Dictionary<string, List<string>>>? SecurityRequirements { get; set; }

    /// <summary>是否支持扩展 AgentCard。</summary>
    public bool SupportsExtendedCard { get; set; }

    /// <summary>扩展 URI 声明。</summary>
    public List<string>? Extensions { get; set; }
}