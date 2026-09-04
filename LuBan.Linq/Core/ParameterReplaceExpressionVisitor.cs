/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Linq.Core
*文件名： ParameterReplaceExpressionVisitor.cs
*版本号： V1.0.0.0
*唯一标识：2da4e1de-aa72-45f6-833f-0da5707ab127
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ParameterReplaceExpressionVisitor 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ParameterReplaceExpressionVisitor 类
*
*****************************************************************************/

namespace LuBan.Linq.Core;

/// <summary>
/// 处理 Lambda 参数不一致问题
/// </summary>
internal sealed class ParameterReplaceExpressionVisitor : ExpressionVisitor
{
    /// <summary>
    /// 参数表达式映射集合
    /// </summary>
    private readonly Dictionary<ParameterExpression, ParameterExpression> _parameterExpressionSetter;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="parameterExpressionSetter">参数表达式映射集合</param>
    public ParameterReplaceExpressionVisitor(Dictionary<ParameterExpression, ParameterExpression> parameterExpressionSetter)
    {
        _parameterExpressionSetter = parameterExpressionSetter ?? [];
    }

    /// <summary>
    /// 替换表达式参数
    /// </summary>
    /// <param name="parameterExpressionSetter">参数表达式映射集合</param>
    /// <param name="expression">表达式</param>
    /// <returns>新的表达式</returns>
    public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> parameterExpressionSetter, Expression expression)
    {
        return new ParameterReplaceExpressionVisitor(parameterExpressionSetter).Visit(expression);
    }

    /// <summary>
    /// 重写基类参数访问器
    /// </summary>
    /// <param name="parameterExpression"></param>
    /// <returns></returns>
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
    {
        if (_parameterExpressionSetter.TryGetValue(parameterExpression, out var replacement))
        {
            parameterExpression = replacement;
        }

        return base.VisitParameter(parameterExpression);
    }
}