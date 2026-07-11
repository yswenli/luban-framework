namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 网关规则匹配结果，表示条件判断后的路由信息。
/// </summary>
public class GatewayRuleResult
{
    /// <summary>
    /// 是否匹配到规则。
    /// </summary>
    public bool Matched { get; set; }
    /// <summary>
    /// 匹配的边ID。
    /// </summary>
    public string? EdgeId { get; set; }
    /// <summary>
    /// 匹配的边文本。
    /// </summary>
    public string? EdgeText { get; set; }
    /// <summary>
    /// 匹配的规则对象。
    /// </summary>
    public GatewayRule? Rule { get; set; }
}