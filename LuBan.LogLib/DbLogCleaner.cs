/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.LogLib
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
namespace LuBan.LogLib;

/// <summary>
/// 日志清理器
/// </summary>
public class DbLogCleaner
{
    bool _stated = false;


    BaseRepository<DbLogApi> _apiLogInfoRepo;

    BaseRepository<DbLogError> _errorLogRepo;

    /// <summary>
    /// 日志配置
    /// </summary>
    public DbLogOptions DbLogOptions { get; private set; }
    /// <summary>
    /// 日志清理器
    /// </summary>
    /// <param name="logOptions"></param>
    public DbLogCleaner(DbLogOptions logOptions)
    {
        DbLogOptions = logOptions;
        _apiLogInfoRepo = new();
        _errorLogRepo = new();
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
            if (DbLogOptions.ApiLogMaxSize > 0)
            {
                var total = _apiLogInfoRepo.Count(q => q.Id > 0);
                if (total > DbLogOptions.ApiLogMaxSize)
                {
                    var deleteCount = total - DbLogOptions.ApiLogMaxSize;
                    var toDeleteIds = _apiLogInfoRepo.OrderBy(q => q.Id)
                        .Take(deleteCount)
                        .Select(q => q.Id)
                        .ToList();
                    _apiLogInfoRepo.Delete(q => toDeleteIds.Contains(q.Id));
                }
            }
            if (DbLogOptions.ApiLogExpiredSeconds > 0)
            {
                var expiredTime = DateTime.Now.AddSeconds(-DbLogOptions.ApiLogExpiredSeconds);
                _apiLogInfoRepo.Delete(q => q.CreateTime < expiredTime);
            }
            if (DbLogOptions.ErrorLogMaxSize > 0)
            {
                var total = _errorLogRepo.Count(q => q.Id > 0);
                if (total > DbLogOptions.ApiLogMaxSize)
                {
                    var deleteCount = total - DbLogOptions.ApiLogMaxSize;
                    var toDeleteIds = _errorLogRepo.OrderBy(q => q.Id)
                        .Take(deleteCount)
                        .Select(q => q.Id)
                        .ToList();
                    _errorLogRepo.Delete(q => toDeleteIds.Contains(q.Id));
                }
            }
            if (DbLogOptions.ApiLogExpiredSeconds > 0)
            {
                var expiredTime = DateTime.Now.AddSeconds(-DbLogOptions.ApiLogExpiredSeconds);
                _errorLogRepo.Delete(q => q.CreateTime < expiredTime);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("数据库日志清理出现异常", ex);
        }
        return !_stated;
    }

}
