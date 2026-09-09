/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Services.JobServices
*文件名： CronDemoJob
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000010
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/9 16:00:00
*描述：Cron 表达式调度演示任务
*
*=================================================
*修改标记
*修改时间：2026/9/9 16:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Cron 表达式调度演示任务
*
*****************************************************************************/
using LuBan.Service;

namespace WebApplication1.Services.JobServices;

/// <summary>
/// Cron 表达式调度演示任务
/// </summary>
[JobInfo("CronDemoJob", "Cron 表达式调度演示任务，每 5 分钟执行一次")]
public class CronDemoJob : BaseJobService
{
    /// <summary>
    /// Cron 表达式调度演示任务，每 5 分钟执行一次
    /// </summary>
    public CronDemoJob() : base("0 */5 * * * *")
    {
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    public override async Task RunAsync()
    {
        ConsoleUtil.WriteLine($"[CronDemoJob] 执行时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        await Task.CompletedTask;
    }
}