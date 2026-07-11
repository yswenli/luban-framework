/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumCultureLevel
*版本号： V1.0.0.0
*唯一标识：6db01775-6055-4e04-a378-c8d26b49efdc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:03:33
*描述：文化程度枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:03:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文化程度枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 文化程度枚举
/// </summary>
[Description("文化程度枚举")]
public enum EnumCultureLevel
{
    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Level0 = 0,

    /// <summary>
    /// 小学
    /// </summary>
    [Description("小学")]
    Level1 = 1,

    /// <summary>
    /// 初中
    /// </summary>
    [Description("初中")]
    Level2 = 2,

    /// <summary>
    /// 普通高中
    /// </summary>
    [Description("普通高中")]
    Level3 = 3,

    /// <summary>
    /// 技工学校
    /// </summary>
    [Description("技工学校")]
    Level4 = 4,

    /// <summary>
    /// 职业教育
    /// </summary>
    [Description("职业教育")]
    Level5 = 5,

    /// <summary>
    /// 职业高中
    /// </summary>
    [Description("职业高中")]
    Level6 = 6,

    /// <summary>
    /// 中等专科
    /// </summary>
    [Description("中等专科")]
    Level7 = 7,

    /// <summary>
    /// 大学专科
    /// </summary>
    [Description("大学专科")]
    Level8 = 8,

    /// <summary>
    /// 大学本科
    /// </summary>
    [Description("大学本科")]
    Level9 = 9,

    /// <summary>
    /// 硕士研究生
    /// </summary>
    [Description("硕士研究生")]
    Level10 = 10,

    /// <summary>
    /// 博士研究生
    /// </summary>
    [Description("博士研究生")]
    Level11 = 11,
}
