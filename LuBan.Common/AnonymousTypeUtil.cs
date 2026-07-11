/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： AnonymousTypeUtil
*版本号： V1.0.0.0
*唯一标识：46c35ebe-02dd-483a-8fb8-dd632d14952b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/26 17:52:32
*描述：匿名类创建器
*
*=================================================
*修改标记
*修改时间：2025/11/26 17:52:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：匿名类创建器
*
*****************************************************************************/
namespace LuBan.Common;


/// <summary>
/// 匿名类创建器
/// </summary>
public static class AnonymousTypeUtil
{
    /// <summary>
    /// 创建匿名类
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static Type CreateType(PropertyInfo[] properties)
    {
        // 创建动态程序集
        var assemblyName = new AssemblyName("AnonymousTypeAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("AnonymousTypeModule");

        // 创建匿名类
        var typeBuilder = moduleBuilder.DefineType("AnonymousType", TypeAttributes.Public | TypeAttributes.Class);

        // 添加属性
        foreach (var property in properties)
        {
            typeBuilder.DefineField(property.Name, property.PropertyType, FieldAttributes.Public);
        }

        // 创建匿名类
        var anonymousType = typeBuilder.CreateType();

        return anonymousType;
    }


    /// <summary>
    /// 创建匿名类
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Type CreateAnonymousType(ExpandoObject obj)
    {
        // 获取动态对象的属性列表
        var properties = obj.GetType().GetProperties();

        // 创建属性列表
        var propertyList = new List<PropertyInfo>();

        foreach (var property in properties)
        {
            if (property == null) continue;
            // 获取属性的类型
            var propertyType = property.PropertyType;

            // 如果属性的类型是动态对象，则递归创建匿名类
            if (propertyType == typeof(ExpandoObject))
            {
                var eo = property.GetValue(obj) as ExpandoObject;
                if (eo != null)
                    propertyType = CreateAnonymousType(eo);
            }

            // 创建属性信息
            var propertyInfo = new { Name = property.Name, Type = propertyType };

            // 添加属性信息到属性列表
            var pgg = propertyInfo.GetType().GetProperty("Type");
            if (pgg != null)
                propertyList.Add(pgg);
        }

        // 创建匿名类
        var anonymousType = AnonymousTypeUtil.CreateType([.. propertyList]);

        return anonymousType;
    }

    /// <summary>
    /// 将动态对象转换为匿名类实例
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="anonymousType"></param>
    /// <returns></returns>
    public static object? ConvertToAnonymousType(ExpandoObject? obj, Type anonymousType)
    {
        if (obj == null) return null;

        // 创建匿名类实例
        var instance = Activator.CreateInstance(anonymousType);

        // 获取匿名类的属性列表
        var properties = anonymousType.GetProperties();

        foreach (var property in properties)
        {
            // 获取动态对象的属性值
            var value = obj.GetType()?.GetProperty(property.Name)?.GetValue(obj);

            // 如果属性的类型是动态对象，则递归转换为匿名类实例
            if (property.PropertyType == typeof(object))
            {
                var propertyValue = ConvertToAnonymousType(value as ExpandoObject, property.PropertyType);
                property.SetValue(instance, propertyValue);
            }
            else
            {
                property.SetValue(instance, value);
            }
        }

        return instance;
    }

    /// <summary>
    /// 判断类型是否实现某个泛型
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="generic">泛型类型</param>
    /// <returns>bool</returns>
    public static bool HasImplementedRawGeneric(this Type type, Type generic)
    {
        // 检查接口类型
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType) return true;

        // 检查类型
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            if (type.BaseType == null) break;
            type = type.BaseType;
        }

        return false;

        // 判断逻辑
        bool IsTheRawGenericType(Type type) => generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
    }

    /// <summary>
    /// 获取方法真实返回类型
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static Type GetRealReturnType(this MethodInfo method)
    {
        // 判断是否是异步方法
        var isAsyncMethod = method.IsAsync();

        // 获取类型返回值并处理 Task 和 Task<T> 类型返回值
        var returnType = method.ReturnType;
        return isAsyncMethod ? (returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void)) : returnType;
    }

    /// <summary>
    /// 判断方法是否是异步
    /// </summary>
    /// <param name="method">方法</param>
    /// <returns></returns>
    public static bool IsAsync(this MethodInfo method)
    {
        var fullName = typeof(Task).FullName;
        if (fullName.IsNullOrEmpty()) return false;
        return method.GetCustomAttribute<AsyncMethodBuilderAttribute>() != null
            || fullName.IsNotNullOrEmpty()
            || method.ReturnType.ToString().StartsWith(fullName);
    }

    #region 属性


    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo? GetProperty(this object obj, string propertyName)
    {
        if (obj == null) return null;
        var propertyInfo = MemoryCache.Instance.Get<PropertyInfo>("ReflectionUtil.GetProperty");
        if (propertyInfo == null)
        {
            propertyInfo = obj.GetType().GetProperty(propertyName);
            MemoryCache.Instance.Set($"{CacheConst.KeySystem}reflectionUtil:getProperty", propertyInfo);
        }
        return propertyInfo;
    }

    static ConcurrentDictionary<string, List<string>> _propertities = new();

    /// <summary>
    /// 获取属性名称列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string>? GetPropertityNames(this object obj)
    {
        if (obj == null) return null;
        var properties = obj.GetProperties();
        if (properties == null || properties.Count < 1) return null;
        List<string> result = new();
        foreach (var item in properties)
        {
            result.Add(item.Name);
        }
        return result;
    }
    /// <summary>
    /// 获取属性名称列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<string> GetPropertityNames(this Type type)
    {
        var properties = type.GetProperties();
        if (properties == null || properties.Length < 1) return [];
        List<string> result = new();
        foreach (var item in properties)
        {
            result.Add(item.Name);
        }
        return result;
    }

    /// <summary>
    /// 获取属性名称列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<string>? GetPropertityNamesFromCache(this object obj)
    {
        var type = obj.GetType();
        var fullName = type.FullName;
        if (fullName.IsNullOrEmpty()) return null;
        var list = obj.GetPropertityNames();
        if (list == null) return null;
        return _propertities.GetOrAdd(fullName, (k) => list);
    }

    /// <summary>
    /// 获取属性名称列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<string>? GetPropertityNamesFromCache(this Type type)
    {
        var fullName = type.FullName;
        if (fullName.IsNullOrEmpty()) return null;
        return _propertities.GetOrAdd(fullName, (k) => type.GetPropertityNames());
    }




    #endregion

}
