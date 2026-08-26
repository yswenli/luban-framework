/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Service
*文件名： JobLogService
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000004
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/01/13 00:00:00
*描述：作业日志服务
*
*=================================================
*修改标记
*修改时间：2026/01/13 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业日志服务
*
*****************************************************************************/
namespace LuBan.Service;

/// <summary>
/// 作业日志服务
/// </summary>
public class JobLogService : BaseService<JobLogService>
{
    private readonly ConcurrentDictionary<string, (DateTime LastLogTime, long LogId)> _jobLogTimestamps = new();

    /// <summary>
    /// 记录作业开始（每秒最多记录一次）
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <returns>作业日志ID与开始时间（开始时间用于后续Duration计算，避免从数据库读回导致的时间偏差）</returns>
    public (long LogId, DateTime StartTime) LogJobStart(string jobName)
    {
        var now = DateTime.Now;
        const int intervalSeconds = 1;

        var result = _jobLogTimestamps.AddOrUpdate(
            key: jobName,
            addValueFactory: (name) =>
            {
                // 隔离连接，避免后台作业多线程共享 Scope 导致 MySqlConnection 并发冲突
                using var resp = new BaseRepository<DbLogJob>(isolated: true);
                var jobLog = new DbLogJob
                {
                    Name = name,
                    StartTime = now,
                    Status = EnumJobStatus.Running,
                    CreateTime = now
                };
                jobLog = resp.InsertReturnEntity(jobLog);
                return (now, jobLog.Id);
            },
            updateValueFactory: (name, existing) =>
            {
                if ((now - existing.LastLogTime).TotalSeconds >= intervalSeconds)
                {
                    // 隔离连接，避免后台作业多线程共享 Scope 导致 MySqlConnection 并发冲突
                    using var resp = new BaseRepository<DbLogJob>(isolated: true);
                    var jobLog = new DbLogJob
                    {
                        Name = name,
                        StartTime = now,
                        Status = EnumJobStatus.Running,
                        CreateTime = now
                    };
                    jobLog = resp.InsertReturnEntity(jobLog);
                    return (now, jobLog.Id);
                }
                return existing;
            }
        );
        return (result.LogId, now);
    }

    /// <summary>
    /// 记录作业成功
    /// </summary>
    /// <param name="logId">日志ID</param>
    /// <param name="startTime">作业开始时间（由LogJobStart返回，避免从数据库读回导致Duration计算偏差）</param>
    /// <param name="message">成功消息</param>
    public void LogJobSuccess(long logId, DateTime startTime, string message = "")
    {
        UpdateJobLog(logId, startTime, EnumJobStatus.NotRunning, EnumJobResult.Success, message);
    }

    /// <summary>
    /// 记录作业失败
    /// </summary>
    /// <param name="logId">日志ID</param>
    /// <param name="startTime">作业开始时间（由LogJobStart返回，避免从数据库读回导致Duration计算偏差）</param>
    /// <param name="message">失败消息</param>
    public void LogJobFailed(long logId, DateTime startTime, string message = "")
    {
        UpdateJobLog(logId, startTime, EnumJobStatus.NotRunning, EnumJobResult.Failed, message);
    }

    /// <summary>
    /// 更新作业日志，
    /// 使用隔离连接，避免共享 SqlSugarScope 在异常场景下连接状态冲突
    /// （例如 RunAsync 抛异常后主连接处于 Connecting 状态，导致日志写入失败 "Cannot Open when State is Connecting"）
    /// </summary>
    /// <param name="logId">日志ID</param>
    /// <param name="startTime">作业开始时间（由LogJobStart返回，避免从数据库读回的StartTime与EndTime时间源不同导致Duration为负）</param>
    /// <param name="status">运行状态</param>
    /// <param name="result">运行结果</param>
    /// <param name="message">消息</param>
    private void UpdateJobLog(long logId, DateTime startTime, EnumJobStatus status, EnumJobResult? result, string message)
    {
        using var resp = new BaseRepository<DbLogJob>(isolated: true);
        var jobLog = resp.GetById(logId);

        if (jobLog != null)
        {
            jobLog.Status = status;
            jobLog.Result = result;
            jobLog.Message = message;
            jobLog.EndTime = DateTime.Now;
            // 使用LogJobStart返回的startTime计算Duration，避免数据库时间与应用时间不同步导致负值
            jobLog.Duration = (long)(jobLog.EndTime.Value - startTime).TotalMilliseconds;
            jobLog.UpdateTime = DateTime.Now;
            // 使用UpdateColumns精确指定更新的列，避免IgnoreColumns默认行为可能跳过枚举默认值(0)的列
            resp.AsUpdateable(jobLog)
                .UpdateColumns(it => new { it.Status, it.Result, it.Message, it.EndTime, it.Duration, it.UpdateTime })
                .ExecuteCommand();
        }
    }

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
    public PagedList<DbLogJob> GetJobLogs(string jobName = "", DateTime? startTime = null, DateTime? endTime = null,
        EnumJobStatus? status = null, EnumJobResult? result = null, int pageIndex = 1, int pageSize = 20)
    {
        var resp = new BaseRepository<DbLogJob>();
        var query = resp.AsQueryable();

        if (!string.IsNullOrEmpty(jobName))
        {
            query = query.Where(x => x.Name.Contains(jobName));
        }

        if (startTime.HasValue)
        {
            query = query.Where(x => x.StartTime >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.EndTime <= endTime.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (result.HasValue)
        {
            query = query.Where(x => x.Result == result.Value);
        }

        // 按创建时间倒序排序
        query = query.OrderByDescending(x => x.CreateTime);

        // 执行分页查询
        var totalCount = query.Count();
        return query.ToPagedList(pageIndex, pageSize);
    }

    /// <summary>
    /// 获取作业日志详情
    /// </summary>
    /// <param name="logId">日志ID</param>
    /// <returns>作业日志详情</returns>
    public DbLogJob GetJobLogDetail(long logId)
    {
        var resp = new BaseRepository<DbLogJob>();
        return resp.GetById(logId);
    }

    /// <summary>
    /// 获取作业当前运行状态
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <returns>作业运行状态</returns>
    public EnumJobStatus GetJobCurrentStatus(string jobName)
    {
        try
        {
            var resp = new BaseRepository<DbLogJob>();
            var latestJob = resp.AsQueryable()
                   .Where(x => x.Name == jobName)
                   .OrderByDescending(x => x.StartTime)
                   .First();
            return latestJob.Status;
        }
        catch
        {
            return EnumJobStatus.NotRunning;
        }
    }

    /// <summary>
    /// 删除作业日志
    /// </summary>
    /// <param name="jobName">作业名称（可选，为空则删除所有日志）</param>
    /// <returns>删除的日志数量</returns>
    public bool DeleteJobLogs(string? jobName = null)
    {
        var resp = new BaseRepository<DbLogJob>();
        if (string.IsNullOrEmpty(jobName))
        {
            return resp.Delete(q => q.Id > 0);
        }
        return resp.Delete(q => q.Name == jobName);
    }
}
