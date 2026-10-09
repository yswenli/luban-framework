/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2ATask
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 任务，框架原生、SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 任务。对应协议 <c>Task</c>，框架原生表示。
/// </summary>
public class A2ATask
{
    /// <summary>任务标识。</summary>
    public string Id { get; set; } = "";

    /// <summary>所属上下文标识。</summary>
    public string? ContextId { get; set; }

    /// <summary>任务状态。</summary>
    public A2ATaskStatus Status { get; set; } = new();

    /// <summary>消息历史。</summary>
    public List<A2AMessage>? History { get; set; }

    /// <summary>产出物列表。</summary>
    public List<A2AArtifact>? Artifacts { get; set; }

    /// <summary>附加元数据（如 <c>partial=true</c>）。</summary>
    public Dictionary<string, string>? Metadata { get; set; }
}