/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.LogLib
*文件名： LoggerCollector
*版本号： V1.0.0.0
*唯一标识：760fb2ce-13b8-45f0-83ac-16260df9089c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/13 17:24:33
*描述：日志收集器
*
*=================================================
*修改标记
*修改时间：2023/12/13 17:24:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志收集器
*
*****************************************************************************/

namespace LuBan.LogLib;


/// <summary>
/// 日志收集器
/// </summary>
public class LoggerCollector : BaseSingleInstance<LoggerCollector>
{
    DbLogCleaner _dbLogCleaner;

    Batcher<LogInfo> _logBatcher;

    Batcher<ApiLogInfo> _apiLogbatcher;

    BaseRepository<DbLogError> _errorLogRepo;

    BaseRepository<DbLogApi> _apiLogInfoRepo;

    readonly string _serviceName = "";

    /// <summary>
    /// 日志收集器
    /// </summary>
    public LoggerCollector()
    {
        _serviceName = ConfigUtil.GetServiceName();

        try
        {
            _errorLogRepo = new BaseRepository<DbLogError>();
            _apiLogInfoRepo = new BaseRepository<DbLogApi>();

        }
        catch (Exception ex)
        {
            throw new Exception("在配置文件中找不到服务名为LogsDB的数据库连接字符串", ex);
        }

        if (!_errorLogRepo.DbConnectionOptions.EnableDbLogs) return;

        _logBatcher = new Batcher<LogInfo>();
        _logBatcher.OnBatched += _batcher1_OnBatched;
        _logBatcher.OnError += _batcher1_OnError;

        _apiLogbatcher = new Batcher<ApiLogInfo>();
        _apiLogbatcher.OnBatched += _batcher4_OnBatched;
        _apiLogbatcher.OnError += _batcher4_OnError;


        _dbLogCleaner = new(_errorLogRepo.DbConnectionOptions.DbLogOptions);
    }


    /// <summary>
    /// 启动日志注册事件
    /// </summary>
    public void Start()
    {
        if (!_errorLogRepo.DbConnectionOptions.EnableDbLogs) return;
        _dbLogCleaner.Start();
        Logger.OnCalled += Logger_OnReceived;
        Logger.OnError += Logger_OnReceived;
    }


    private void Logger_OnReceived(LogInfo obj)
    {
        if (obj != null)
        {
            if (obj is ApiLogInfo aObj)
            {
                _apiLogbatcher.Add(aObj);
            }
            else
            {
                _logBatcher.Add(obj);
            }
        }
    }

    private void _batcher1_OnBatched(List<LogInfo> data)
    {
        if (data != null && data.Count > 0)
        {
            var list = new List<DbLogError>();
            foreach (var item in data)
            {
                var sysLog = new DbLogError()
                {
                    CreateTime = DateTime.Now,
                    CreateUserId = 1,
                    CreateUserName = "System",
                    Description = item.Description,
                    Exception = item.Exception == null ? "" : item.Exception.ToJson(),
                    IsDelete = false,
                    Parmas = item.Params == null ? "" : item.Params.ToJson(),
                    ServiceName = _serviceName
                };
                list.Add(sysLog);
            }
            try
            {
                _errorLogRepo.InsertRange(list);
            }
            catch (Exception ex)
            {
                Logger.Warn("写入异常日志到数据库失败", ex);
            }
        }
    }

    private void _batcher4_OnBatched(List<ApiLogInfo> data)
    {
        if (data != null && data.Count > 0)
        {
            var list = new List<DbLogApi>();
            foreach (var item in data)
            {
                var userAgent = item.UserAgent;
                var device = string.Empty;
                var os = string.Empty;
                string ua = string.Empty;
                if (userAgent.IsNotNullOrEmpty())
                {
                    var deviceInfo = userAgent.GetDeviceInfo();
                    if (deviceInfo != null)
                    {
                        device = deviceInfo.Value.Item1;
                        os = deviceInfo.Value.Item2;
                        ua = deviceInfo.Value.Item3;
                    }
                }
                var heads = item.Header;
                if (heads.IsNotNullOrEmpty() && heads.Length > 2048)
                {
                    heads = heads.Substring(0, 2048);
                }
                var url = item.Url;
                if (url.IsNotNullOrEmpty() && url.Length > 2048)
                {
                    url = url.Substring(0, 2048);
                }
                var sysApiLog = new DbLogApi()
                {
                    CreateTime = DateTime.Now,
                    CreateUserId = 1,
                    CreateUserName = "System",
                    IsDelete = false,
                    ServiceName = _serviceName,
                    CallIp = item.CallIp,
                    Cost = item.Cost,
                    Header = heads,
                    Input = item.Input,
                    Output = item.Output,
                    RequestMethod = item.RequestMethod,
                    StatusCode = item.StatusCode,
                    Url = item.Url,
                    UserAgent = item.UserAgent,
                    UserId = item.UserID,
                    Exception = (item.Exception != null ? item.Exception.ToJson() : null),
                    Device = device,
                    Os = os,
                    Ua = ua
                };
                list.Add(sysApiLog);
            }
            try
            {
                var result = _apiLogInfoRepo.InsertRange(list);
            }
            catch (Exception ex)
            {
                Logger.Warn("写入调用日志到数据库失败", ex);
            }
        }
    }

    private void _batcher1_OnError(Exception obj)
    {
        Logger.ErrorWithOutEvent("LoggerCollector._batcher1_OnError", obj);
    }

    private void _batcher4_OnError(Exception obj)
    {
        Logger.ErrorWithOutEvent("LoggerCollector._batcher4_OnError", obj);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Stop()
    {
        Logger.OnCalled -= Logger_OnReceived;
        Logger.OnError -= Logger_OnReceived;
        _dbLogCleaner.Stop();
    }

}
