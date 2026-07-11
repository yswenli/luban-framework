/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
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
    public Guid Id { get; } = Guid.NewGuid();
    public Func<Task> Func { get; }
    public PoolTaskStatus Status { get; internal set; } = PoolTaskStatus.Pending;
    public Exception? Exception { get; internal set; }
    public DateTime? StartTime { get; internal set; }
    public DateTime? EndTime { get; internal set; }

    public PoolTaskInfo2(Func<Task> func)
    {
        Func = func;
    }
}
