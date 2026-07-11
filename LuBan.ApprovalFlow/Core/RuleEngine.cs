namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 规则引擎：用于评估网关条件表达式，支持多种运算符和逻辑组合。
/// 提供条件求值功能，用于流程网关的条件路由判断。
/// </summary>
public class RuleEngine
{
    /// <summary>
    /// 评估网关规则集合，返回第一个匹配的规则结果。
    /// </summary>
    /// <param name="rules">网关规则列表。</param>
    /// <param name="variables">流程变量字典。</param>
    /// <param name="defaultEdgeId">默认边ID（无匹配时使用）。</param>
    /// <returns>网关规则评估结果。</returns>
    public GatewayRuleResult Evaluate(
        List<GatewayRule>? rules,
        Dictionary<string, object>? variables,
        string? defaultEdgeId = null)
    {
        // 无规则或无变量时返回默认结果
        if (rules == null || rules.Count == 0 || variables == null)
        {
            return new GatewayRuleResult
            {
                Matched = false,
                EdgeId = defaultEdgeId,
                EdgeText = "default"
            };
        }

        // 遍历规则，返回第一个匹配的结果
        foreach (var rule in rules)
        {
            if (EvaluateConditions(rule.Conditions, rule.Logic, variables))
            {
                return new GatewayRuleResult
                {
                    Matched = true,
                    EdgeId = rule.EdgeId,
                    EdgeText = rule.EdgeText,
                    Rule = rule
                };
            }
        }

        // 无匹配规则时返回默认边
        return new GatewayRuleResult
        {
            Matched = false,
            EdgeId = defaultEdgeId,
            EdgeText = "default"
        };
    }

    /// <summary>
    /// 评估条件集合，根据逻辑运算符（AND/OR）组合结果。
    /// </summary>
    /// <param name="conditions">条件列表。</param>
    /// <param name="logic">逻辑运算符：and 或 or。</param>
    /// <param name="variables">流程变量字典。</param>
    /// <returns>条件是否满足。</returns>
    public bool EvaluateConditions(
        List<RuleCondition>? conditions,
        string logic,
        Dictionary<string, object>? variables)
    {
        // 无条件时默认返回true
        if (conditions == null || conditions.Count == 0 || variables == null)
            return true;

        // 评估每个条件
        var results = conditions.Select(c => EvaluateCondition(c, variables)).ToList();

        // 根据逻辑运算符组合结果
        if (logic == "or")
            return results.Any(r => r);
        else
            return results.All(r => r);
    }

    /// <summary>
    /// 评估单个条件表达式。
    /// </summary>
    /// <param name="condition">规则条件。</param>
    /// <param name="variables">流程变量字典。</param>
    /// <returns>条件是否满足。</returns>
    public bool EvaluateCondition(
        RuleCondition condition,
        Dictionary<string, object>? variables)
    {
        // 变量不存在时的处理
        if (variables == null || !variables.TryGetValue(condition.Field, out var fieldValue))
        {
            return condition.Operator == ConstOperatorType.IsNull;
        }

        var compareValue = condition.Value;

        switch (condition.Operator)
        {
            case ConstOperatorType.Equal:
                // 等于：支持直接比较和字符串比较
                return Equals(fieldValue, compareValue) || ConvertToString(fieldValue) == ConvertToString(compareValue);

            case ConstOperatorType.NotEqual:
                // 不等于：两个值都不相等才返回true
                return !Equals(fieldValue, compareValue) && ConvertToString(fieldValue) != ConvertToString(compareValue);

            case ConstOperatorType.GreaterThan:
                // 大于
                return CompareNumeric(fieldValue, compareValue) > 0;

            case ConstOperatorType.GreaterThanOrEqual:
                // 大于等于
                return CompareNumeric(fieldValue, compareValue) >= 0;

            case ConstOperatorType.LessThan:
                // 小于
                return CompareNumeric(fieldValue, compareValue) < 0;

            case ConstOperatorType.LessThanOrEqual:
                // 小于等于
                return CompareNumeric(fieldValue, compareValue) <= 0;

            case ConstOperatorType.In:
                // 包含于：字段值在给定列表中
                return IsIn(fieldValue, compareValue);

            case ConstOperatorType.NotIn:
                // 不包含于：字段值不在给定列表中
                return !IsIn(fieldValue, compareValue);

            case ConstOperatorType.Contains:
                // 包含：字段值包含给定字符串
                return Contains(fieldValue, compareValue);

            case ConstOperatorType.IsNull:
                // 为空
                return fieldValue == null;

            case ConstOperatorType.IsNotNull:
                // 不为空
                return fieldValue != null;

            case ConstOperatorType.Between:
                // 介于：字段值在指定范围内
                return IsBetween(fieldValue, compareValue);

            default:
                return false;
        }
    }

    /// <summary>
    /// 数值比较：将两个值转换为数值后比较大小。
    /// </summary>
    /// <param name="fieldValue">字段值。</param>
    /// <param name="compareValue">比较值。</param>
    /// <returns>比较结果：大于返回正数，小于返回负数，等于返回0。</returns>
    private int CompareNumeric(object? fieldValue, object? compareValue)
    {
        if (fieldValue == null || compareValue == null) return 0;

        try
        {
            // 尝试数值比较
            var fieldDecimal = Convert.ToDecimal(fieldValue);
            var compareDecimal = Convert.ToDecimal(compareValue);
            return fieldDecimal.CompareTo(compareDecimal);
        }
        catch
        {
            // 数值转换失败时使用字符串比较
            return string.Compare(ConvertToString(fieldValue), ConvertToString(compareValue), StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 判断字段值是否在给定列表中。
    /// </summary>
    /// <param name="fieldValue">字段值。</param>
    /// <param name="compareValue">比较值（支持列表、逗号分隔字符串或单值）。</param>
    /// <returns>是否在列表中。</returns>
    private bool IsIn(object? fieldValue, object? compareValue)
    {
        if (fieldValue == null || compareValue == null) return false;

        // 解析比较值为列表
        List<object> compareList;
        if (compareValue is List<object> objList)
        {
            compareList = objList;
        }
        else if (compareValue is string strValue)
        {
            // 逗号分隔的字符串
            compareList = strValue.Split(',').Select(s => s.Trim() as object).ToList();
        }
        else if (compareValue is List<string> strList)
        {
            compareList = strList.Select(s => s as object).ToList();
        }
        else
        {
            compareList = new List<object> { compareValue };
        }

        // 检查字段值是否在列表中
        var fieldStr = ConvertToString(fieldValue);
        return compareList.Any(v => ConvertToString(v) == fieldStr);
    }

    /// <summary>
    /// 判断字段值是否包含给定字符串。
    /// </summary>
    /// <param name="fieldValue">字段值。</param>
    /// <param name="compareValue">比较值。</param>
    /// <returns>是否包含。</returns>
    private bool Contains(object? fieldValue, object? compareValue)
    {
        if (fieldValue == null || compareValue == null) return false;

        var fieldStr = ConvertToString(fieldValue);
        var compareStr = ConvertToString(compareValue);
        return fieldStr.Contains(compareStr);
    }

    /// <summary>
    /// 判断字段值是否在指定范围内。
    /// </summary>
    /// <param name="fieldValue">字段值。</param>
    /// <param name="compareValue">范围值（包含最小值和最大值的列表）。</param>
    /// <returns>是否在范围内。</returns>
    private bool IsBetween(object? fieldValue, object? compareValue)
    {
        if (fieldValue == null || compareValue == null) return false;

        try
        {
            var fieldDecimal = Convert.ToDecimal(fieldValue);
            var rangeList = compareValue as List<object> ?? new List<object> { compareValue };

            if (rangeList.Count >= 2)
            {
                var min = Convert.ToDecimal(rangeList[0]);
                var max = Convert.ToDecimal(rangeList[1]);
                return fieldDecimal >= min && fieldDecimal <= max;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// 将对象转换为字符串。
    /// </summary>
    /// <param name="value">要转换的对象。</param>
    /// <returns>字符串结果。</returns>
    private string ConvertToString(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }
}