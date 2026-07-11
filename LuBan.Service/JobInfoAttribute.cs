/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
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