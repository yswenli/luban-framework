/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.OnlineUser
*文件名： OnlineUserStoreProvider
*版本号： V1.0.0.0
*唯一标识：0506282b-82ec-4dd9-aede-99d02655c571
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:53:58
*描述：在线用户存储提供器
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:53:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：在线用户存储提供器
*
*****************************************************************************/

using System.Reflection;

namespace LuBan.Web.Core.OnlineUser;

/// <summary>
/// 在线用户存储提供器
/// </summary>
public static class OnlineUserStoreProvider
{
    private static IOnlineUserStore? _registeredStore;
    private static readonly object _registerLock = new();

    private static readonly Lazy<IOnlineUserStore> _lazyStore = new(() =>
    {
        // 优先使用已注册的 store
        lock (_registerLock)
        {
            if (_registeredStore != null)
                return _registeredStore;
        }

        // 尝试从 DI 容器解析工厂，触发注册
        TryResolveFactoryFromDI();

        // 再次检查注册结果
        lock (_registerLock)
        {
            if (_registeredStore != null)
                return _registeredStore;
        }

        // 回退：反射创建
        return CreateStoreFromReflection();
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 注册在线用户存储实现
    /// </summary>
    public static void Register(IOnlineUserStore store)
    {
        lock (_registerLock)
        {
            _registeredStore = store;
        }
    }

    /// <summary>
    /// 获取在线用户存储实现
    /// </summary>
    public static IOnlineUserStore Store => _lazyStore.Value;

    /// <summary>
    /// 尝试从 DI 容器解析工厂，触发注册
    /// </summary>
    private static void TryResolveFactoryFromDI()
    {
        try
        {
            Type? factoryType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                factoryType = GetLoadableTypes(assembly)
                    .FirstOrDefault(t => t.Name == "OnlineUserStoreFactory" && t.GetInterfaces().Any(i => i.Name == "ISingleton"));
                if (factoryType != null) break;
            }

            if (factoryType == null)
                return;

            var method = typeof(ServiceProviderUtil)
                .GetMethod("GetService", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes);
            if (method == null)
                return;

            var generic = method.MakeGenericMethod(factoryType);
            generic.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Logger.Error("OnlineUserStoreProvider", $"从DI解析工厂失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 通过反射创建存储实例
    /// </summary>
    private static IOnlineUserStore CreateStoreFromReflection()
    {
        var useRedis = HostingOptions.Default.EnableRedisCache;
        var storeTypeName = useRedis ? "RedisOnlineUserStore" : "DbOnlineUserStore";

        Type? storeType = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            storeType = GetLoadableTypes(assembly)
                .FirstOrDefault(t => t.Name == storeTypeName && typeof(IOnlineUserStore).IsAssignableFrom(t));
            if (storeType != null) break;
        }

        if (storeType != null)
        {
            return (IOnlineUserStore)Activator.CreateInstance(storeType)!;
        }

        throw new InvalidOperationException($"OnlineUserStore未找到，请确保{storeTypeName}所在程序集已被加载");
    }

    /// <summary>
    /// 安全获取程序集中可加载的类型
    /// </summary>
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types?.Where(t => t != null).Select(t => t!) ?? Type.EmptyTypes;
        }
        catch
        {
            return Type.EmptyTypes;
        }
    }
}