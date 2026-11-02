/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading.Core
*文件名： PoolTaskInfo2
*版本号： V1.0.0.0
*唯一标识：575248df-b368-4cb2-a816-3b6496496e77
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/21 13:52:09
*描述：任务池任务包装
*
*=================================================
*修改标记
*修改时间：2025/5/21 13:52:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：任务池任务包装
*
*****************************************************************************/
namespace LuBan.Threading.Core;

/// <summary>
/// 任务池任务包装
/// </summary>
public class PoolTaskInfo2
{
    /// <summary>
    /// 任务唯一标识，创建时自动生成。
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>
    /// 任务实际执行的异步委托。
    /// </summary>
    public Func<Task> Func { get; }
    /// <summary>
    /// 任务当前状态，初始为待执行。
    /// </summary>
    public PoolTaskStatus Status { get; internal set; } = PoolTaskStatus.Pending;
    /// <summary>
    /// 任务执行失败时捕获的异常，成功时为空。
    /// </summary>
    public Exception? Exception { get; internal set; }
    /// <summary>
    /// 任务开始执行的时间，尚未开始为空。
    /// </summary>
    public DateTime? StartTime { get; internal set; }
    /// <summary>
    /// 任务结束的时间，尚未结束为空。
    /// </summary>
    public DateTime? EndTime { get; internal set; }

    /// <summary>
    /// 用待执行的异步委托包装出任务池任务信息。
    /// </summary>
    /// <param name="func">任务实际执行的异步委托。</param>
    public PoolTaskInfo2(Func<Task> func)
    {
        Func = func;
    }
}
