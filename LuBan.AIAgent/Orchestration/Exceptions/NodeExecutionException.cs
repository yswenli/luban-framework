/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： NodeExecutionException
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：节点执行异常，聚合多个节点失败信息
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 节点执行异常，聚合多个节点失败信息。
/// </summary>
public class NodeExecutionException : Exception
{
    /// <summary>
    /// 获取失败节点结果列表。
    /// </summary>
    public List<NodeResult> FailedNodes { get; }

    /// <summary>
    /// 创建 NodeExecutionException 实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="failedNodes">失败节点结果列表。</param>
    public NodeExecutionException(string message, List<NodeResult> failedNodes)
        : base(message)
    {
        FailedNodes = failedNodes;
    }
}
