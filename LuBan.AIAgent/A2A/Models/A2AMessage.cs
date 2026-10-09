/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2AMessage
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 消息，框架原生、SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 消息角色。
/// </summary>
public enum A2ARole
{
    /// <summary>用户。</summary>
    User = 0,

    /// <summary>代理。</summary>
    Agent = 1
}

/// <summary>
/// A2A 消息。对应协议 <c>Message</c>，框架原生表示。
/// </summary>
public class A2AMessage
{
    /// <summary>消息角色。</summary>
    public A2ARole Role { get; set; } = A2ARole.User;

    /// <summary>内容部件列表。</summary>
    public List<A2APart> Parts { get; set; } = new();

    /// <summary>消息标识。</summary>
    public string? MessageId { get; set; }

    /// <summary>所属上下文标识。</summary>
    public string? ContextId { get; set; }

    /// <summary>所属任务标识。</summary>
    public string? TaskId { get; set; }

    /// <summary>引用的其它任务标识（用于表达节点依赖等）。</summary>
    public List<string>? ReferenceTaskIds { get; set; }

    /// <summary>扩展字段。</summary>
    public Dictionary<string, string>? Extensions { get; set; }

    /// <summary>附加元数据。</summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
