namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 网关规则，定义条件网关的分支路由规则。
/// </summary>
public class GatewayRule
{
    /// <summary>
    /// 关联的边ID，用于确定路由目标。
    /// </summary>
    public string EdgeId { get; set; } = string.Empty;
    /// <summary>
    /// 边的显示文本，如"通过"、"退回"等。
    /// </summary>
    public string? EdgeText { get; set; }
    /// <summary>
    /// 条件列表，用于判断是否匹配此规则。
    /// </summary>
    public List<RuleCondition>? Conditions { get; set; }
    /// <summary>
    /// 条件逻辑：and/or，默认and。
    /// </summary>
    public string Logic { get; set; } = "and";
}