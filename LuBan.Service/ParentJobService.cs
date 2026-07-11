/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Service
*文件名： ParentJobService
*版本号： V1.0.0.0
*唯一标识：33130cfe-f76b-4cf7-af18-75e794f01a0f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/7 15:47:22
*描述：框架内置父任务服务
*
*=================================================
*修改标记
*修改时间：2025/3/7 15:47:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架内置父任务服务
*
*****************************************************************************/
namespace LuBan.Service;

/// <summary>
/// 框架内置父任务服务，
/// 用于处理子任务
/// </summary>
public sealed class ParentJobService : BaseJobService, IJob
{
    static Dictionary<Type, BaseSubJobService> _subJobServices = [];

    /// <summary>
    /// 框架内置父任务服务，
    /// 用于处理子任务
    /// </summary>
    public ParentJobService() : base(0)
    {
        var instances = DynamicUtil.DynamicLoadInstance<BaseSubJobService>();
        if (instances == null || instances.Count < 1) return;
        foreach (var item in instances)
        {
            if (item.Value is BaseSubJobService subJob)
            {
                _subJobServices.TryAdd(item.Key, subJob);
            }
        }
    }


    /// <summary>
    /// 运行
    /// </summary>
    public override void Run()
    {
        if (_subJobServices == null || _subJobServices.Count < 1) return;

        while (IsRunning)
        {
            Thread.Sleep(100);

            var currentTime = DateTime.Now;

            foreach (var service in _subJobServices)
            {
                if (service.Value == null)
                {
                    continue;
                }

                // 检查是否应该执行任务
                if (service.Value.ShouldRun(currentTime))
                {
                    service.Value.Run();
                    service.Value.LastRunTime = currentTime;
                }
            }
        }
    }
}
