/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： DynamicUtil
*版本号： V1.0.0.0
*唯一标识：716382f3-9451-4131-9393-a3afb79faa86
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/26 17:39:04
*描述：动态工具
*
*=================================================
*修改标记
*修改时间：2025/11/26 17:39:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：动态工具
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 动态工具
/// </summary>
public static class DynamicUtil
{

    /// <summary>
    /// 获取程序集
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static Assembly GetAssemblyName(string assemblyName)
    {
        return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
    }

    /// <summary>
    /// 获取程序集
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetAssemblyName(Type type)
    {
        return GetAssemblyName(type.GetTypeInfo());
    }



    /// <summary>
    /// 加载程序集类型，支持格式：程序集;网站类型命名空间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Type? GetType(string formatStr)
    {
        var typeDefinitions = formatStr.Split(';');
        return GetType(typeDefinitions[0], typeDefinitions[1]);
    }

    /// <summary>
    /// 根据程序集名称、类型完整限定名获取运行时类型
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    public static Type? GetType(string assemblyName, string typeFullName)
    {
        return GetAssemblyName(assemblyName).GetType(typeFullName);
    }


    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static IEnumerable<Type>? DynamicLoadTypes(this Assembly assembly, string typeName = "")
    {
        if (assembly == null) return null;

        Type[] types = assembly.GetTypes();

        if (types == null || types.Length < 1) return null;

        List<Type> result = new List<Type>();

        foreach (var ty in types)
        {
            if (ty == null)
            {
                continue;
            }
            if (typeName.IsNullOrEmpty())
            {
                result.Add(ty);
                continue;
            }
            if (EqualsName(ty, typeName))
            {
                result.Add(ty);
            }
        }

        return result;
    }


    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<Type>? DynamicLoadTypes<T>(this Assembly assembly)
    {
        if (assembly == null) return null;

        Type[] types = assembly.GetTypes();

        if (types == null || types.Length < 1) return null;

        return types.Where(q => typeof(T).IsAssignableFrom(q));
    }

    /// <summary>
    /// 判断名称是否存在，包括嵌套的父类
    /// </summary>
    /// <param name="type"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static bool EqualsName(Type? type, string typeName)
    {
        if (type == null || typeName.IsNullOrEmpty()) return false;
        if (type.Name.Equals(typeName, true) || type.FullName.Equals(typeName, true))
        {
            return true;
        }
        return EqualsName(type.BaseType, typeName);
    }

    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <param name="assemblies"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static IEnumerable<Type> DynamicLoadTypes(this List<Assembly> assemblies, string typeName = "")
    {
        foreach (var assembly in assemblies)
        {
            if (assembly != null)
            {
                var types = assembly.DynamicLoadTypes(typeName);
                if (types != null && types.Any())
                {
                    foreach (var type in types)
                    {
                        yield return type;
                    }
                }
            }
        }
        yield break;
    }

    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IEnumerable<Type> DynamicLoadTypes<T>(this List<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (assembly != null)
            {
                var types = assembly.DynamicLoadTypes<T>();
                if (types != null && types.Any())
                {
                    foreach (var type in types)
                    {
                        yield return type;
                    }
                }
            }
        }
        yield break;
    }


    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <param name="dllPath"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static IEnumerable<Type>? DynamicLoadTypes(string dllPath, string typeName = "")
    {
        var assembly = Assembly.LoadFile(dllPath);

        return assembly.DynamicLoadTypes(typeName);
    }

    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static IEnumerable<Type>? DynamicLoadTypes(string typeName = "")
    {
        return RuntimeUtil.GetAssemblies()?.DynamicLoadTypes(typeName) ?? null;
    }

    /// <summary>
    /// 动态加载类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<Type>? DynamicLoadTypes<T>()
    {
        return RuntimeUtil.GetAssemblies()?.DynamicLoadTypes<T>() ?? null;
    }



    /// <summary>
    /// 动态加载实例
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="dllPath"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Dictionary<Type, dynamic>? DynamicLoadInstance(string typeName, string dllPath, params object[] args)
    {
        var types = DynamicLoadTypes(dllPath, typeName);
        if (types == null || !types.Any()) return null;
        Dictionary<Type, dynamic> dic = [];
        foreach (var ty in types)
        {
            if (ty == null) continue;
            if (ty.IsAbstract || ty.IsInterface || ty.IsPrimitive || ty.IsNotPublic || ty.IsNestedPrivate || !ty.IsVisible) continue;
            var instance = Activator.CreateInstance(ty, args);
            if (instance == null) continue;
            dic.Add(ty, instance);
        }
        return dic;
    }

    /// <summary>
    /// 动态加载实例
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Dictionary<Type, dynamic> DynamicLoadInstance(string? typeName, params object[] args)
    {
        if (typeName.IsNullOrEmpty()) return [];
        var tys = DynamicLoadTypes(typeName);
        if (tys == null || !tys.Any()) return [];
        Dictionary<Type, dynamic> dic = [];
        foreach (var ty in tys)
        {
            if (ty == null) continue;
            if (ty.IsAbstract || ty.IsInterface || ty.IsPrimitive || ty.IsNotPublic || ty.IsNestedPrivate || !ty.IsVisible) continue;
            var obj = Activator.CreateInstance(ty, args);
            if (obj != null)
                dic.Add(ty, obj);
        }
        return dic;
    }

    /// <summary>
    /// 动态加载实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<Type, dynamic> DynamicLoadInstance<T>(params object[] args) where T : class
    {
        var types = DynamicLoadTypes<T>();
        if (types == null || !types.Any()) return [];
        Dictionary<Type, dynamic> dic = [];
        foreach (var ty in types)
        {
            if (ty == null) continue;
            if (ty.IsAbstract || ty.IsInterface || ty.IsPrimitive || ty.IsNotPublic || ty.IsNestedPrivate || !ty.IsVisible) continue;
            var obj = Activator.CreateInstance(ty, args);
            if (obj != null)
                dic.Add(ty, obj);
        }
        return dic;
    }



    /// <summary>
    /// 动态加载实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    public static void DynamicLoadInstance<T>(Action<KeyValuePair<Type, dynamic>> action) where T : class
    {
        var list = DynamicLoadInstance<T>();
        if (list != null && list.Count > 0)
        {
            foreach (var item in list)
            {
                action.Invoke(item);
            }
        }
    }


    /// <summary>
    /// 动态加载dll并执行某类中的方法
    /// </summary>
    /// <param name="dllPath"></param>
    /// <param name="typeName"></param>
    /// <param name="initArgs"></param>
    /// <param name="methodName"></param>
    /// <param name="methodArgs"></param>
    /// <returns></returns>
    public static IEnumerable<dynamic> DynamicExecute(string dllPath, string typeName, object[] initArgs, string methodName, params object[] methodArgs)
    {
        var dic = DynamicLoadInstance(dllPath, typeName, initArgs);
        if (dic == null || dic.Count < 1) yield break;
        foreach (var obj in dic)
        {
            yield return FastILUtil.MethodInvoke(obj.Key, methodName, obj.Value, methodArgs);
        }
    }

    /// <summary>
    /// 动态加载dll并执行某类中的方法
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="methodName"></param>
    /// <param name="methodArgs"></param>
    /// <returns></returns>
    public static IEnumerable<dynamic> DynamicExecute(string typeName, string methodName, params object[] methodArgs)
    {
        var dic = DynamicLoadInstance(typeName);
        if (dic == null || dic.Count < 1) yield break;
        foreach (var obj in dic)
        {
            yield return FastILUtil.MethodInvoke(obj.Key, methodName, obj.Value, methodArgs);
        }
    }

    /// <summary>
    /// 动态执行方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="objs"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static List<T>? DynamicExecute<T>(this Dictionary<Type, T> objs, string methodName, params object[] args)
    {
        if (objs == null || objs.Count < 1) return null;
        List<T> list = [];
        foreach (var item in objs)
        {
            if (item.Value == null) continue;
            var data = item.Key.DynamicExecute(item.Value, methodName, args);
            if (data == null) continue;
            list.Add((T)data);
        }
        return list;
    }


    /// <summary>
    /// 动态执行方法
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object? DynamicExecute(this object obj, string methodName, params object[] args)
    {
        return FastILUtil.MethodInvoke(obj.GetType(), methodName, obj, args);
    }



    /// <summary>
    /// 动态执行方法
    /// </summary>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object? DynamicExecute(this Type type, object obj, string methodName, params object[] args)
    {
        return FastILUtil.MethodInvoke(type, methodName, obj, args);
    }

    /// <summary>
    /// 动态执行方法
    /// </summary>
    /// <param name="objs"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static List<object>? DynamicExecute(this IEnumerable<object> objs, string methodName, params object[] args)
    {
        if (objs == null) return null;

        List<object> results = [];

        foreach (var obj in objs)
        {
            var result = obj.DynamicExecute(methodName, args);
            if (result != null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    /// <summary>
    /// 获取某个接口的类实例方法执行结果集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dllFile"></param>
    /// <param name="methodName"></param>
    /// <param name="typeName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static List<object> DynamicExecute<T>(string dllFile,
        string methodName,
        string typeName = "",
        params object[] args)
    {
        var instances = DynamicLoadInstance(typeName, dllFile);
        if (instances == null || instances.Count < 1) return [];
        var result = instances.DynamicExecute(methodName, args);
        if (result != null)
            return [.. result];
        return [];
    }

    /// <summary>
    /// 获取某个接口的类实例方法执行结果集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dllPaths"></param>
    /// <param name="methodName"></param>
    /// <param name="typeName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static List<object>? DynamicExecute<T>(IEnumerable<string> dllPaths,
        string methodName,
        string typeName = "",
        params object[] args)
    {
        if (dllPaths != null)
        {
            var result = new List<object>();
            foreach (var dllPath in dllPaths)
            {
                var data = DynamicExecute<T>(dllPath, methodName, typeName, args);
                if (data == null || data.Count < 1) continue;
                result.AddRange(data);
            }
            return result;
        }
        return null;
    }

}
