/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Logs
*文件名： ApiLogAttribute
*版本号： V1.0.0.0
*唯一标识：ec89ef3c-3581-4fb4-8fa7-ffecbf40a694
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 14:02:14
*描述：接口调用日志
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 14:02:14
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：接口调用日志
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 接口调用日志,可使用NoApiLogAttribute移除
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ApiLogAttribute : BaseFilterAttribute, IAsyncActionFilter, IAsyncResultFilter, IOrderedFilter
{
    static readonly AsyncLocal<Stopwatch> _stopwatchLocal = new();
    static readonly AsyncLocal<string> _inputLocal = new();
    static readonly AsyncLocal<bool> _noLogLocal = new();

    public new int Order => 99999;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        _stopwatchLocal.Value = stopwatch;
        _inputLocal.Value = string.Empty;
        _noLogLocal.Value = false;

        if (context.HasAttribute<NoApiLogAttribute>())
        {
            _noLogLocal.Value = true;
        }
        else
        {
            try
            {
                var input = string.Empty;
                bool isFile = false;
                if (context.HttpContext.Request.HasFormContentType)
                {
                    var files = context.HttpContext.Request.Form?.Files ?? null;
                    if (files != null && files.Count > 0)
                    {
                        isFile = true;
                        input = files.Select(q => new { q.Name, q.FileName, q.ContentType, q.Length }).ToJson() ?? "文件";
                    }
                }

                var args = context.ActionArguments;
                if (args! != null && args.Count > 0)
                    foreach (var arg in args)
                    {
                        if (!isFile)
                        {
                            if (!string.IsNullOrEmpty(arg.Key) && arg.Key.IndexOf("base64") > -1)
                            {
                                isFile = true;
                                input = "base64 文件";
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                if (!isFile)
                {
                    if (args != null && args.Count > 0)
                    {
                        if (context.HttpContext.Request.Path.Value.IndexOf("/api/ExtraFile/Upload", true) < 0)
                        {
                            input = SerializeUtil.Serialize(args);
                        }
                    }
                    else
                    {
                        var body = await context.HttpContext.GetRequestBodyTextAsync();
                        if (body.IsNotNullOrEmpty())
                        {
                            input = $"{input},body={body}";
                        }
                    }
                }
                _inputLocal.Value = input;
            }
            catch (Exception ex)
            {
                Logger.Warn($"接口调用日志记录失败", ex);
            }
        }

        await next.Invoke();
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        Exception? exception = null;
        try
        {
            await next.Invoke();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        _stopwatchLocal.Value?.Stop();

        try
        {
            if (_noLogLocal.Value) return;

            var result = string.Empty;

            if (context.Result != null)
            {
                result = context.GetResultLogText();
                if (result.IsNotNullOrEmpty() && result.Length > 10240)
                {
                    result = result.Substring(0, 10240);
                }
            }

            long userId = 0;

            if (SessionUser.UserId > 0)
            {
                userId = SessionUser.UserId;
            }

            var host = context.HttpContext.Request.Host.Value;

            if (host.EndsWith(":80"))
            {
                host = host[..^3];
            }

            if (host.EndsWith(":443"))
            {
                host = host[..^4];
            }

            if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues values) && values.Count > 0)
            {
                var prefix = values.FirstOrDefault();
                if (prefix.IsNotNullOrEmpty())
                {
                    host = host + "/" + prefix;
                }
            }

            var url = $"{context.HttpContext.Request.Scheme}://{host}{context.HttpContext.Request.Path}{(context.HttpContext.Request.QueryString.HasValue ? context.HttpContext.Request.QueryString.Value : "")}";

            if (exception == null)
                exception = ServiceProviderUtil.GetExceptionScope()?.Exception ?? null;

            var input = _inputLocal.Value ?? string.Empty;
            if (input.IsNotNullOrEmpty() && input.Length > 10240)
            {
                input = input.Substring(0, 10240);
            }

            Logger.ApiCallLog(context.HttpContext.TraceIdentifier,
                $"{context.HttpContext.GetClientIp()}:{context.HttpContext.Connection.RemotePort}",
                url,
                context.HttpContext.Request.Method,
                SerializeUtil.Serialize(context.HttpContext.Request.Headers),
                input,
                _stopwatchLocal.Value?.ElapsedMilliseconds ?? 0,
                context.HttpContext.Response.StatusCode,
                result ?? "",
                userId.ToString(),
                exception);
        }
        catch (Exception ex)
        {
            Logger.Warn($"ApiLogAttribute记录日志失败", ex);
        }
    }


}


/// <summary>
/// 不记录api日志
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NoApiLogAttribute : Attribute
{

}