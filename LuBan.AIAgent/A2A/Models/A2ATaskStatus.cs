/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Models
*文件名： A2ATaskStatus
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 任务状态，框架原生、SDK 无关
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A.Models;

/// <summary>
/// A2A 任务状态。对应协议 <c>TaskStatus</c>。
/// </summary>
public class A2ATaskStatus
{
    /// <summary>状态取值。</summary>
    public A2ATaskState State { get; set; } = A2ATaskState.Unspecified;

    /// <summary>状态附带消息（如中断态的追问）。</summary>
    public A2AMessage? Message { get; set; }

    /// <summary>状态时间戳。</summary>
    public DateTimeOffset? Timestamp { get; set; }
}