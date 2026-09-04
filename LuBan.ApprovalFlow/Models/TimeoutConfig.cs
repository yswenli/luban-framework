/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： TimeoutConfig.cs
*版本号： V1.0.0.0
*唯一标识：1cee8d5d-8288-4181-a18b-5ca3722f9431
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：TimeoutConfig 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：TimeoutConfig 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 超时配置，定义节点超时处理策略。
/// </summary>
public class TimeoutConfig
{
    /// <summary>
    /// 超时时间（小时）。
    /// </summary>
    public int? TimeoutHours { get; set; }
    /// <summary>
    /// 超时动作：auto_approve/auto_reject/notify/transfer。
    /// </summary>
    public string? TimeoutAction { get; set; }
    /// <summary>
    /// 提前通知时间（小时）。
    /// </summary>
    public int? NotifyBeforeHours { get; set; }
    /// <summary>
    /// 通知间隔时间列表（小时）。
    /// </summary>
    public List<int>? NotifyIntervalHours { get; set; }
    /// <summary>
    /// 通知目标列表。
    /// </summary>
    public List<NotifyTarget>? NotifyTargets { get; set; }
}

/// <summary>
/// 通知目标，定义超时通知的接收对象。
/// </summary>
public class NotifyTarget
{
    /// <summary>
    /// 目标类型：user/role/initiator/assignee。
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// 用户ID（当Type为user时使用）。
    /// </summary>
    public long? UserId { get; set; }
    /// <summary>
    /// 角色编码（当Type为role时使用）。
    /// </summary>
    public string? Role { get; set; }
}