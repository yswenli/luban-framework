/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： RuntimeUtil
*版本号： V1.0.0.0
*唯一标识：390fab72-c146-4dc0-89ef-b0172069d58b
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 14:11:03
*描述：程序集相关工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 14:11:03
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：程序集相关工具类
*
*****************************************************************************/

namespace LuBan.Common;

/// <summary>
/// 程序集相关工具类
/// </summary>
public static class RuntimeUtil
{
    static readonly HashSet<string> _exceptSet = new(StringComparer.OrdinalIgnoreCase)
    {
        "mscorlib",
        "System",
        "netstandard",
        "dotnet",
        "aspnet",
        "Window",
        "Anonymously",
        "Azure",
        "AngleSharp",
        "Aspose",
        "Aliyun",
        "Ben",
        "BouncyCastle",
        "crypto",
        "Enums.NET",
        "ExtendedNumerics.BigDecimal",
        "Flurl",
        "Google",
        "Grpc",
        "HtmlAgilityPack",
        "Humanizer",
        "Org",
        "Remotion",
        "Newtonsoft",
        "Mapster",
        "Microsoft",
        "Magicodes",
        "MailKit",
        "MimeKit",
        "Mono",
        "MySqlConnector",
        "MathNet",
        "Lazy",
        "log4net",
        "Nacos",
        "nacos-sdk-csharp",
        "Nito",
        "Npgsql",
        "NPOI",
        "NuGet",
        "Oracle",
        "Oscar",
        "SQLite",
        "SharpZipLib",
        "Swashbuckle",
        "SKIT",
        "SixLabors",
        "SqlSugar",
        "SkiaSharp",
        "Yitter",
        "ZXing",
        "<"
    };

    static readonly ConcurrentBag<string> _customExclusions = new();

    /// <summary>
    /// 添加自定义排除前缀
    /// </summary>
    /// <param name="prefix">要排除的程序集名称前缀</param>
    public static void AddExclusion(string prefix)
    {
        _customExclusions.Add(prefix);
    }

    /// <summary>
    /// 清除自定义排除前缀
    /// </summary>
    public static void ClearExclusions()
    {
        while (_customExclusions.TryTake(out _)) { }
    }

    static bool IsExcluded(string name)
    {
        if (_exceptSet.Contains(name)) return true;
        foreach (var prefix in _exceptSet)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
        }
        foreach (var prefix in _customExclusions)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    /// <summary>
    /// 获取指定的程序集
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    /// <param name="fromCache"></param>
    /// <returns></returns>
    public static List<Assembly>? GetAssemblies(string prefix = "", string suffix = "", bool fromCache = true)
    {
        var cacheKey = $"{CacheConst.KeySystem}assemblies:{prefix}_{suffix}";
        if (!fromCache)
        {
            var assemblies = LoadAllAssemblies();

            if (prefix.IsNullOrEmpty() && suffix.IsNullOrEmpty()) return assemblies;

            var list = new List<Assembly>();

            if (assemblies != null && assemblies.Count > 0)
            {
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in assemblies)
                {
                    if (item.FullName.IsNullOrEmpty() || !seen.Add(item.FullName)) continue;

                    var matched = false;

                    if (prefix.IsNotNullOrEmpty())
                    {
                        matched = item.FullName.StartsWith(prefix);
                    }

                    if (!matched && suffix.IsNotNullOrEmpty())
                    {
                        var name = item.FullName.Substring(0, item.FullName.IndexOf(","));
                        matched = name.EndsWith(suffix);
                    }

                    if (!matched) continue;

                    list.Add(item);
                }
            }
            MemoryCache.Instance.Set(cacheKey, list);
            return list;
        }
        return MemoryCache.Instance.GetOrSet(cacheKey, (k) => GetAssemblies(prefix, suffix, false));
    }

    /// <summary>
    /// 获取全部程序集
    /// </summary>
    /// <returns></returns>
    static List<Assembly>? LoadAllAssemblies()
    {
        using var locker = LockerBuilder.Default.Create("RuntimeUtil.LoadAllAssemblies");
        var list = new List<Assembly>();
        if (DependencyContext.Default != null)
        {
            foreach (var lib in DependencyContext.Default.CompileLibraries)
            {
                var libName = lib.Name;
                if (lib.Serviceable || IsExcluded(libName)) continue;
                if (libName.StartsWith("LuBan."))
                {
                    libName = libName["LuBan.".Length..];
                }
                try
                {
                    list.Add(Assembly.Load(new AssemblyName(libName)));
                }
                catch { }
            }
            foreach (var lib in DependencyContext.Default.RuntimeLibraries)
            {
                var libName = lib.Name;
                if (lib.Serviceable || IsExcluded(libName)) continue;
                if (libName.StartsWith("LuBan."))
                {
                    libName = libName["LuBan.".Length..];
                }
                try
                {
                    list.Add(Assembly.Load(new AssemblyName(libName)));
                }
                catch { }
            }
        }

        var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var item in currentAssemblies)
        {
            if (item.FullName.IsNullOrEmpty() || IsExcluded(item.FullName)) continue;
            list.Add(item);
        }
        return list.Distinct().ToList();
    }

    /// <summary>
    /// GetAssembly
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static Assembly? GetAssembly(string assemblyName)
    {
        if (assemblyName.IsNullOrEmpty()) return null;
        return GetAssemblies()?.FirstOrDefault(f => f.FullName.IsNotNullOrEmpty() && f.FullName.Contains(assemblyName));
    }

    /// <summary>
    /// GetCurrentAssembly
    /// </summary>
    /// <returns></returns>
    public static Assembly GetCurrentAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }

    /// <summary>
    /// 获取类型列表
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="suffix"></param>
    /// <param name="fromCache"></param>
    /// <returns></returns>
    public static List<Type>? GetTypes(string prefix = "", string suffix = "", bool fromCache = true)
    {
        var cacheKey = $"{CacheConst.KeySystem}types:{prefix}_{suffix}";
        if (!fromCache)
        {
            List<Type> list = [];
            var assemblies = GetAssemblies(prefix, suffix, fromCache);
            if (assemblies == null || assemblies.Count == 0) return list;
            foreach (var assembly in assemblies)
            {
                var typeinfos = assembly.DefinedTypes;
                foreach (var typeinfo in typeinfos)
                {
                    if (typeinfo.FullName.IsNullOrEmpty() || IsExcluded(typeinfo.FullName)) continue;
                    list.Add(typeinfo.AsType());
                }
            }
            MemoryCache.Instance.Set(cacheKey, list);
            return list;
        }
        return MemoryCache.Instance.GetOrSet(cacheKey, (k) => GetTypes(prefix, suffix, false));
    }

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type? GetType(string typeName)
    {
        return GetTypes()?.FirstOrDefault(q => q.Name == typeName);
    }

    /// <summary>
    /// 执行静态方法
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object? StaticInvoke(this Type type, string methodName, object?[]? args)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        return method?.Invoke(null, args);
    }

    /// <summary>
    /// 执行实例化方法
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object? Invoke(this Type type, string methodName, object?[]? args)
    {
        var obj = Activator.CreateInstance(type);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        return method?.Invoke(obj, args);
    }

    /// <summary>
    /// 根据AssemblyName获取所有的类
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static IList<Type> GetTypesByAssembly(string assemblyName)
    {
        List<Type> list = [];
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
        var typeinfos = assembly.DefinedTypes;
        foreach (var typeinfo in typeinfos)
        {
            list.Add(typeinfo.AsType());
        }
        return list;
    }

    static readonly ConcurrentDictionary<Type, List<Type>> _attributeTypeCache = new();

    /// <summary>
    /// 获取当前指定标识的所有类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<Type>? GetTypesByAttribute<T>() where T : Attribute
    {
        try
        {
            return _attributeTypeCache.GetOrAdd(typeof(T), attrType =>
            {
                var types = GetTypes();
                return types?.Where(u => u.IsDefined(attrType, false)).ToList() ?? new List<Type>();
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// GetImplementType
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="baseInterfaceType"></param>
    /// <returns></returns>
    public static Type? GetImplementType(string typeName, Type baseInterfaceType)
    {
        var types = GetTypes();
        if (types == null) return null;
        return types.FirstOrDefault(t =>
        {
            if (t.Name == typeName && t.GetTypeInfo().GetInterfaces().Any(b => b.Name == baseInterfaceType.Name))
            {
                var typeinfo = t.GetTypeInfo();
                return typeinfo.IsClass && !typeinfo.IsAbstract && !typeinfo.IsGenericType;
            }
            return false;
        });
    }

    /// <summary>
    /// 是否是windows
    /// </summary>
    /// <returns></returns>
    public static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
    /// <summary>
    /// 是否是Linux
    /// </summary>
    /// <returns></returns>
    public static bool IsLinux()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
    /// <summary>
    /// 是否是FreeBSD
    /// </summary>
    /// <returns></returns>
    public static bool IsFreeBSD()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
    }
    /// <summary>
    /// 是否是OSX
    /// </summary>
    /// <returns></returns>
    public static bool IsOSX()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }
}
