/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Core
*文件名： ReportColumn
*版本号： V1.0.0.0
*唯一标识：3360d754-9cd5-4ca6-9e06-91871171bdfa
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 11:17:26
*描述：报表列信息
*
*=================================================
*修改标记
*修改时间：2025/2/24 11:17:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报表列信息
*
*****************************************************************************/
namespace LuBan.Reporting.Core;

/// <summary>
/// 报表列信息
/// </summary>
public class ReportColumn
{
    /// <summary>
    /// 属性名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 属性名和标题
    /// </summary>
    public NamePair NamePair { get; set; }
    /// <summary>
    /// 布尔值
    /// </summary>
    public List<string>? BoolValues { get; set; }
    /// <summary>
    /// 枚举值
    /// </summary>
    public List<string>? EnumValues { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }

    /// <summary>
    /// 格式化字符串
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 显示类型
    /// </summary>
    public Type DisplayType { get; set; }


    /// <summary>
    /// 自定义委托转换
    /// </summary>
    public Tuple<Type, string>? CustormConvert { get; set; }

    /// <summary>
    /// 报表列信息
    /// </summary>
    /// <param name="displayType"></param>
    /// <param name="sortNo"></param>
    /// <param name="name"></param>
    /// <param name="title"></param>
    /// <param name="boolValues"></param>
    /// <param name="enumValues"></param>
    /// <param name="datetimeFormat"></param>
    public ReportColumn(Type displayType, int sortNo, string name, string title, List<string>? boolValues = null, List<string>? enumValues = null, string datetimeFormat = "yyyy-MM-dd HH:mm:ss", Tuple<Type, string>? custormConvert = null)
    {
        DisplayType = displayType;
        if (sortNo <= 0) sortNo = 1;
        SortNo = sortNo;
        Name = name;
        Title = title;
        NamePair = new NamePair(name, title);
        BoolValues = boolValues;
        EnumValues = enumValues;
        DateTimeFormat = datetimeFormat;
        CustormConvert = custormConvert;
    }
}
