/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Models
*文件名： TitleValue
*版本号： V1.0.0.0
*唯一标识：158c79f6-3527-4ddc-94cf-511b7453e392
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/6 11:44:10
*描述：
*
*=================================================
*修改标记
*修改时间：2025/3/6 11:44:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace System;


#region TitleValue 

/// <summary>
/// 获取指定字段数据，例如：姓名、性别、年龄、身份证号等等
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class PropertyTitle : Attribute
{
    /// <summary>
    /// 获取指定字段数据，例如：姓名、性别、年龄、身份证号等等
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 获取指定字段数据，例如：姓名、性别、年龄、身份证号等等
    /// </summary>
    /// <param name="queTitle"></param>
    public PropertyTitle(string queTitle)
    {
        Title = queTitle;
    }
}

/// <summary>
/// 标题/值
/// </summary>
public sealed class TitleValue
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 标题/值
    /// </summary>
    public TitleValue()
    {

    }
    /// <summary>
    /// 标题/值
    /// </summary>
    /// <param name="title"></param>
    /// <param name="value"></param>
    public TitleValue(string title, string value)
    {
        Title = title;
        Value = value;
    }
}

/// <summary>
/// 标题/值集合
/// </summary>
public sealed class TitleValueCollection : List<TitleValue>
{
    /// <summary>
    /// 全部标题
    /// </summary>
    public IEnumerable<string> AllTitles
    {
        get
        {
            return this.Select(t => t.Title).ToList();
        }
    }

    /// <summary>
    /// 指定值
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public string? this[string title]
    {
        get
        {
            return this.FirstOrDefault(t => t.Title == title)?.Value;
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="title"></param>
    /// <param name="value"></param>
    public void Add(string title, string value)
    {
        this.Add(new TitleValue() { Title = title, Value = value });
    }

    /// <summary>
    /// 是否包含指定标题
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public bool ContainsTitle(string title)
    {
        return this.Any(t => t.Title == title);
    }



}

/// <summary>
/// 指定字段数据，例如：姓名、性别、年龄、身份证号等等转换器，
/// 配合 PropertyNameAttribute 使用
/// </summary>
public static class TitleValueConvertor
{
    /// <summary>
    /// 从列表转换成 TitleValueCollection
    /// </summary>
    /// <param name="collection"></param>
    public static TitleValueCollection ConvertToTitleValueCollection(this IEnumerable<TitleValue> collection)
    {
        var result = new TitleValueCollection();
        if (collection != null && collection.Any())
        {
            result.AddRange(collection);
        }
        return result;
    }

    /// <summary>
    /// 指定字段数据转换
    /// 配合 PropertyTitle 使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="matchType"></param>
    /// <returns></returns>
    public static T? ConvertTo<T>(this TitleValueCollection collection, EnumConvertMatchType matchType = EnumConvertMatchType.ExactlyMatch) 
        where T : class
    {
        if (collection == null || collection.Count < 1) return default;
        var t = Activator.CreateInstance<T>();
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (properties != null && properties.Length > 0)
        {
            foreach (var property in properties)
            {
                var attrs = property.GetCustomAttributes<PropertyTitle>();
                if (attrs == null || attrs.Count < 1) continue;
                foreach (var name in collection.AllTitles)
                {
                    if (name.IsNullOrEmpty()) continue;
                    try
                    {
                        var val = collection[name] ?? "";
                        if (val.IsNullOrEmpty())
                        {
                            continue;
                        }
                        var hasAttr = false;
                        switch (matchType)
                        {
                            case EnumConvertMatchType.ExactlyMatch:
                                if (attrs.Any(q => q.Title == name))
                                {
                                    hasAttr = true;
                                }
                                break;
                            case EnumConvertMatchType.IgnoreCase:
                                if (attrs.Any(q => q.Title.Equals(name, StringComparison.OrdinalIgnoreCase)))
                                {
                                    hasAttr = true;
                                }
                                break;
                            case EnumConvertMatchType.Contain:
                                if (attrs.Any(q => q.Title.Contains(name)))
                                {
                                    hasAttr = true;
                                }
                                break;
                            case EnumConvertMatchType.ContainAndIgnoreCase:
                                if (attrs.Any(q => q.Title.Contains(name, StringComparison.OrdinalIgnoreCase)))
                                {
                                    hasAttr = true;
                                }
                                break;
                        }
                        if (!hasAttr)
                        {
                            continue;
                        }

                        if (property.PropertyType.IsNullable())
                        {
                            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                            if (underlyingType == null)
                            {
                                continue;
                            }
                            if (underlyingType == typeof(bool))
                            {
                                if (val == "是" || val == "1")
                                {
                                    ReflectionUtil.SetPropertyValue(t, property, true);
                                }
                                else
                                {
                                    ReflectionUtil.SetPropertyValue(t, property, false);
                                }
                                continue;
                            }
                            var convertedValue = Convert.ChangeType(val, underlyingType);
                            ReflectionUtil.SetPropertyValue(t, property, convertedValue);
                        }
                        else
                        {
                            if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                            {
                                if (val == "是" || val == "1")
                                {
                                    ReflectionUtil.SetPropertyValue(t, property, true);
                                }
                                else
                                {
                                    ReflectionUtil.SetPropertyValue(t, property, false);
                                }
                                continue;
                            }
                            var convertedValue = Convert.ChangeType(val, property.PropertyType);
                            ReflectionUtil.SetPropertyValue(t, property, convertedValue);
                        }
                    }
                    catch { }
                }
            }
        }
        return t;
    }

    /// <summary>
    /// 指定字段数据转换
    /// 配合 PropertyTitle 使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="matchType"></param>
    /// <returns></returns>
    public static T? ConvertTo<T>(this IEnumerable<TitleValue> list, EnumConvertMatchType matchType = EnumConvertMatchType.ExactlyMatch) where T : class, new()
    {
        var collection = list.ConvertToTitleValueCollection();
        return collection.ConvertTo<T>(matchType);
    }
    /// <summary>
    /// 指定字段数据转换
    /// 配合 PropertyTitle 使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="matchType"></param>
    /// <returns></returns>
    public static List<T>? ConvertToList<T>(this List<TitleValueCollection> list, EnumConvertMatchType matchType = EnumConvertMatchType.IgnoreCase) where T : class, new()
    {
        if (list == null || list.Count == 0) return null;
        var tList = new List<T>();
        foreach (var item in list)
        {
            var t = item.ConvertTo<T>(matchType);
            if (t != null)
            {
                tList.Add(t);
            }
        }
        return tList;
    }

    /// <summary>
    /// 指定字段数据转换
    /// 配合 PropertyTitle 使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="matchType"></param>
    /// <returns></returns>
    public static List<T>? ConvertToList<T>(this IEnumerable<IEnumerable<TitleValue>> list, EnumConvertMatchType matchType = EnumConvertMatchType.IgnoreCase) where T : class, new()
    {
        var data = new List<TitleValueCollection>();
        foreach (var item in list)
        {
            data.Add(item.ConvertToTitleValueCollection());
        }
        return data.ConvertToList<T>(matchType);
    }

    /// <summary>
    /// 读取标题值集合
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static TitleValueCollection ToTitleValues(this object obj)
    {
        var result = new TitleValueCollection();
        obj.GetProperties().ForEach((p) =>
        {
            var titlValue = new TitleValue()
            {
                Title = p.Name,
                Value = p.GetValue(obj)?.ToString() ?? ""
            };
            result.Add(titlValue);
        });
        return result;
    }

    /// <summary>
    /// 读取标题值集合
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IEnumerable<TitleValueCollection> ToTitleValueCollections(this IEnumerable<object> list)
    {
        foreach (var item in list)
        {
            yield return item.ToTitleValues();
        }
    }

    /// <summary>
    /// 读取标题值集合,
    /// 配合 PropertyTitle 使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static TitleValueCollection ToTitleValuesByAttributes<T>(this T obj)
        where T : class, new()
    {
        var result = new TitleValueCollection();
        obj.GetProperties().ForEach((p) =>
        {
            var attrs = p.GetCustomAttributes<PropertyTitle>();
            if (attrs != null && attrs.Any())
            {
                foreach (var attr in attrs)
                {
                    var titlValue = new TitleValue()
                    {
                        Title = attr.Title,
                        Value = p.GetValue(obj)?.ToString() ?? ""
                    };
                    result.Add(titlValue);
                }
            }

        });
        return result;
    }

    /// <summary>
    /// 读取标题值集合,
    /// 配合 PropertyTitle 使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IEnumerable<TitleValueCollection> ToTitleValueCollectionsByAttributes<T>(this IEnumerable<T> list)
        where T : class, new()
    {
        foreach (var item in list)
        {
            yield return item.ToTitleValuesByAttributes<T>();
        }
    }

}

#endregion
