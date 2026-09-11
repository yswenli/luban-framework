/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Controllers
*文件名： JobsController
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000005
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/01/13 00:00:00
*描述：作业监控与日志管理控制器
*
*=================================================
*修改标记
*修改时间：2026/09/11 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：GET→POST 全面改造，新增配置管理端点，移除 UpdateJobCron
*
*****************************************************************************/
namespace LuBan.Web.Core.Controllers;


/// <summary>
/// 作业监控与日志管理控制器
/// </summary>
public sealed class JobsController : BaseAdminController
{
    /// <summary>
    /// 获取作业日志列表
    /// </summary>
    [HttpPost]
    public PagedList<DbLogJob> GetJobLogs(string? jobName = null, DateTime? startTime = null, DateTime? endTime = null,
        EnumJobStatus? status = null, EnumJobResult? result = null, int pageIndex = 1, int pageSize = 20)
    {
        return JobLogService.Instance.GetJobLogs(jobName, startTime, endTime, status, result, pageIndex, pageSize);
    }

    /// <summary>
    /// 获取作业日志详情
    /// </summary>
    [HttpPost]
    public DbLogJob GetJobLogDetail(long id)
    {
        return JobLogService.Instance.GetJobLogDetail(id);
    }

    /// <summary>
    /// 获取作业当前状态
    /// </summary>
    [HttpPost]
    public dynamic GetJobStatus(string jobName)
    {
        var jobInfo = JobInfosCache.Instance[jobName];
        if (jobInfo != null)
        {
            return new { JobName = jobName, Status = jobInfo.Status };
        }
        else
        {
            var dbStatus = JobLogService.Instance.GetJobCurrentStatus(jobName);
            return new { JobName = jobName, Status = dbStatus };
        }
    }

    /// <summary>
    /// 获取所有作业信息
    /// </summary>
    [HttpPost]
    public List<JobInfo> GetAllJobs()
    {
        return JobInfosCache.Instance.List;
    }

    /// <summary>
    /// 启动所有作业
    /// </summary>
    [HttpPost]
    public string StartAllJobs()
    {
        JobServiceLoader.Start();
        return "所有作业启动成功";
    }

    /// <summary>
    /// 停止所有作业
    /// </summary>
    [HttpPost]
    public string StopAllJobs()
    {
        JobServiceLoader.Stop();
        return "所有作业停止成功";
    }

    /// <summary>
    /// 启动指定作业
    /// </summary>
    [HttpPost]
    public string StartJob(string jobName)
    {
        JobServiceLoader.StartJob(jobName);
        return $"作业 {jobName} 启动成功";
    }

    /// <summary>
    /// 停止指定作业
    /// </summary>
    [HttpPost]
    public string StopJob(string jobName)
    {
        JobServiceLoader.StopJob(jobName);
        return $"作业 {jobName} 停止成功";
    }

    /// <summary>
    /// 删除作业日志
    /// </summary>
    [HttpPost]
    public string DeleteJobLogs(string? jobName = null)
    {
        JobLogService.Instance.DeleteJobLogs(jobName);
        if (string.IsNullOrEmpty(jobName))
        {
            return $"所有作业日志删除成功";
        }
        else
        {
            return $"作业 {jobName} 的日志删除成功";
        }
    }

    /// <summary>
    /// 获取指定作业的 Cron 表达式
    /// </summary>
    [HttpPost]
    public dynamic GetJobCron(string name)
    {
        var cron = JobServiceLoader.GetJobCron(name);
        return new { JobName = name, Cron = cron };
    }

    /// <summary>
    /// 获取指定作业的下一次执行时间
    /// </summary>
    [HttpPost]
    public dynamic GetJobNextOccurrence(string name)
    {
        var next = JobServiceLoader.GetJobNextOccurrence(name);
        return new { JobName = name, NextOccurrence = next };
    }

    /// <summary>
    /// 获取所有作业配置
    /// </summary>
    [HttpPost]
    public List<DbJobInfo> GetJobConfigs()
    {
        return JobConfigService.Instance.GetAllJobConfigs();
    }

    /// <summary>
    /// 更新指定作业的配置
    /// </summary>
    [HttpPost]
    public dynamic UpdateJobConfig(string name, string? cron = null, bool? isEnabled = null, string? remark = null)
    {
        var oldConfig = JobConfigService.Instance.GetJobConfig(name);
        if (oldConfig == null)
            throw new FriendlyException($"作业 '{name}' 的配置不存在", ErrorCategory.Business);

        var oldCron = oldConfig.Cron;
        var oldEnabled = oldConfig.IsEnabled;

        JobConfigService.Instance.UpdateJobConfig(name, cron, isEnabled, remark);

        if (cron != null && cron != oldCron)
        {
            var job = JobServiceLoader.GetJobInstance(name);
            if (job is BaseBackgroundService bgService)
                bgService.SetCron(cron);
        }

        if (isEnabled.HasValue && isEnabled.Value != oldEnabled)
        {
            if (isEnabled.Value)
            {
                JobServiceLoader.StartJob(name);
            }
            else
            {
                JobServiceLoader.StopJob(name);
            }
        }

        return new { JobName = name, Cron = cron ?? oldCron, IsEnabled = isEnabled ?? oldEnabled };
    }
}