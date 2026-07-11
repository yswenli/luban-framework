namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 规则条件，定义单个条件表达式。
/// </summary>
public class RuleCondition
{
    /// <summary>
    /// 条件字段名称。
    /// </summary>
    public string Field { get; set; } = string.Empty;
    /// <summary>
    /// 比较操作符：eq/ne/gt/gte/lt/lte/contains/startswith/endswith/empty/notempty/in/notin。
    /// </summary>
    public string Operator { get; set; } = ConstOperatorType.Equal;
    /// <summary>
    /// 比较值。
    /// </summary>
    public object? Value { get; set; }
}