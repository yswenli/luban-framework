/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2AStreamEvent
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 流式事件，框架原生、SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 流式事件种类。
/// </summary>
public enum A2AStreamEventKind
{
    /// <summary>状态更新（对应 TaskStatusUpdateEvent）。</summary>
    Status = 0,

    /// <summary>产出物更新（对应 TaskArtifactUpdateEvent）。</summary>
    Artifact = 1
}

/// <summary>
/// A2A 流式事件。对应协议 <c>TaskStatusUpdateEvent</c> / <c>TaskArtifactUpdateEvent</c>，
/// 框架原生表示；不支持 A2A 流式语义时可退化为单个 <see cref="A2AStreamEventKind.Status"/> 事件。
/// </summary>
public class A2AStreamEvent
{
    /// <summary>事件种类。</summary>
    public A2AStreamEventKind Kind { get; set; }

    /// <summary>任务标识。</summary>
    public string TaskId { get; set; } = "";

    /// <summary>上下文标识。</summary>
    public string? ContextId { get; set; }

    /// <summary>状态载荷；<see cref="A2AStreamEventKind.Status"/> 时有效。</summary>
    public A2ATaskStatus? Status { get; set; }

    /// <summary>产出物载荷；<see cref="A2AStreamEventKind.Artifact"/> 时有效。</summary>
    public A2AArtifact? Artifact { get; set; }

    /// <summary>附加元数据（如 <c>{ eventType, nodeId, activityKind }</c>）。</summary>
    public Dictionary<string, string>? Metadata { get; set; }
}