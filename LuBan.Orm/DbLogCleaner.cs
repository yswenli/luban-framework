/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm
*文件名： LogCleaner
*版本号： V1.0.0.0
*唯一标识：86fd92d0-6e45-40a1-b7fe-ffc0e7173238
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/3 14:44:45
*描述：日志清理器
*
*=================================================
*修改标记
*修改时间：2025/12/3 14:44:45
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志清理器
*
*****************************************************************************/
using LuBan.Logging.Configuration;

namespace LuBan.Orm;

/// <summary>
/// 日志清理器
/// </summary>
public class DbLogCleaner
{
    bool _stated = false;

    /// <summary>
    /// 日志配置
    /// </summary>
    public LogLimitConfig LogLimit { get; private set; }
    /// <summary>
    /// 日志清理器
    /// </summary>
    /// <param name="logLimit"></param>
    public DbLogCleaner(LogLimitConfig logLimit)
    {
        LogLimit = logLimit;
    }

    /// <summary>
    /// 开始
    /// </summary>
    public void Start()
    {
        if (!_stated)
        {
            _stated = true;

            ThreadUtil.ThreadWhile(Clear, (int)TimeSpan.FromHours(1).TotalMilliseconds);
        }

    }
    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        if (_stated)
        {
            _stated = false;
        }
    }


    /// <summary>
    /// 清理
    /// </summary>
    /// <returns></returns>
    private bool Clear()
    {
        if (!_stated) return _stated;
        try
        {
            var apiLogRepo = new BaseRepository<DbLogApi>();
            var errorLogRepo = new BaseRepository<DbLogError>();

            if (LogLimit.ApiLogMaxSize > 0)
            {
                var total = apiLogRepo.Count(q => q.Id > 0);
                if (total > LogLimit.ApiLogMaxSize)
                {
                    var deleteCount = total - LogLimit.ApiLogMaxSize;
                    var toDeleteIds = apiLogRepo.OrderBy(q => q.Id)
                        .Take(deleteCount)
                        .Select(q => q.Id)
                        .ToList();
                    apiLogRepo.Delete(q => toDeleteIds.Contains(q.Id));
                }
            }
            if (LogLimit.ApiLogExpiredSeconds > 0)
            {
                var expiredTime = DateTime.Now.AddSeconds(-LogLimit.ApiLogExpiredSeconds);
                apiLogRepo.Delete(q => q.CreateTime < expiredTime);
            }
            if (LogLimit.ErrorLogMaxSize > 0)
            {
                var total = errorLogRepo.Count(q => q.Id > 0);
                if (total > LogLimit.ErrorLogMaxSize)
                {
                    var deleteCount = total - LogLimit.ErrorLogMaxSize;
                    var toDeleteIds = errorLogRepo.OrderBy(q => q.Id)
                        .Take(deleteCount)
                        .Select(q => q.Id)
                        .ToList();
                    errorLogRepo.Delete(q => toDeleteIds.Contains(q.Id));
                }
            }
            if (LogLimit.ErrorLogExpiredSeconds > 0)
            {
                var expiredTime = DateTime.Now.AddSeconds(-LogLimit.ErrorLogExpiredSeconds);
                errorLogRepo.Delete(q => q.CreateTime < expiredTime);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("数据库日志清理出现异常", ex);
        }
        return !_stated;
    }

}
