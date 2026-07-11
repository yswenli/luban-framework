/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Core
*文件名： ReportBase
*版本号： V1.0.0.0
*唯一标识：f7b6bf8a-7ebd-46b1-b523-fd3fe5f6245f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 10:59:06
*描述：报告基类
*
*=================================================
*修改标记
*修改时间：2025/2/24 10:59:06
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报告基类
*
*****************************************************************************/

namespace LuBan.Reporting.Core;

using System.Collections.Concurrent;

/// <summary>
/// 报告基类
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
internal abstract class ReportBase<T> where T : class, new()
{
    /// <summary>
    /// 数据列表
    /// </summary>
    internal protected List<T> Data { get; set; }

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    private static readonly ConcurrentDictionary<Type, List<ReportColumn>> _columnInfoCache = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">数据列表</param>
    internal ReportBase(List<T> data)
    {
        if (data == null || data.Count < 1) throw new Exception("data is null or empty");
        Data = data;
    }

    /// <summary>
    /// 获取列信息
    /// </summary>
    /// <returns>列信息列表</returns>
    protected List<ReportColumn> GetColumnInfos()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var result = new List<ReportColumn>(properties.Length);
        int sortNo = 0;

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<ReportIgnoreAttribute>() != null) continue;

            var attribute = property.GetCustomAttribute<ReportDescriptionAttribute>();
            if (attribute == null)
            {
                sortNo++;
                result.Add(new ReportColumn(property.PropertyType, sortNo, property.Name, property.Name));
            }
            else
            {
                sortNo = attribute.SortNo;
                var displayType = property.PropertyType;

                if (property.PropertyType.IsNullable())
                {
                    displayType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                }

                if (displayType == typeof(bool) ||
                    displayType == typeof(DateTime) ||
                    displayType.IsEnum)
                {
                    displayType = typeof(string);
                }

                result.Add(new ReportColumn(displayType, sortNo, property.Name, attribute.Title, attribute.BoolValues, attribute.EnumValues, attribute.DatetimeFormat, attribute.CustormConvert));
            }
        }

        return result.OrderBy(q => q.SortNo).ToList();
    }

    /// <summary>
    /// 获取数据表
    /// </summary>
    /// <returns>数据表</returns>
    protected DataTable GetDataTable()
    {
        var dt = new DataTable();
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var columns = GetColumnInfos();

        foreach (var c in columns)
        {
            dt.Columns.Add(c.Title, c.DisplayType);
        }

        foreach (var item in Data)
        {
            if (item == null) continue;

            var newRow = dt.NewRow();

            foreach (var pi in properties)
            {
                if (pi == null || !pi.CanRead) continue;

                var column = columns.FirstOrDefault(q => q.Name == pi.Name);
                if (column == null) continue;

                var val = pi.GetValue(item);
                if (val == null) continue;
                if (column.CustormConvert != null)
                {
                    var customConvert = column.CustormConvert.Item1.Invoke(column.CustormConvert.Item2, [val]);
                    newRow[column.Title] = customConvert;
                    continue;
                }
                if (pi.PropertyType.IsNullable())
                {
                    var propType = Nullable.GetUnderlyingType(pi.PropertyType);
                    if (propType == null) continue;
                    if (val is bool boolVal)
                    {
                        newRow[column.Title] = column.BoolValues != null && column.BoolValues.Count == 2
                            ? boolVal ? column.BoolValues[0] : column.BoolValues[1]
                            : boolVal ? "是" : "否";
                    }
                    else if (propType.IsEnum)
                    {
                        var enumVal = val as Enum;
                        if (enumVal != null)
                        {
                            var indexValue = (int)val;
                            if (column.EnumValues != null && column.EnumValues.Count > 0 && column.EnumValues.Count > indexValue)
                            {
                                newRow[column.Title] = column.EnumValues[indexValue];
                            }
                            else
                            {
                                var enumValue = string.Empty;
                                try
                                {
                                    enumValue = EnumUtil.GetDescription(enumVal);
                                }
                                catch
                                {
                                    enumValue = enumVal.ToString();
                                }
                                if (enumValue.IsNotNullOrEmpty())
                                {
                                    newRow[column.Title] = enumValue;
                                }
                                else
                                {
                                    newRow[column.Title] = val;
                                }
                            }
                        }
                        else
                        {
                            newRow[column.Title] = string.Empty;
                        }
                    }
                    else if (val is DateTime dt2)
                    {
                        newRow[column.Title] = dt2.ToString(column.DateTimeFormat);
                    }
                    else
                    {
                        newRow[column.Title] = val;
                    }
                }
                else
                {
                    if (val is bool boolVal)
                    {
                        newRow[column.Title] = column.BoolValues != null && column.BoolValues.Count == 2
                            ? boolVal ? column.BoolValues[0] : column.BoolValues[1]
                            : boolVal ? "是" : "否";
                    }
                    else if (pi.PropertyType.IsEnum)
                    {
                        var enumVal = val as Enum;
                        if (enumVal != null)
                        {
                            var indexValue = (int)val;
                            if (column.EnumValues != null && column.EnumValues.Count > 0 && column.EnumValues.Count > indexValue)
                            {
                                newRow[column.Title] = column.EnumValues[indexValue];
                            }
                            else
                            {
                                var enumValue = string.Empty;
                                try
                                {
                                    enumValue = EnumUtil.GetDescription(enumVal);
                                }
                                catch
                                {
                                    enumValue = enumVal.ToString();
                                }
                                if (enumValue.IsNotNullOrEmpty())
                                {
                                    newRow[column.Title] = enumValue;
                                }
                                else
                                {
                                    newRow[column.Title] = val;
                                }
                            }
                        }
                        else if (val is DateTime dt2)
                        {
                            newRow[column.Title] = dt2.ToString(column.DateTimeFormat);
                        }
                        else
                        {
                            newRow[column.Title] = val;
                        }
                    }
                }
            }

            dt.Rows.Add(newRow);
        }

        return dt;
    }

    /// <summary>
    /// 获取属性信息
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>属性信息数组</returns>
    private static PropertyInfo[] GetProperties(Type type)
    {
        if (!_propertyCache.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            _propertyCache[type] = properties;
        }
        return properties;
    }

    /// <summary>
    /// 保存报告到文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public abstract void Save(string filePath);

    /// <summary>
    /// 保存报告到流
    /// </summary>
    /// <returns>报告流</returns>
    public abstract Stream SaveStream();
}
