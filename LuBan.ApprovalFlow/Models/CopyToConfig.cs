namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 抄送配置，定义抄送目标和触发条件。
/// </summary>
public class CopyToConfig
{
    /// <summary>
    /// 抄送目标列表。
    /// </summary>
    public List<CopyToTarget>? Targets { get; set; }
    /// <summary>
    /// 触发事件：on_complete/on_approve/on_reject/on_start。
    /// </summary>
    public string TriggerEvent { get; set; } = string.Empty;
    /// <summary>
    /// 是否包含表单数据。
    /// </summary>
    public bool? IncludePayload { get; set; }
    /// <summary>
    /// 是否包含审批历史。
    /// </summary>
    public bool? IncludeHistory { get; set; }
    /// <summary>
    /// 通知模板编码。
    /// </summary>
    public string? NotifyTemplate { get; set; }
}

/// <summary>
/// 抄送目标，定义抄送接收对象。
/// </summary>
public class CopyToTarget
{
    /// <summary>
    /// 目标类型：user/role/expression。
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
    /// <summary>
    /// 表达式（当Type为expression时使用，如"${initiatorUserId}")。
    /// </summary>
    public string? Expression { get; set; }
}