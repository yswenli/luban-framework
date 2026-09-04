/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： DelegateEventArgs.cs
*版本号： V1.0.0.0
*唯一标识：5980894d-0b11-4de2-bc33-e82329ec4948
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：DelegateEventArgs 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DelegateEventArgs 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 委托事件参数，当设置或取消委托关系时触发。
/// </summary>
public class DelegateEventArgs : EventArgs
{
    /// <summary>
    /// 委托记录ID。
    /// </summary>
    public long DelegationId { get; }
    /// <summary>
    /// 委托人用户ID。
    /// </summary>
    public long FromUserId { get; }
    /// <summary>
    /// 委托人名称。
    /// </summary>
    public string FromUserName { get; }
    /// <summary>
    /// 受托人用户ID。
    /// </summary>
    public long ToUserId { get; }
    /// <summary>
    /// 受托人名称。
    /// </summary>
    public string ToUserName { get; }
    /// <summary>
    /// 委托开始时间。
    /// </summary>
    public DateTime StartTime { get; }
    /// <summary>
    /// 托结束时间。
    /// </summary>
    public DateTime EndTime { get; }
    /// <summary>
    /// 委托原因。
    /// </summary>
    public string? Reason { get; }
    /// <summary>
    /// 委托范围角色列表。
    /// </summary>
    public List<string>? ScopeRoles { get; }
    /// <summary>
    /// 操作动作：create/update/delete。
    /// </summary>
    public string Action { get; }
    /// <summary>
    /// 操作时间。
    /// </summary>
    public DateTime ActionTime { get; }

    /// <summary>
    /// 构造委托事件参数。
    /// </summary>
    public DelegateEventArgs(long delegationId, long fromUserId, string fromUserName, long toUserId, string toUserName, DateTime startTime, DateTime endTime, string? reason, List<string>? scopeRoles, string action, DateTime actionTime)
    {
        DelegationId = delegationId;
        FromUserId = fromUserId;
        FromUserName = fromUserName;
        ToUserId = toUserId;
        ToUserName = toUserName;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason;
        ScopeRoles = scopeRoles;
        Action = action;
        ActionTime = actionTime;
    }
}