/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Service
*文件名： JobInfoAttribute
*版本号： V1.0.0.0
*唯一标识：b81209f4-45f0-4096-aa62-476fa895ce93
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/28 16:53:45
*描述：任务信息
*
*=================================================
*修改标记
*修改时间：2025/11/28 16:53:45
*修改人： yswenli
*版本号： V1.0.0.0
*描述：任务信息
*
*****************************************************************************/
using System.Collections.Concurrent;

namespace LuBan.Service;

/// <summary>
/// 任务信息
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class JobInfoAttribute : Attribute
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 任务名称缓存，避免高频调用时重复反射 GetCustomAttribute
    /// </summary>
    private static readonly ConcurrentDictionary<Type, string> _jobNameCache = new();

    /// <summary>
    /// 获取任务名称，
    /// 优先返回 JobInfoAttribute.Name（友好中文名），
    /// 特性不存在或 Name 为 null/空字符串时退回 type.Name（类名）。
    /// 结果按类型缓存，避免高频调用时的反射开销。
    /// 此为全链路统一的任务标识来源：JobServiceLoader 注册/启停、
    /// BaseBackgroundService 写日志、JobsController 查询均应使用本方法，
    /// 避免"任务列表展示类名、日志表存中文名"的双轨不一致问题。
    /// </summary>
    /// <param name="type">任务类型</param>
    /// <returns>任务名称</returns>
    public static string GetJobName(Type type)
    {
        return _jobNameCache.GetOrAdd(type, t =>
        {
            var name = t.GetCustomAttribute<JobInfoAttribute>()?.Name;
            return string.IsNullOrEmpty(name) ? t.Name : name!;
        });
    }

    /// <summary>
    /// 任务信息
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public JobInfoAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// 任务信息
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class JobInfoAttribute<T> : JobInfoAttribute where T : IJob
{
    /// <summary>
    /// 任务信息
    /// </summary>
    /// <param name="description"></param>
    public JobInfoAttribute(string? description = "") : base(typeof(T).Name, description ?? typeof(T).Name)
    {

    }
}