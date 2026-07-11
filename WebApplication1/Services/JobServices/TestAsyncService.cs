/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Services.JobServices
*文件名： TestAsyncService
*版本号： V1.0.0.0
*唯一标识：163d67a3-3286-41df-97df-b43534b7dfed
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/18 11:17:16
*描述：测试后台服务的异步方法
*
*=================================================
*修改标记
*修改时间：2025/8/18 11:17:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：测试后台服务的异步方法
*
*****************************************************************************/
using LuBan.Service;

namespace WebApplication1.Services.JobServices;

/// <summary>
/// 测试后台服务的异步方法
/// </summary>
[JobInfo<TestAsyncService>("测试后台服务的异步方法")]
public class TestAsyncService : BaseJobService
{
    /// <summary>
    /// 测试后台服务的异步方法
    /// </summary>
    public TestAsyncService() : base(1000)
    {

    }

    /// <summary>
    /// 测试后台服务的异步方法
    /// </summary>
    public override void Run()
    {
        //ConsoleUtil.WriteLine("TestAsyncService Run");
    }
    /// <summary>
    /// 测试后台服务的异步方法
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        //ConsoleUtil.WriteLine("TestAsyncService RunAsync");
        // 移除不必要的短延迟，任务已通过构造函数设置了1000ms的执行间隔
        await Task.CompletedTask;
    }
}
