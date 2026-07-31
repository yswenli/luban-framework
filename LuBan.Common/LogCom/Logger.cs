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
using System.Text.Json.Nodes;

namespace System;

/// <summary>
/// LuBan.Framework 日志组件。
/// </summary>
public static class Logger
{
    private static ILogger _loginfo = NullLogger.Instance;
    private static ILogger _logdebug = NullLogger.Instance;
    private static ILogger _logwarn = NullLogger.Instance;
    private static ILogger _logerror = NullLogger.Instance;
    private static ILogger _logcall = NullLogger.Instance;
    private static Func<object, string> _serializer = obj => obj?.ToString() ?? "";
    private static readonly object _gate = new();

    public static event Action<LogInfo> OnLogged;

    public static event Action<ApiLogInfo> OnCalled;

    public static event Action<LogInfo> OnError;

    public static event Action<LogInfo> OnDebug;

    /// <summary>
    /// 由 LuBan.Logging 在启动时注入 ILoggerFactory（线程安全）。
    /// 内部按原 log4net 的 5 个 logger name 创建 5 个 category logger。
    /// </summary>
    /// <param name="factory"></param>
    public static void SetLogger(ILoggerFactory factory)
    {
        lock (_gate)
        {
            if (factory == null)
            {
                _loginfo = _logdebug = _logwarn = _logerror = _logcall = NullLogger.Instance;
                return;
            }
            _loginfo = factory.CreateLogger("loginfo");
            _logdebug = factory.CreateLogger("logdebug");
            _logwarn = factory.CreateLogger("logwarn");
            _logerror = factory.CreateLogger("logerror");
            _logcall = factory.CreateLogger("logcall");
        }
    }

    /// <summary>
    /// 由 LuBan.Logging 在启动时注入 STJ 序列化器（线程安全）。
    /// </summary>
    /// <param name="serializer"></param>
    public static void SetSerializer(Func<object, string> serializer)
    {
        lock (_gate)
        {
            _serializer = serializer ?? (obj => obj?.ToString() ?? "");
        }
    }

    /// <summary>
    /// 设置控制台输出（保留兼容性，目前为空实现）。
    /// </summary>
    public static void SetConsoleAppender()
    {
    }

    /// <summary>
    /// 记录日志。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="des"></param>
    /// <param name="console"></param>
    /// <param name="params"></param>
    public static void Info(string name, string des, ConsoleColor console, params object[] @params)
    {
        LogInfo logInfo;
        try
        {
            logInfo = new LogInfo
            {
                Description = des,
                Exception = null,
                Params = @params
            };
            if (name.IsNotNullOrEmpty())
            {
                logInfo.Description = name + "\t" + des;
            }
        }
        catch
        {
            return;
        }

        if (_loginfo is NullLogger)
        {
            try { OnLogged?.Invoke(logInfo); } catch { }
            return;
        }

        try
        {
            string text = _serializer(logInfo);
            _loginfo.LogInformation(text);
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
    /// 记录日志。
    /// </summary>
    /// <param name="msg"></param>
    public static void Info(string msg)
    {
        Info(msg, []);
    }

    /// <summary>
    /// 记录日志。
    /// </summary>
    /// <param name="des"></param>
    /// <param name="params"></param>
    public static void Info(string des, params object[] @params)
    {
        Info("", des, ConsoleColor.White, @params);
    }

    /// <summary>
    /// 记录日志。
    /// </summary>
    /// <param name="des"></param>
    /// <param name="consoleColor"></param>
    /// <param name="params"></param>
    public static void Info(string des, ConsoleColor consoleColor, params object[] @params)
    {
        Info("", des, consoleColor, @params);
    }

    /// <summary>
    /// 记录调试日志。
    /// </summary>
    /// <param name="enableDebug"></param>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Debug(bool enableDebug, string description, Exception? ex = null, params object[] @params)
    {
        if (enableDebug)
        {
            LogInfo obj;
            try
            {
                obj = new LogInfo
                {
                    Description = description,
                    Exception = ex,
                    Params = @params,
                    EnableDebug = enableDebug
                };
            }
            catch
            {
                return;
            }

            if (_logdebug is NullLogger)
            {
                try { OnDebug?.Invoke(obj); } catch { }
                return;
            }

            try
            {
                string text = _serializer(obj);
                _logdebug.LogDebug(text);
                text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.DarkYellow);
            }
            catch
            {
            }

            try
            {
                OnDebug?.Invoke(obj);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// 记录调试日志。
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Debug(string description, Exception? ex = null, params object[] @params)
    {
        Debug(NacosConfigUtil.EnabelDebug, description, ex, @params);
    }

    /// <summary>
    /// 记录调试日志。
    /// </summary>
    /// <param name="enableDebug"></param>
    /// <param name="description"></param>
    /// <param name="params"></param>
    public static void Debug(bool enableDebug, string description, params object[] @params)
    {
        if (enableDebug)
        {
            LogInfo obj;
            try
            {
                obj = new LogInfo
                {
                    Description = description,
                    Params = @params,
                    EnableDebug = enableDebug
                };
            }
            catch
            {
                return;
            }

            if (_logdebug is NullLogger)
            {
                try { OnDebug?.Invoke(obj); } catch { }
                return;
            }

            try
            {
                string text = _serializer(obj);
                _logdebug.LogDebug(text);
                text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.DarkYellow);
            }
            catch
            {
            }

            try
            {
                OnDebug?.Invoke(obj);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// 记录调试日志。
    /// </summary>
    /// <param name="description"></param>
    /// <param name="params"></param>
    public static void Debug(string description, params object[] @params)
    {
        Debug(NacosConfigUtil.EnabelDebug, description, @params);
    }

    /// <summary>
    /// 记录警告信息。
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Warn(string description, Exception? ex = null, params object[] @params)
    {
        LogInfo obj;
        try
        {
            obj = new LogInfo
            {
                Description = description,
                Exception = ex,
                Params = @params
            };
        }
        catch
        {
            return;
        }

        if (_logwarn is NullLogger)
        {
            try { OnLogged?.Invoke(obj); } catch { }
            return;
        }

        try
        {
            string text = _serializer(obj);
            _logwarn.LogWarning(text);
            text.WriteLine(withTime: true, "HH:mm:ss.fff", ConsoleColor.DarkYellow);
        }
        catch
        {
        }

        try
        {
            OnLogged?.Invoke(obj);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录异常信息。
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Error(string description, Exception ex, params object[] @params)
    {
        Error(1, description, ex, @params);
    }

    /// <summary>
    /// 记录异常信息。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="msg"></param>
    public static void Error(string name, string msg)
    {
        Error(1, name, new Exception(msg), string.Empty);
    }

    /// <summary>
    /// 记录异常信息。
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
    /// 记录异常信息。
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
    /// 记录异常信息。
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
    /// 记录异常信息。
    /// </summary>
    /// <param name="level"></param>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void Error(int level, string description, Exception ex, params object[] @params)
    {
        LogInfo obj;
        try
        {
            obj = new LogInfo
            {
                Description = description,
                Exception = ex,
                Params = @params,
                Level = level
            };
        }
        catch
        {
            return;
        }

        if (_logerror is NullLogger)
        {
            try { OnError?.Invoke(obj); } catch { }
            return;
        }

        try
        {
            string text = _serializer(obj);
            if (text.IsNotNullOrEmpty())
            {
                _logerror.LogError(text);
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
    /// 记录异常信息,但不触发事件。
    /// </summary>
    /// <param name="description"></param>
    /// <param name="ex"></param>
    /// <param name="params"></param>
    public static void ErrorWithOutEvent(string description, Exception ex, params object[] @params)
    {
        LogInfo obj;
        try
        {
            obj = new LogInfo
            {
                Description = description,
                Exception = ex,
                Params = @params,
                Level = 1
            };
        }
        catch
        {
            return;
        }

        if (_logerror is NullLogger) return;

        try
        {
            string text = _serializer(obj);
            _logerror.LogError(text);
            text.WriteLine(color: ConsoleColor.Red);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录异常信息,但不触发事件(不入库)。
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
    /// 记录API调用日志。
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
        ApiLogInfo apiLogModel;
        try
        {
            string userAgent = "";
            if (!string.IsNullOrEmpty(header))
            {
                var jObject = JsonNode.Parse(header);
                userAgent = jObject?["User-Agent"]?.GetValue<string>() ?? "";
            }

            apiLogModel = new ApiLogInfo
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
            };
        }
        catch
        {
            return;
        }

        ApiCallLog(apiLogModel);
    }

    /// <summary>
    /// 记录API调用日志。
    /// </summary>
    /// <param name="apiLogModel"></param>
    public static void ApiCallLog(ApiLogInfo apiLogModel)
    {
        if (_logcall is NullLogger)
        {
            try
            {
                if (apiLogModel.Exception == null)
                    OnCalled?.Invoke(apiLogModel);
                else
                    OnError?.Invoke(apiLogModel);
            }
            catch { }
            return;
        }

        try
        {
            var text = _serializer(apiLogModel);
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
                _logcall.LogInformation(text);
            }
        }
        catch
        {
        }

        try
        {
            if (apiLogModel.Exception == null)
                OnCalled?.Invoke(apiLogModel);
            else
                OnError?.Invoke(apiLogModel);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 记录API调用日志。
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
        ApiLogInfo apiLogModel;
        try
        {
            string userAgent = "";
            if (header.IsNotNullOrEmpty())
            {
                var jObject = JsonNode.Parse(header);
                userAgent = jObject?["User-Agent"]?.GetValue<string>() ?? "";
            }

            apiLogModel = new ApiLogInfo
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
            };
        }
        catch
        {
            return;
        }

        ApiCallLog(apiLogModel);
    }
}
