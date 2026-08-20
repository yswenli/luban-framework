/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： ProgressEventType
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排进度事件类型枚举
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 编排进度事件类型枚举。
/// </summary>
public enum ProgressEventType
{
    /// <summary>
    /// 规划开始。
    /// </summary>
    PlanningStarted,

    /// <summary>
    /// 规划完成。
    /// </summary>
    PlanningCompleted,

    /// <summary>
    /// 节点开始执行。
    /// </summary>
    NodeStarted,

    /// <summary>
    /// 节点执行完成。
    /// </summary>
    NodeCompleted,

    /// <summary>
    /// 节点执行失败。
    /// </summary>
    NodeFailed,

    /// <summary>
    /// 层级执行完成。
    /// </summary>
    LayerCompleted,

    /// <summary>
    /// 编排完成。
    /// </summary>
    OrchestratingCompleted
}
