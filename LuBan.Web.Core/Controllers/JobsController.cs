/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
*修改时间：2026/01/13 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业监控与日志管理控制器
*
*****************************************************************************/
namespace LuBan.Web.Core.Controllers;

#nullable disable warnings

/// <summary>
/// 作业监控与日志管理控制器
/// </summary>
public sealed class JobsController : BaseAdminController
{
    /// <summary>
    /// 获取作业日志列表
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="status">运行状态</param>
    /// <param name="result">运行结果</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页条数</param>
    /// <returns>作业日志列表</returns>
    [HttpGet]
    public PagedList<DbLogJob> GetJobLogs(string jobName = null, DateTime? startTime = null, DateTime? endTime = null,
        EnumJobStatus? status = null, EnumJobResult? result = null, int pageIndex = 1, int pageSize = 20)
    {
        return JobLogService.Instance.GetJobLogs(jobName, startTime, endTime, status, result, pageIndex, pageSize);
    }

    /// <summary>
    /// 获取作业日志详情
    /// </summary>
    /// <param name="id">日志ID</param>
    /// <returns>作业日志详情</returns>
    [HttpGet]
    public DbLogJob GetJobLogDetail(long id)
    {
        return JobLogService.Instance.GetJobLogDetail(id);
    }

    /// <summary>
    /// 获取作业当前状态
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <returns>作业当前状态</returns>
    [HttpGet]
    public dynamic GetJobStatus(string jobName)
    {
        var jobInfo = JobInfosCache.Instance[jobName];
        if (jobInfo != null)
        {
            return new { JobName = jobName, Status = jobInfo.Status };
        }
        else
        {
            // 如果缓存中没有找到，再尝试从数据库获取最新状态
            var dbStatus = JobLogService.Instance.GetJobCurrentStatus(jobName);
            return new { JobName = jobName, Status = dbStatus };
        }
    }

    /// <summary>
    /// 获取所有作业信息
    /// </summary>
    /// <returns>所有作业信息</returns>
    [HttpGet]
    public List<JobInfo> GetAllJobs()
    {
        return JobInfosCache.Instance.List;
    }

    /// <summary>
    /// 启动所有作业
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPost]
    public string StartAllJobs()
    {
        JobServiceLoader.Start();
        return "所有作业启动成功";
    }

    /// <summary>
    /// 停止所有作业
    /// </summary>
    /// <returns>操作结果</returns>
    [HttpPost]
    public string StopAllJobs()
    {
        JobServiceLoader.Stop();
        return "所有作业停止成功";
    }

    /// <summary>
    /// 启动指定作业
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <returns>操作结果</returns>
    [HttpPost]
    public string StartJob(string jobName)
    {
        JobServiceLoader.StartJob(jobName);
        return $"作业 {jobName} 启动成功";
    }

    /// <summary>
    /// 停止指定作业
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <returns>操作结果</returns>
    [HttpPost]
    public string StopJob(string jobName)
    {
        JobServiceLoader.StopJob(jobName);
        return $"作业 {jobName} 停止成功";
    }

    /// <summary>
    /// 删除作业日志
    /// </summary>
    /// <param name="jobName">作业名称（可选，为空则删除所有日志）</param>
    /// <returns>操作结果</returns>
    [HttpPost]
    public string DeleteJobLogs(string jobName = null)
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
}
