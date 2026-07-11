/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Service
*文件名： BaseSubJobService
*版本号： V1.0.0.0
*唯一标识：7b6d1750-311d-42c6-a373-b583957adcd2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/7 15:52:39
*描述：
*
*=================================================
*修改标记
*修改时间：2025/3/7 15:52:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Service;

/// <summary>
/// 子任务服务基类
/// </summary>
public abstract class BaseSubJobService
{
    /// <summary>
    /// 子任务执行间隔时间
    /// </summary>
    public int IntervalTime { get; set; } = 100;

    /// <summary>
    /// 上次执行时间
    /// </summary>
    public DateTime LastRunTime { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 自定义业务逻辑
    /// </summary>
    public abstract void Run();

    /// <summary>
    /// 是否应该执行任务
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <returns>是否应该执行</returns>
    public bool ShouldRun(DateTime currentTime)
    {
        // 如果是第一次执行，或者已经过了间隔时间
        return LastRunTime == DateTime.MinValue ||
               (currentTime - LastRunTime).TotalMilliseconds >= IntervalTime;
    }
}
