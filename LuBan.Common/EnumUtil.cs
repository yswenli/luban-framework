/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： EnumUtil
*版本号： V1.0.0.0
*唯一标识：f5f2798a-6186-4510-b81e-ac70b0a49615
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/4/26 18:10:43
*描述：枚举工具类
*
*=====================================================================
*修改标记
*修改时间：2021/4/26 18:10:43
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：枚举工具类
*
*****************************************************************************/
namespace System;


/// <summary>
/// 枚举工具类
/// </summary>
public static class EnumUtil
{
    /// <summary>
    /// 获取枚举的所有值
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>枚举值数组</returns>
    public static TEnum[] GetValues<TEnum>() where TEnum : struct
    {
        return Enum
                .GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToArray();
    }

    /// <summary>
    /// 将字符串转换为枚举
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="strEnum">枚举字符串</param>
    /// <returns>枚举值</returns>
    public static TEnum ToEnum<TEnum>(string strEnum) where TEnum : struct, Enum
    {
        return (TEnum)Enum.Parse(typeof(TEnum), strEnum);
    }

    /// <summary>
    /// 获取枚举信息列表
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>枚举信息列表</returns>
    public static List<EnumInfo> GetEnumInfos<TEnum>() where TEnum : struct, Enum
    {
        return GetEnumInfos(typeof(TEnum));
    }

    /// <summary>
    /// 获取枚举信息列表
    /// </summary>
    /// <param name="enType">枚举类型</param>
    /// <returns>枚举信息列表</returns>
    public static List<EnumInfo> GetEnumInfos(Type enType)
    {
        var enmuNames = Enum.GetNames(enType);
        var listRtn = new List<EnumInfo>();
        foreach (var enmuName in enmuNames)
        {
            var tempEnum = Enum.Parse(enType, enmuName);
            var dto = new EnumInfo();
            dto.Index = (int)tempEnum;
            dto.Text = enmuName;
            dto.Description = GetDescription((Enum)tempEnum);
            listRtn.Add(dto);
        }
        return listRtn;
    }

    /// <summary>
    /// 获取具有特定属性的枚举信息列表
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="typeAttr">属性类型</param>
    /// <returns>枚举信息列表</returns>
    public static List<EnumInfo> GetEnumInfos<TEnum>(Type typeAttr) where TEnum : struct, Enum
    {
        var enType = typeof(TEnum);
        var enmuNames = Enum.GetNames(enType);
        var listRtn = new List<EnumInfo>();
        foreach (var enmuName in enmuNames)
        {
            var tempEnum = Enum.Parse(enType, enmuName);
            var member = enType.GetMember(tempEnum?.ToString() ?? "");
            if (member != null && member.Length > 0)
            {
                if (member[0].GetCustomAttribute(typeAttr) != null)
                {
                    var dto = new EnumInfo();
                    dto.Index = Convert.ToInt32(tempEnum);
                    dto.Text = enmuName;
                    if (tempEnum != null)
                    {
                        var enumObj = (Enum)tempEnum;
                        if (enumObj != null)
                            dto.Description = GetDescription(enumObj);
                    }
                    listRtn.Add(dto);
                }
            }
        }
        return listRtn;
    }

    /// <summary>
    /// 获取枚举名称列表
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>枚举名称列表</returns>
    public static List<string> GetNames<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetNames(typeof(TEnum)).ToList();
    }

    /// <summary>
    /// 获取具有特定属性的枚举名称列表
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <typeparam name="Attr">属性类型</typeparam>
    /// <returns>枚举名称列表</returns>
    public static List<string> GetNames<TEnum, Attr>()
        where TEnum : struct, Enum
        where Attr : Attribute
    {
        return [.. GetEnums<TEnum, Attr>().Select(e => e.ToString())];
    }

    /// <summary>
    /// 获取具有特定属性的枚举值列表
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <typeparam name="Attr">属性类型</typeparam>
    /// <returns>枚举值列表</returns>
    public static List<TEnum> GetEnums<TEnum, Attr>()
        where TEnum : struct, Enum
        where Attr : Attribute
    {
        var names = Enum.GetNames(typeof(TEnum)).ToList();
        var list = names.Select(c => StringToEnum<TEnum>(names, c)).Where(t => t.GetType().GetMember(t.ToString()).First().GetCustomAttribute(typeof(Attr)) != null).ToList();
        return list;
    }

    /// <summary>
    /// 获取枚举值列表
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>枚举值列表</returns>
    public static List<TEnum> GetEnums<TEnum>() where TEnum : struct, Enum
    {
        var names = Enum.GetNames(typeof(TEnum)).ToList();
        var list = names.Select(c => StringToEnum<TEnum>(names, c)).ToList();
        return list;
    }

    /// <summary>
    /// 将字符串转换为枚举
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="str">枚举字符串</param>
    /// <returns>枚举值</returns>
    public static TEnum? StringToEnum<TEnum>(string str) where TEnum : struct, Enum
    {
        return StringToEnum<TEnum>(GetNames<TEnum>(), str);
    }

    /// <summary>
    /// 将字符串转换为枚举
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="enumNames">枚举名称列表</param>
    /// <param name="str">枚举字符串</param>
    /// <returns>枚举值</returns>
    public static TEnum StringToEnum<TEnum>(List<string> enumNames, string str) where TEnum : struct, Enum
    {
        if (enumNames.Exists(c => string.Equals(c, str, StringComparison.OrdinalIgnoreCase)))
            return (TEnum)Enum.Parse(typeof(TEnum), str, true);
        else
            throw new Exception("将字符串转换为枚举失败");
    }

    /// <summary>
    /// 将整数转换为枚举
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="iValue">整数值</param>
    /// <returns>枚举值</returns>
    public static TEnum? IntToEnum<TEnum>(int iValue) where TEnum : struct, Enum
    {
        if (Enum.IsDefined(typeof(TEnum), iValue))
            return (TEnum)Enum.ToObject(typeof(TEnum), iValue);
        else
            return null;
    }

    /// <summary>
    /// 获取枚举描述
    /// </summary>
    /// <param name="value">枚举值</param>
    /// <returns>枚举描述</returns>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();
        var attrs = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        if (attrs == null) return value.ToString();
        return attrs is not DescriptionAttribute attribute ? value.ToString() : attribute.Description;
    }

    /// <summary>
    /// 获取枚举描述列表
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static List<string> GetDescriptions(this Type enumType)
    {
        var result = new List<string>();
        foreach (var field in enumType.GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                is DescriptionAttribute attr)
            {
                result.Add(attr.Description);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取枚举描述列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<string> GetDescriptions<T>() where T : Enum
    {
        var result = new List<string>();
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                is DescriptionAttribute attr)
            {
                result.Add(attr.Description);
            }
        }
        return result;
    }

    /// <summary>
    /// 将枚举转换为字典
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>枚举字典</returns>
    public static Dictionary<string, int> ToDictionary<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(StringComparison)).Cast<StringComparison>().ToDictionary(t => t.ToString(), t => (int)t);
    }

    /// <summary>
    /// 将枚举转换为字典
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <returns>枚举字典</returns>
    public static Dictionary<string, string> ToDictionary2<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToDictionary(t => t.ToString(), t => t.ToString());
    }



    private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> _descriptionCache = new();
    /// <summary>
    /// 根据描述DescriptionAttribute获取枚举
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="description"></param>
    /// <returns></returns>
    public static TEnum? GetEnumByDescription<TEnum>(string description) where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        if (enumType == null) return null;
        if (!_descriptionCache.TryGetValue(enumType, out var descriptionMap))
        {
            var ps = enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static)?
                .Where(field => Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute);
            if (ps == null) return null;
            descriptionMap = new Dictionary<string, object>();
            foreach (var field in ps)
            {
                if (field == null) continue;
                var k = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
                if (k.IsNullOrEmpty()) continue;
                var v = field.GetValue(null);
                if (v == null) continue;
                descriptionMap.TryAdd(k, v);
            }
            _descriptionCache[enumType] = descriptionMap;
        }
        if (descriptionMap.TryGetValue(description, out var value))
        {
            return (TEnum)value;
        }
        return null;
    }
}
