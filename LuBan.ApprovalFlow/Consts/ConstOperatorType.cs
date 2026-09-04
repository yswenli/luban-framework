/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstOperatorType.cs
*版本号： V1.0.0.0
*唯一标识：ff1e4e07-fd40-4abb-97aa-6fcd7327d5c2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstOperatorType 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstOperatorType 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 操作符类型常量，用于定义条件表达式的比较操作符
/// </summary>
public class ConstOperatorType
{
    /// <summary>
    /// 等于
    /// </summary>
    public const string Equal = "==";

    /// <summary>
    /// 不等于
    /// </summary>
    public const string NotEqual = "!=";

    /// <summary>
    /// 大于
    /// </summary>
    public const string GreaterThan = ">";

    /// <summary>
    /// 大于等于
    /// </summary>
    public const string GreaterThanOrEqual = ">=";

    /// <summary>
    /// 小于
    /// </summary>
    public const string LessThan = "<";

    /// <summary>
    /// 小于等于
    /// </summary>
    public const string LessThanOrEqual = "<=";

    /// <summary>
    /// 包含于（在指定集合中）
    /// </summary>
    public const string In = "in";

    /// <summary>
    /// 不包含于（不在指定集合中）
    /// </summary>
    public const string NotIn = "notIn";

    /// <summary>
    /// 包含（字符串包含子串）
    /// </summary>
    public const string Contains = "contains";

    /// <summary>
    /// 为空
    /// </summary>
    public const string IsNull = "isNull";

    /// <summary>
    /// 不为空
    /// </summary>
    public const string IsNotNull = "isNotNull";

    /// <summary>
    /// 在指定范围内
    /// </summary>
    public const string Between = "between";
}