/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading.Core
*文件名： PoolTaskInfo
*版本号： V1.0.0.0
*唯一标识：13080a24-d2e1-44a1-a4df-9fae3d8a2800
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/21 13:49:29
*描述：
*
*=================================================
*修改标记
*修改时间：2025/5/21 13:49:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Threading.Core;

/// <summary>
/// 线程池任务包装
/// </summary>
public class PoolTaskInfo
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>
    /// 要执行任务
    /// </summary>
    public Action Action { get; }
    /// <summary>
    /// 任务状态
    /// </summary>
    public PoolTaskStatus Status { get; internal set; } = PoolTaskStatus.Pending;
    /// <summary>
    /// 异常信息
    /// </summary>
    public Exception? Exception { get; internal set; }
    /// <summary>
    /// 任务开始时间
    /// </summary>
    public DateTime? StartTime { get; internal set; }
    /// <summary>
    /// 任务结束时间
    /// </summary>
    public DateTime? EndTime { get; internal set; }
    /// <summary>
    /// 线程池任务包装
    /// </summary>
    /// <param name="action"></param>
    public PoolTaskInfo(Action action)
    {
        Action = action;
    }
}
