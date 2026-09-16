/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： OrchestrationProgressContent
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/16
*描述：编排进度内容，作为流式响应中的 AIContent 载体，供上层 UI 实时渲染编排进度
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 编排进度内容。编排分支为"规划 + 执行"的长耗时非流式过程，
/// 本类型把 <see cref="OrchestrationProgress"/> 包装成 <see cref="AIContent"/>，
/// 随 <c>AgentResponseUpdate</c> 实时产出，使上层 UI 能边执行边展示进度。
/// </summary>
public class OrchestrationProgressContent : AIContent
{
    /// <summary>
    /// 创建编排进度内容。
    /// </summary>
    /// <param name="eventType">事件类型。</param>
    /// <param name="nodeId">关联节点标识（可选）。</param>
    /// <param name="message">事件消息（可选）。</param>
    /// <param name="nodeResult">关联的节点结果（可选，节点完成/失败时提供耗时与错误）。</param>
    /// <param name="activity">节点内部活动明细（可选，节点执行期的思考/工具调用等）。</param>
    public OrchestrationProgressContent(
        ProgressEventType eventType,
        string? nodeId = null,
        string? message = null,
        NodeResult? nodeResult = null,
        NodeActivityItem? activity = null)
    {
        EventType = eventType;
        NodeId = nodeId;
        Message = message;
        NodeResult = nodeResult;
        Activity = activity;
    }

    /// <summary>
    /// 获取事件类型。
    /// </summary>
    public ProgressEventType EventType { get; }

    /// <summary>
    /// 获取关联的节点标识。
    /// </summary>
    public string? NodeId { get; }

    /// <summary>
    /// 获取事件消息。
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// 获取关联的节点结果（节点完成/失败时提供耗时与错误信息）。
    /// </summary>
    public NodeResult? NodeResult { get; }

    /// <summary>
    /// 获取节点内部活动明细（思考/正文/工具调用/工具结果）。
    /// </summary>
    public NodeActivityItem? Activity { get; }

    /// <summary>
    /// 由进度事件创建进度内容。
    /// </summary>
    /// <param name="progress">编排进度事件。</param>
    /// <returns>进度内容实例。</returns>
    public static OrchestrationProgressContent From(OrchestrationProgress progress)
        => new(progress.EventType, progress.NodeId, progress.Message, progress.NodeResult, progress.Activity);
}