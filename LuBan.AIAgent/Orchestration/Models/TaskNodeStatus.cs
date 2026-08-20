/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： TaskNodeStatus
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：任务节点状态机枚举
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 任务节点状态机枚举。
/// </summary>
public enum TaskNodeStatus
{
    /// <summary>
    /// 待执行。
    /// </summary>
    Pending,

    /// <summary>
    /// 正在执行。
    /// </summary>
    Running,

    /// <summary>
    /// 执行成功。
    /// </summary>
    Succeeded,

    /// <summary>
    /// 执行失败。
    /// </summary>
    Failed,

    /// <summary>
    /// 被跳过（前驱关键节点失败或被取消）。
    /// </summary>
    Skipped,

    /// <summary>
    /// 已取消（整个图谱被取消）。
    /// </summary>
    Cancelled
}
