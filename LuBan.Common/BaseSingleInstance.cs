/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： BaseSingleInstance
*版本号： V1.0.0.0
*唯一标识：d13f644f-0058-4283-a703-9924e2b93cf2
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/10/19 9:47:39
*描述：懒加载单例模式基类
*
*=====================================================================
*修改标记
*修改时间：2021/10/19 9:47:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：懒加载单例模式基类
*
*****************************************************************************/

namespace LuBan.Common;

public abstract class BaseSingleInstance<T> where T : new()
{
    static readonly Lazy<T> _lazy = new Lazy<T>(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static T Instance => _lazy.Value;
}

/// <summary>
/// 单例工具类
/// </summary>
public static class SingleInstance
{
    static readonly ConcurrentDictionary<string, dynamic> _cache;

    /// <summary>
    /// 单例工具类
    /// </summary>
    static SingleInstance()
    {
        _cache = new();
    }


    /// <summary>
    /// 创建单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public static T? Create<T>(params object[] args) where T : class
    {
        var type = typeof(T);
        var fullName = type.FullName;
        if (fullName.IsNullOrEmpty()) return default;
        return _cache.GetOrAdd(fullName, () =>
        {
            if (args == null || args.Length < 1)
            {
                var val = Activator.CreateInstance(type);
                return val == null ? throw new Exception("创建实例失败") : (dynamic)(T)val;
            }
            else
            {
                var val = Activator.CreateInstance(type, args);
                return val == null ? throw new Exception("创建实例失败") : (dynamic)(T)val;
            }
        });
    }
}
