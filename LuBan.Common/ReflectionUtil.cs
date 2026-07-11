/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： ReflectionUtil
*版本号： V1.0.0.0
*唯一标识：5be32297-1fc7-41b5-9218-b573f2ab272f
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/11 13:29:17
*描述：反射相关工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/11 13:29:17
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：反射相关工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 反射相关工具类
/// </summary>
public static class ReflectionUtil
{
    /// <summary>
    /// 创建实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Create<T>() where T : class, new()
    {
        return Activator.CreateInstance<T>();
    }

    /// <summary>
    /// 创建实例
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static dynamic? Create(this Type type)
    {
        return Activator.CreateInstance(type);
    }


    /// <summary>
    /// 创建某实例泛型参数的实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static T? CreateOnGenericType<T>(this object obj, int index = 0)
    {
        if (obj == null) return default;
        var genericType = obj.GetType().GenericTypeArguments[index];
        var result = Activator.CreateInstance(genericType);
        if (result == null) return default;
        return (T)result;
    }

    /// <summary>
    /// 判断是否是可空值类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNullableValueType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// 验证字段是否支持 null 比较（引用类型/可空值类型可支持，非可空值类型不支持）
    /// </summary>
    /// <param name="property"></param>
    /// <exception cref="InvalidCastException"></exception>
    public static void ValidateNullComparison(PropertyInfo property)
    {
        Type propertyType = property.PropertyType;
        // 非可空值类型（如 int、DateTime）不能与 null 比较
        if (propertyType.IsValueType && !propertyType.IsNullableValueType())
        {
            throw new InvalidCastException(
                $"字段 {property.Name} 是不可空值类型（{propertyType.Name}），不能使用 null 作为筛选值");
        }
    }


    /// <summary>
    /// 创建“元素是否等于val”的委托（优化性能，避免频繁反射）
    /// </summary>
    /// <param name="elementType"></param>
    /// <param name="val"></param>
    /// <param name="isValNull"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Func<object, bool> CreateEqualityChecker(Type elementType, dynamic val, bool isValNull)
    {
        if (isValNull)
        {
            // val为null：判断元素是否为null（值类型元素已在前面校验过，不会进入此分支）
            return element => element == null;
        }
        else
        {
            // val非null：获取val的实际类型，处理类型转换（如int→long、string→object）
            Type valType = val.GetType();
            object typedVal = val;

            // 若val类型与元素类型不匹配，尝试显式转换（如string→int、int→decimal）
            if (valType != elementType && !valType.IsAssignableTo(elementType))
            {
                try
                {
                    typedVal = Convert.ChangeType(val, elementType);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"无法将val类型 {valType.FullName}（值：{val}）转换为集合元素类型 {elementType.FullName}",
                        nameof(val),
                        ex);
                }
            }

            // 情况1：元素类型重写了Equals方法（如string、int、自定义类）→ 调用元素的Equals
            MethodInfo? equalsMethod = elementType.GetMethod(
                nameof(Equals),
                [elementType]); // 匹配参数为elementType的Equals方法（非object参数）
            if (equalsMethod != null && equalsMethod != null && equalsMethod.DeclaringType != null && !equalsMethod.DeclaringType.Equals(typeof(object)))
            {
                return element =>
                {
                    if (element == null || equalsMethod == null) return false; // 元素为null，val非null → 不相等
                    return (bool)(equalsMethod.Invoke(element, [typedVal]) ?? false);
                };
            }
            return element => object.Equals(element, typedVal);
        }
    }

    #region 从type的的全名称中获取对象

    static ConcurrentDictionary<string, dynamic> _instanceCache = [];
    static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _propertyCache = new();

    public static PropertyInfo? GetCachedProperty(this Type type, string propertyName)
    {
        return _propertyCache.GetOrAdd((type, propertyName),
            key => key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance));
    }

    /// <summary>
    /// 从type的的全名称中获取对象
    /// </summary>
    /// <param name="assemblyQualifiedName"></param>
    /// <returns></returns>
    public static dynamic GetInstanceByAssemblyQualifiedName(this string assemblyQualifiedName)
    {
        return _instanceCache.GetOrAdd(assemblyQualifiedName, (k) =>
        {
            var index = assemblyQualifiedName.IndexOf(",");
            var typeName = assemblyQualifiedName.Substring(0, index);
            var assemblyStr = assemblyQualifiedName.Substring(index + 1).Trim();
            var obj = Assembly.Load(assemblyStr).CreateInstance(typeName);
            return obj == null ? throw new Exception($"无法创建实例:{assemblyQualifiedName}") : (dynamic)obj;
        });
    }

    /// <summary>
    /// 从type的的全名称中获取对象并执行指定的方法
    /// </summary>
    /// <param name="assemblyQualifiedName"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static dynamic ExecuteByAssemblyQualifiedName(this string assemblyQualifiedName, string methodName, params object[] args)
    {
        var instance = assemblyQualifiedName.GetInstanceByAssemblyQualifiedName();
        var type = instance.GetType();
        return FastILUtil.MethodInvoke(type, methodName, instance, args);
    }


    #endregion

    /// <summary>
    /// 从名称中创建List
    /// </summary>
    /// <param name="namespaceStr"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IList CreateList(string namespaceStr, string className)
    {
        var model = Assembly.GetExecutingAssembly().CreateInstance(string.Join(".", new object[] { namespaceStr, className }));
        if (model == null || model.GetType() == null) throw new Exception($"无法创建实例:{namespaceStr}.{className}");
        var ilist = Activator.CreateInstance(typeof(List<>).MakeGenericType([model.GetType()])) ?? null;
        if (ilist is IList list && list != null) return list;
        throw new Exception($"无法创建实例:{namespaceStr}.{className}");
    }

    /// <summary>
    /// 从类型中创建List
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IList CreateList(this Type type)
    {
        var ilist = Activator.CreateInstance(typeof(List<>).MakeGenericType([type])) ?? null;
        if (ilist is IList list && list != null) return list;
        throw new Exception($"无法创建实例:{type}");
    }

    /// <summary>
    /// 判断对象是否为匿名类型
    /// </summary>
    public static bool IsAnonymousType(this object obj)
    {
        if (obj == null)
            return false;

        Type type = obj.GetType();

        // 匿名类型的关键特征：
        // 1. 类型名称以 <>f__AnonymousType 开头
        // 2. 标记为编译器生成（[CompilerGenerated]）
        // 3. 是密封类
        return type.IsClass && type.IsSealed
            && type.Name.StartsWith("<>f__AnonymousType");
    }

    /// <summary>
    /// 获取类的属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static PropertyInfo[] GetPropertities<T>() where T : class, new()
    {
        var type = typeof(T);

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }


    /// <summary>
    /// 检查是否存在attribute
    /// </summary>
    /// <typeparam name="Attr"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool HasAttribute<Attr>(this Type type) where Attr : Attribute
    {
        return type.IsDefined(typeof(Attr), true);
    }

    /// <summary>
    /// 判断对象是否存在attribute
    /// </summary>
    /// <param name="type"></param>
    /// <param name="attrName"></param>
    /// <returns></returns>
    public static bool HasAttribute(this Type? type, string attrName)
    {
        if (type == null || attrName.IsNullOrEmpty()) return false;
        foreach (var attr in Attribute.GetCustomAttributes(type, inherit: true))
        {
            var aType = attr.GetType();
            if (string.Equals(aType.Name, attrName, StringComparison.OrdinalIgnoreCase)
                || (aType.FullName != null && string.Equals(aType.FullName, attrName, StringComparison.OrdinalIgnoreCase)))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 对象某个属性是否存在attribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public static bool HasAttribute<T>(this PropertyInfo property) where T : Attribute
    {
        return property.IsDefined(typeof(T), true);
    }

    /// <summary>
    /// 对象某个方法是否有attribute标记
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static bool MethodHasAttribute<T>(this object obj, string methodName) where T : Attribute
    {
        if (obj == null || methodName.IsNullOrEmpty()) return false;
        var method = obj.GetType().GetMethod(methodName);
        if (method == null) return false;
        return method.IsDefined(typeof(T), false);
    }

    /// <summary>
    /// 获取对象某个方法自定义AttributeList
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static List<string?>? GetMethodAttributes(this object obj, string methodName)
        => obj?.GetType().GetMethod(methodName)?.CustomAttributes?.Select(q => q.AttributeType.FullName)?.ToList();


    /// <summary>
    /// 对象某个方法的参数是否有attribute标记
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static bool MethodParametersHasAttribute<T>(this object obj, string methodName, out List<string> parameters) where T : Attribute
    {
        parameters = [];
        if (obj == null || methodName.IsNullOrEmpty()) return false;
        var method = obj.GetType().GetMethod(methodName);
        if (method == null) return false;
        var ps = method.GetParameters();
        if (ps == null || ps.Length < 1) return false;
        var attributeType = typeof(T);
        foreach (var p in ps)
        {
            if (p.Name.IsNotNullOrEmpty() && p.IsDefined(attributeType, false))
                parameters.Add(p.Name);
        }
        parameters = parameters.Distinct().ToList();
        return parameters.Count > 0;
    }

    /// <summary>
    /// 获取对象某个方法的参数的自定义AttributeList
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static List<string?>? GetMethodParametersAttributes(this object obj, string methodName) => obj?.GetType().GetMethod(methodName)?.GetParameters()?.SelectMany(q => q.CustomAttributes)?.Select(q => q.AttributeType.FullName)?.ToList() ?? null;



    /// <summary>
    /// 根据指定的attr获取当前类别和Attr
    /// </summary>
    /// <typeparam name="TAttr"></typeparam>
    /// <param name="callBack"></param>
    /// <exception cref="Exception"></exception>
    public static void GetTypesByAttribute<TAttr>(Action<KeyValuePair<Type, TAttr>> callBack) where TAttr : Attribute
    {
        var assemblies = RuntimeUtil.GetAssemblies();
        if (assemblies != null)
        {
            var types = assemblies.DynamicLoadTypes<TAttr>()
            .Where(t => t.HasAttribute<TAttr>())
            .ToList();
            if (types == null || types.Count < 1) throw new Exception($"找不到标记Attribute为{typeof(TAttr).Name}的类");
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<TAttr>();
                if (attr == null) continue;
                callBack.Invoke(new KeyValuePair<Type, TAttr>(type, attr));
            }
        }
    }

    /// <summary>
    /// 获取方法上指定的两个Attribute，且某个Attribute生效
    /// </summary>
    /// <typeparam name="Attr1"></typeparam>
    /// <typeparam name="Attr2"></typeparam>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    private static bool AttributeHasEffectCore<Attr1, Attr2>(ICustomAttributeProvider provider)
        where Attr1 : Attribute where Attr2 : Attribute
    {
        var attrs = provider.GetCustomAttributes(inherit: true);
        if (attrs == null || attrs.Length == 0) return false;
        int index1 = -1, index2 = -1;
        for (int i = 0; i < attrs.Length; i++)
        {
            if (attrs[i] is Attr1) index1 = i;
            if (attrs[i] is Attr2) index2 = i;
        }
        return index1 > -1 && (index2 < 0 || index1 > index2);
    }

    public static bool AttributeHasEffect<Attr1, Attr2>(this MethodInfo methodInfo)
        where Attr1 : Attribute where Attr2 : Attribute
    {
        return AttributeHasEffectCore<Attr1, Attr2>(methodInfo);
    }

    public static bool AttributeHasEffect<Attr1, Attr2>(this Type instanceType)
        where Attr1 : Attribute where Attr2 : Attribute
    {
        return AttributeHasEffectCore<Attr1, Attr2>(instanceType);
    }


    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    public static void SetPropertyValue(this object obj, PropertyInfo property, object value)
    {
        if (obj == null) return;
        if (property.CanWrite)
        {
            FastILUtil.SetProperty(obj.GetType(), property.Name, obj, value);
        }
    }

    /// <summary>
    /// 设置属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public static void SetPropertyValue(this object obj, string propertyName, object value)
    {
        if (obj == null) return;
        var propertiy = obj.GetProperty(propertyName);
        if (propertiy == null) return;
        obj.SetPropertyValue(propertiy, value);
    }

    /// <summary>
    /// 获取属性列表
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static List<PropertyInfo> GetProperties([DisallowNull] this object model)
    {
        return model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
    }

    /// <summary>
    /// 获取属性列表
    /// </summary>
    /// <param name="type"></param>
    /// <param name="bindingFlags"></param>
    /// <returns></returns>
    public static List<PropertyInfo> GetProperties([DisallowNull] this Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
    {
        try
        {
            return type.GetProperties(bindingFlags).ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <returns></returns>
    public static List<PropertyInfo> GetProperties<T>([DisallowNull] this T model) where T : class, new()
    {
        return model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
    }

    /// <summary>
    /// 获取自定义attribute
    /// </summary>
    /// <param name="model"></param>
    /// <param name="attrType"></param>
    /// <returns></returns>
    public static List<Attribute> GetAttributes([DisallowNull] this object model, Type? attrType)
    {
        if (attrType == null)
            return model.GetType().GetCustomAttributes(inherit: true).Cast<Attribute>().ToList();
        return model.GetType().GetCustomAttributes(attrType, inherit: true).Cast<Attribute>().ToList();
    }

    /// <summary>
    /// 获取自定义attribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="attrType"></param>
    /// <returns></returns>
    public static List<Attribute> GetCustomAttributes<T>([DisallowNull] this Type type, Type? attrType) where T : class, new()
    {
        if (attrType == null)
            return Attribute.GetCustomAttributes(type, inherit: true).ToList();
        return Attribute.GetCustomAttributes(type, attrType, inherit: true).ToList();
    }

    /// <summary>
    /// 获取自定义attribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="attrType"></param>
    /// <returns></returns>
    public static List<Attribute> GetCustomAttributes<T>([DisallowNull] this T model, Type? attrType) where T : class, new()
    {
        if (attrType == null)
            return model.GetType().GetCustomAttributes(inherit: true).Cast<Attribute>().ToList();
        return model.GetType().GetCustomAttributes(attrType, inherit: true).Cast<Attribute>().ToList();
    }

    /// <summary>
    /// 获取自定义attribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T? GetCustomAttribute<T>([DisallowNull] this Type type) where T : Attribute
    {
        return Attribute.GetCustomAttribute(type, typeof(T)) as T;
    }

    /// <summary>
    /// 获取自定义attribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T? GetCustomAttribute<T>([DisallowNull] this object obj) where T : Attribute
    {
        var attrs = obj.GetType().GetAttributes(null);
        if (attrs == null || attrs.Count < 1) return default;
        return attrs.FirstOrDefault(x => x.IsAssignableFrom<T>()) as T;
    }
    /// <summary>
    /// 获取类自定义attributes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static List<T>? GetCustomAttributes<T>([DisallowNull] this object obj) where T : Attribute
    {
        if (obj == null) return default;
        var attrs = obj.GetType().GetAttributes(typeof(T));
        if (attrs == null || attrs.Count < 1) return default;
        var list = new List<T>();
        foreach (var attr in attrs)
        {
            if (attr is T t)
            {
                list.Add(t);
            }
        }
        return list;
    }

    /// <summary>
    /// 获取属性自定义attributes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    /// <returns></returns>
    public static List<T>? GetCustomAttributes<T>([DisallowNull] this PropertyInfo property) where T : Attribute
    {
        var attrs = property.GetCustomAttributes(true);
        if (attrs == null || attrs.Length < 1) return default;
        var list = new List<T>();
        foreach (var attr in attrs)
        {
            if (attr is T t)
            {
                list.Add(t);
            }
        }
        return list;
    }



    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static object? GetPropertyValue(object obj, PropertyInfo property)
    {
        if (property.CanRead)
        {
            return FastILUtil.GetProperty(obj.GetType(), property.Name, obj);
        }
        return null;
    }

    /// <summary>
    /// 获取属性名称
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<string> GetPropertyNames<T>(this Type type)
       where T : Attribute
    {
        return type.GetProperties()
            .Where(x => x.IsDefined(typeof(T), false))
            .Select(x => x.Name)
            .ToList();
    }


    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object? ConvertValue(Type type, object value)
    {
        if (Convert.IsDBNull(value) || value == null)
        {
            return null;
        }

        if (type == null)
        {
            return null;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var unNullType = Nullable.GetUnderlyingType(type);
            if (unNullType == null) return null;
            type = unNullType;
        }

        Type valueType = value.GetType();

        if (type == valueType)
        {
            return value;
        }

        if (type.IsEnum)
        {
            try
            {
                if (value is string)
                {
                    return Enum.Parse(type, value.ToString() ?? "", true);
                }
                return Enum.ToObject(type, value);
            }
            catch
            {
                return null;
            }
        }

        if (type == typeof(Guid))
        {
            if (value is string strValue && Guid.TryParse(strValue, out Guid guidValue))
            {
                return guidValue;
            }
            return Guid.Empty;
        }

        if (type == typeof(DateTime))
        {
            if (value is string strValue && DateTime.TryParse(strValue, out DateTime dateTimeValue))
            {
                return dateTimeValue;
            }
            return null;
        }

        if (type == typeof(bool))
        {
            if (value is string strValue)
            {
                if (bool.TryParse(strValue, out bool boolValue))
                {
                    return boolValue;
                }
                if (int.TryParse(strValue, out int intValue))
                {
                    return intValue != 0;
                }
            }
            return Convert.ToBoolean(value);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType != null && value is Array array)
            {
                Array convertedArray = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    var val = array.GetValue(i);
                    if (val != null)
                        convertedArray.SetValue(ConvertValue(elementType, val), i);
                }
                return convertedArray;
            }
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type elementType = type.GetGenericArguments()[0];
            if (value is IEnumerable enumerable)
            {
                var list = Activator.CreateInstance(type) as IList;
                if (list == null) return null;
                foreach (var item in enumerable)
                {
                    list.Add(ConvertValue(elementType, item));
                }
                return list;
            }
        }

        try
        {
            return Convert.ChangeType(value, type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 是否是可空类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// 是否是字典
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsDictionary(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    /// <summary>
    /// 是否是列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsList(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    /// <summary>
    /// 是否是枚举
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsIEnumerable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }

    /// <summary>
    /// 是否是数组
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsArray(this Type type)
    {
        return type.IsArray;
    }


    /// <summary>
    /// 是否继承某个接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool HasInterface<T>(this Object obj)
    {
        return typeof(T).IsAssignableFrom(obj.GetType());
    }

    /// <summary>
    /// 是否含有接口
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="interfaceName"></param>
    /// <returns></returns>
    public static bool HasInterface(this Object obj, string interfaceName)
    {
        if (obj == null || interfaceName.IsNullOrEmpty()) return false;
        var type = obj.GetType();
        if (!type.IsClass || type.IsAbstract) return false;
        return type.GetInterfaces().Any(i =>
            string.Equals(i.Name, interfaceName, StringComparison.OrdinalIgnoreCase) ||
            (i.FullName != null && string.Equals(i.FullName, interfaceName, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// 是否继承某个接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool HasInterface<T>(this Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }

    /// <summary>
    /// 是否继承某个类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsAssignableFrom<T>(this Object obj) where T : class
    {
        var type = obj.GetType();
        if (type.IsClass)
        {
            return type.IsAssignableFrom(typeof(T));
        }
        return false;
    }

    /// <summary>
    /// 动态添加atrribute
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="attr"></param>
    public static void AddAttribute(this Object obj, Attribute attr)
    {
        TypeDescriptor.AddAttributes(obj, attr);
    }


    #region 将字符串格式化成指定的数据类型


    /// <summary>
    /// 将字符串格式化成指定的数据类型
    /// </summary>
    /// <param name="str"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Object? ConvertToType(this String str, Type type)
    {
        if (String.IsNullOrEmpty(str))
            return null;
        if (type == null)
            return str;
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType == null) return null;
            String[] strs = str.Split([';']);
            Array array = Array.CreateInstance(elementType, strs.Length);
            for (int i = 0, c = strs.Length; i < c; ++i)
            {
                array.SetValue(ConvertSimpleType(strs[i], elementType), i);
            }
            return array;
        }
        return ConvertSimpleType(str, type);
    }


    private static object? ConvertSimpleType(object value, Type destinationType)
    {
        object? returnValue;
        if ((value == null) || destinationType.IsInstanceOfType(value))
        {
            return value;
        }
        var str = value as string;
        if ((str != null) && (str.Length == 0))
        {
            return null;
        }
        TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
        bool flag = converter.CanConvertFrom(value.GetType());
        if (!flag)
        {
            converter = TypeDescriptor.GetConverter(value.GetType());
        }
        if (!flag && !converter.CanConvertTo(destinationType))
        {
            throw new InvalidOperationException("无法转换成类型：" + value.ToString() + "==>" + destinationType);
        }
        try
        {
            returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("类型转换出错：" + value.ToString() + "==>" + destinationType, e);
        }
        return returnValue;
    }
    #endregion

    /// <summary>
    /// 获取方法调用链,堆栈跟踪
    /// </summary>
    /// <param name="depth">方法的回溯深度</param>
    /// <returns></returns>
    public static Tuple<string, string, ParameterInfo[]>? GetCurrentMethodFullName(int depth = 2)
    {
        try
        {
            StackTrace st = new StackTrace();
            var frame = st.GetFrame(depth);
            var method = frame?.GetMethod();
            if (method == null) return null;
            var declaringType = method.DeclaringType;
            if (declaringType != null)
            {
                string className = declaringType.ToString();
                if (className == "AsyncMethodBuilderCore")
                    return GetCurrentMethodFullName(depth + 1);
                string methodName = method.Name;
                var args = method.GetParameters();
                return new Tuple<string, string, ParameterInfo[]>(className, methodName, args);
            }
        }
        catch
        {
        }
        return null;
    }


    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static dynamic? GetPropertyValue(this object obj, string propertyName)
    {
        if (obj == null) return null;
        if (propertyName.IsNullOrEmpty()) return null;
        var property = obj.GetType().GetCachedProperty(propertyName);
        if (property == null) return null;
        return GetPropertyValue(obj, property);
    }

    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="bindingFlags"></param>
    /// <returns></returns>
    public static dynamic? GetPropertyValue(this object obj, string propertyName, BindingFlags bindingFlags)
    {
        if (obj == null) return null;
        if (propertyName.IsNullOrEmpty()) return null;
        var property = obj.GetType().GetProperty(propertyName, bindingFlags);
        if (property == null) return null;
        return GetPropertyValue(obj, property);
    }

    /// <summary>
    /// 修复泛型类型注册类型问题
    /// （解决开放泛型名称格式异常、跨程序集类型标识不一致等问题，确保类型可正确查找和注册）
    /// </summary>
    /// <param name="type">待修复的类型（可能是开放泛型/闭合泛型）</param>
    /// <returns>修复后的有效 Type 对象，若无法修复则返回原类型</returns>
    /// <exception cref="ArgumentNullException">当输入 type 为 null 时抛出</exception>
    public static Type FixedGenericType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type), "待修复的类型不能为 null");
        if (!type.IsGenericType)
            return type;
        Type genericDefinition = type.GetGenericTypeDefinition();
        string typeName = BuildStandardGenericTypeName(genericDefinition, type.GetGenericArguments());
        Type? fixedType = type.Assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
        return fixedType ?? type;
    }

    /// <summary>
    /// 构建标准的泛型类型名称（符合 .NET 类型全称规范）
    /// </summary>
    /// <param name="genericDefinition">泛型定义（开放泛型，如 List<>）</param>
    /// <param name="genericArguments">泛型参数类型数组（如 int[] 对应 List<int>）</param>
    /// <returns>标准泛型类型全称（如 System.Collections.Generic.List`1[System.Int32]）</returns>
    private static string BuildStandardGenericTypeName(Type genericDefinition, Type[] genericArguments)
    {
        string baseTypeName = genericDefinition.Name;
        int backtickIndex = baseTypeName.IndexOf('`');
        if (backtickIndex > 0)
            baseTypeName = baseTypeName.Substring(0, backtickIndex);

        string genericSuffix = $"`{genericArguments.Length}";
        string genericBaseName = $"{genericDefinition.Namespace}.{baseTypeName}{genericSuffix}";

        if (genericArguments.Length == 0 || genericArguments.All(arg => arg == null))
            return genericBaseName; // 开放泛型直接返回（如 List`1）

        string argsTypeName = string.Join(",", genericArguments.Select(arg =>
        {
            // 嵌套泛型（如 List<string>）需递归处理名称格式
            if (arg.IsGenericType)
                return BuildStandardGenericTypeName(arg.GetGenericTypeDefinition(), arg.GetGenericArguments());
            // 非泛型参数直接用全称（如 System.Int32）
            return arg.FullName;
        }));
        return $"{genericBaseName}[{argsTypeName}]";
    }
}
