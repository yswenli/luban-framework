namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 委托请求，设置任务委托关系。
/// </summary>
public class DelegateRequest
{
    /// <summary>
    /// 委托人用户ID。
    /// </summary>
    public long FromUserId { get; set; }
    /// <summary>
    /// 托人名称。
    /// </summary>
    public string FromUserName { get; set; } = string.Empty;
    /// <summary>
    /// 受托人用户ID。
    /// </summary>
    public long ToUserId { get; set; }
    /// <summary>
    /// 受托人名称。
    /// </summary>
    public string ToUserName { get; set; } = string.Empty;
    /// <summary>
    /// 委托开始时间。
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// 托结束时间。
    /// </summary>
    public DateTime EndTime { get; set; }
    /// <summary>
    /// 委托原因。
    /// </summary>
    public string? Reason { get; set; }
    /// <summary>
    /// 委托范围角色列表，限定特定角色的任务才委托。
    /// </summary>
    public List<string>? ScopeRoles { get; set; }
}