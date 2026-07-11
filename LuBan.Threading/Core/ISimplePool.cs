namespace LuBan.Threading.Core;

/// <summary>
/// 简单任务池接口，定义任务池的基本操作方法
/// </summary>
public interface ISimplePool
{
    /// <summary>
    /// 任务池名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 释放任务池资源
    /// </summary>
    void Dispose();

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
