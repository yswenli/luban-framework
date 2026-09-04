/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Threading.Core
*文件名： ISimplePool.cs
*版本号： V1.0.0.0
*唯一标识：1f780191-0dd4-4a5e-b27a-4774849f829a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:29
*描述：ISimplePool 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ISimplePool 类
*
*****************************************************************************/

namespace LuBan.Threading.Core;

/// <summary>
/// 简单任务池接口，定义任务池的基本操作方法
/// </summary>
public interface ISimplePool : IDisposable
{
    /// <summary>
    /// 任务池名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 入队一个同步任务
    /// </summary>
    /// <param name="task">要执行的同步任务</param>
    /// <returns>任务ID</returns>
    Guid Enqueue(Action task);

    /// <summary>
    /// 入队一个异步任务
    /// </summary>
    /// <param name="task">要执行的异步任务</param>
    /// <returns>任务ID</returns>
    Guid Enqueue(Func<Task> task);

    /// <summary>
    /// 获取指定任务的详细信息
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>任务信息，若不存在则为 null</returns>
    PoolTaskInfo2? GetTaskInfo(Guid taskId);

    /// <summary>
    /// 获取指定任务的状态
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>任务状态，若不存在则为 null</returns>
    PoolTaskStatus? GetTaskStatus(Guid taskId);
}
