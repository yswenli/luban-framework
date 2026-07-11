/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Attrs
*文件名： ReportDescriptionAttribute
*版本号： V1.0.0.0
*唯一标识：585a2d04-bc93-42b4-a994-5b6533fb8cc7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 10:26:39
*描述：报表描述
*
*=================================================
*修改标记
*修改时间：2025/2/24 10:26:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报表描述
*
*****************************************************************************/
namespace LuBan.Reporting.Attrs;



/// <remarks>
/// 报表描述
/// </remarks>
/// <param name="title"></param>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class ReportDescriptionAttribute : Attribute
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 序号
    /// </summary>
    public int SortNo { get; set; }

    /// <summary>
    /// 布尔值
    /// </summary>
    public List<string>? BoolValues { get; set; } = ["是", "否"];

    /// <summary>
    /// 枚举类型
    /// </summary>
    public List<string>? EnumValues { get; set; }

    /// <summary>
    /// 日期时间格式
    /// </summary>
    public string DatetimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 自定义委托转换
    /// </summary>
    public Tuple<Type, string>? CustormConvert { get; set; }

    /// <summary>
    /// 报表描述
    /// </summary>
    /// <param name="title">列标题</param>
    /// <param name="sortNo">列位置</param>
    /// <param name="boolValues"></param>
    /// <param name="enumValues">根据值自动枚举，值必须和枚举完全对应</param>
    /// <param name="datetimeFormat">日期格式</param>
    public ReportDescriptionAttribute(string title, int sortNo, string? boolValues = "", string? enumValues = "", string datetimeFormat = "yyyy-MM-dd HH:mm:ss")
    {
        Title = title;
        SortNo = sortNo;
        if (boolValues.IsNotNullOrEmpty())
        {
            var boolValuesArr = boolValues.Split(',');
            if (boolValuesArr.Length == 2)
                BoolValues = boolValuesArr.ToList();
        }
        if (enumValues.IsNotNullOrEmpty())
        {
            var enumValuesArr = enumValues.Split(',');
            if (enumValuesArr != null && enumValuesArr.Length > 0)
                EnumValues = enumValuesArr.ToList();
        }
        if (datetimeFormat.IsNullOrEmpty())
        {
            DatetimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
        else
        {
            DatetimeFormat = datetimeFormat;
        }
    }

    /// <summary>
    /// 报表描述，自动枚举：自动获取枚举描述
    /// </summary>
    /// <param name="title">列标题</param>
    /// <param name="sortNo">列位置</param>
    /// <param name="enumType">根据值自动枚举，值必须和枚举完全对应</param>
    public ReportDescriptionAttribute(string title, int sortNo, Type enumType)
    {
        Title = title;
        SortNo = sortNo;
        EnumValues = enumType.GetDescriptions();
        if (EnumValues == null || EnumValues.Count == 0)
            EnumValues = Enum.GetNames(enumType).ToList();

    }

    /// <summary>
    /// 报表描述，自定义委托方法转换
    /// </summary>
    /// <param name="title">列标题</param>
    /// <param name="sortNo">列位置</param>
    /// <param name="custormConvert">自定义值的转换方法</param>
    public ReportDescriptionAttribute(string title, int sortNo, Tuple<Type, string> custormConvert)
    {
        Title = title;
        SortNo = sortNo;
        CustormConvert = custormConvert;
    }
}
