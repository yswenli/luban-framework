/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Logs
*文件名： Logger
*版本号： V1.0.0.0
*唯一标识：02884b7f-495c-432f-b914-e8b287a11395
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/4/8 18:09:33
*描述：LuBan.Framework 日志组件
*
*=================================================
*修改标记
*修改时间：2023/4/8 18:09:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan.Framework 日志组件
*
*****************************************************************************/

namespace System;

/// <summary>
/// LuBan.Framework 日志组件
/// </summary>
public static class Logger
{
    private static readonly ILog _loginfo;

    private static readonly ILog _logdebug;

    private static readonly ILog _logwarn;

    private static readonly ILog _logerror;

    private static readonly ILog _logcall;

    public static event Action<LogInfo> OnLogged;

    public static event Action<ApiLogInfo> OnCalled;

    public static event Action<LogInfo> OnError;

    public static event Action<LogInfo> OnDebug;

    /// <summary>
    /// 日志信息
    /// </summary>
    static Logger()
    {
        XmlConfigurator.Configure(new FileInfo(PathUtil.GetRootFullName("log4net.config")));
        _loginfo = LogManager.GetLogger("loginfo");
        _logdebug = LogManager.GetLogger("logdebug");
        _logwarn = LogManager.GetLogger("logwarn");
        _logerror = LogManager.GetLogger("logerror");
        _logcall = LogManager.GetLogger("logcall");
    }
    /// <summary>
    /// 设置控制台输出
    /// </summary>
    public static void SetConsoleAppender()
    {
        PatternLayout layout = new("%m%n");
        ConsoleAppender appender = new()
        {
            Layout = layout
        };
        BasicConfigurator.Configure(appender);
    }


    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="name"></param>
    /// <param name="des"></param>
    /// <param name="console"></param>
    /// <param name="params"></param>
    public static void Info(string name, string des, ConsoleColor console, params object[] @params)
    {
        var logInfo = new LogInfo
        {
            Description = des,
            Exception = null,
            Params = @params
        };
        if (name.IsNotNullOrEmpty())
        {
            logInfo.Description = name + "\t" + des;
        }

        try
        {
            string text = SerializeUtil.Serialize(logInfo, true, false, true, true);
            _loginfo.Info(text);
            des.WriteLine(color: console);
        }
        catch
        {
        }

        try
        {
            OnLogged?.Invoke(logInfo);
        }
        catch
        {
        }
    }


    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="msg"></param>
    public static void Info(string msg)
    {
        Info(msg, []);
    }


    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="des"></param>
    /// <param name="params"></param>
    public static void Info(string des, params object[] @params)
    {
        Info("", des, ConsoleColor.White, @params);
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="des"></param>
    /// <param name="consoleColor"></param>
    /// <param name="params"></param>
    public static void Info(string des, ConsoleColor consoleColor, params object[] @params)
    {
        Info("", des, consoleColor, @params);
    }

    /// <summary>
    /// 记录调试日志
    /// </summary>
    /// <param name="enableDebug"></param>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Debug(bool enableDebug, string description, Exception? ex = null, params object[] @params)
    {
        if (enableDebug)
        {
            LogInfo obj = new LogInfo
            {
                Description = description,
                Exception = ex,
                Params = @params,
                EnableDebug = enableDebug
            };
            try
            {
                string text = SerializeUtil.Serialize(obj, true, false, true, true);
                _logdebug.Info(text);
                text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.DarkYellow);
            }
            catch
            {
            }

            try
            {
                Logger.OnDebug?.Invoke(obj);
            }
            catch
            {
            }
        }
    }
    /// <summary>
    /// 记录调试日志
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Debug(string description, Exception? ex = null, params object[] @params)
    {
        Debug(NacosConfigUtil.EnabelDebug, description, ex, @params);
    }
    /// <summary>
    /// 记录调试日志
    /// </summary>
    /// <param name="enableDebug"></param>
    /// <param name="description"></param>
    /// <param name="params"></param>
    public static void Debug(bool enableDebug, string description, params object[] @params)
    {
        if (enableDebug)
        {
            LogInfo obj = new LogInfo
            {
                Description = description,
                Params = @params,
                EnableDebug = enableDebug
            };
            try
            {
                string text = SerializeUtil.Serialize(obj, true, false, true, true);
                _logdebug.Info(text);
                text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.DarkYellow);
            }
            catch
            {
            }

            try
            {
                Logger.OnDebug?.Invoke(obj);
            }
            catch
            {
            }
        }
    }
    /// <summary>
    /// 记录调试日志
    /// </summary>
    /// <param name="description"></param>
    /// <param name="params"></param>
    public static void Debug(string description, params object[] @params)
    {
        Debug(NacosConfigUtil.EnabelDebug, description, @params);
    }
    /// <summary>
    /// 记录警告信息
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Warn(string description, Exception? ex = null, params object[] @params)
    {
        var obj = new LogInfo
        {
            Description = description,
            Exception = ex,
            Params = @params
        };
        try
        {
            string text = SerializeUtil.Serialize(obj, true, false, true, true);
            _logwarn.Info(text);
            text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.DarkYellow);
        }
        catch
        {
        }

        try
        {
            Logger.OnLogged?.Invoke(obj);
        }
        catch
        {
        }
    }
    /// <summary>
    /// 记录异常信息
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Error(string description, Exception ex, params object[] @params)
    {
        Error(1, description, ex, @params);
    }
    /// <summary>
    /// 记录异常信息
    /// </summary>
    /// <param name="name"></param>
    /// <param name="msg"></param>
    public static void Error(string name, string msg)
    {
        Error(1, name, new Exception(msg), string.Empty);
    }

    /// <summary>
    /// 记录异常信息
    /// </summary>
    /// <param name="ex"></param>
    public static void Error(Exception ex)
    {
        try
        {
            var tuple = ReflectionUtil.GetCurrentMethodFullName();
            if (tuple != null)
            {
                Error($"{tuple.Item1}.{tuple.Item2}", ex, tuple.Item3);
            }
            else
            {
                Error("异常信息", ex);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录异常信息
    /// </summary>
    /// <param name="error"></param>
    public static void Error(string error)
    {
        try
        {
            var ex = new Exception(error);
            var tuple = ReflectionUtil.GetCurrentMethodFullName();
            if (tuple != null)
            {
                Error($"{tuple.Item1}.{tuple.Item2}", ex, tuple.Item3);
            }
            else
            {
                Error("异常信息", ex);
            }
        }
        catch
        {
        }
    }
    /// <summary>
    /// 记录异常信息
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Error(Exception ex, params object[] @params)
    {
        try
        {
            string description = "";
            var tuple = ReflectionUtil.GetCurrentMethodFullName();
            if (tuple != null)
            {
                description = tuple.Item1 + "." + tuple.Item2;
            }

            Error(description, ex, @params);
        }
        catch
        {
        }
    }
    /// <summary>
    /// 记录异常信息
    /// </summary>
    /// <param name="level"></param>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Error(int level, string description, Exception ex, params object[] @params)
    {
        var obj = new LogInfo
        {
            Description = description,
            Exception = ex,
            Params = @params,
            Level = level
        };
        try
        {
            string text = SerializeUtil.Serialize(obj, true, false, true, true);
            if (text.IsNotNullOrEmpty())
            {
                _logerror.Info(text);
                text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.Red);
            }
        }
        catch
        {

        }

        try
        {
            OnError?.Invoke(obj);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录异常信息,但不触发事件
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void ErrorWithOutEvent(string description, Exception ex, params object[] @params)
    {
        LogInfo obj = new LogInfo
        {
            Description = description,
            Exception = ex,
            Params = @params,
            Level = 1
        };
        try
        {
            string text = SerializeUtil.Serialize(obj, true, false, true, true);
            _logerror.Info(text);
            text.WriteLine(color: ConsoleColor.Red);
        }
        catch
        {
        }
    }
    /// <summary>
    /// 记录异常信息,但不触发事件(不入库)
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void ErrorWithOutEvent(Exception ex, params object[] @params)
    {
        try
        {
            string description = "";
            Tuple<string, string, ParameterInfo[]>? currentMethodFullName = ReflectionUtil.GetCurrentMethodFullName();
            if (currentMethodFullName != null)
            {
                description = currentMethodFullName.Item1 + "." + currentMethodFullName.Item2;
            }

            ErrorWithOutEvent(description, ex, @params);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录API调用日志
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="ip"></param>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="header"></param>
    /// <param name="input"></param>
    /// <param name="cost"></param>
    /// <param name="statusCode"></param>
    /// <param name="result"></param>
    /// <param name="userID"></param>
    /// <param name="ex"></param>
    public static void ApiCallLog(string traceId, string ip, string url, string method, string header, string input, long cost, int statusCode, string result, string userID, Exception? ex)
    {
        string userAgent = "";
        try
        {
            if (!string.IsNullOrEmpty(header))
            {
                JObject jObject = JObject.Parse(header);
                userAgent = jObject.GetValue("User-Agent")?.ToString() ?? "";
            }
        }
        catch
        {
        }

        ApiCallLog(new ApiLogInfo
        {
            TraceId = traceId,
            CallIp = ip,
            Url = url,
            RequestMethod = method,
            Header = header,
            UserAgent = userAgent,
            Input = input,
            Cost = cost,
            StatusCode = statusCode,
            Output = result,
            UserID = userID,
            Exception = ex
        });
    }
    /// <summary>
    /// 记录API调用日志
    /// </summary>
    /// <param name="apiLogModel"></param>
    public static void ApiCallLog(ApiLogInfo apiLogModel)
    {
        try
        {
            var text = apiLogModel.ToJson();
            if (text.IsNotNullOrEmpty())
            {
                if (apiLogModel.Exception == null)
                {
                    text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.Green);
                }
                else
                {
                    text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.Red);
                }
                _logcall.Info(text);
            }
        }
        catch
        {

        }

        try
        {
            if (apiLogModel.Exception == null)
                Logger.OnCalled?.Invoke(apiLogModel);
            else
                Logger.OnError?.Invoke(apiLogModel);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录API调用日志
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="ip"></param>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="header"></param>
    /// <param name="input"></param>
    /// <param name="cost"></param>
    /// <param name="statusCode"></param>
    /// <param name="result"></param>
    /// <param name="userId"></param>
    /// <param name="ex"></param>
    public static void ApiErrorLog(string traceId, string ip, string url, string method, string header, string input, long cost, int statusCode, string result, string userId, Exception ex)
    {
        string userAgent = "";
        try
        {
            if (header.IsNotNullOrEmpty())
            {
                var jObject = JObject.Parse(header);
                userAgent = jObject?.GetValue("User-Agent")?.ToString() ?? "";
            }
        }
        catch
        {
        }

        ApiCallLog(new ApiLogInfo
        {
            TraceId = traceId,
            CallIp = ip,
            Url = url,
            RequestMethod = method,
            Header = header,
            Input = input,
            Cost = cost,
            StatusCode = statusCode,
            Output = result,
            UserID = userId,
            UserAgent = userAgent,
            Exception = ex
        });
    }
}

