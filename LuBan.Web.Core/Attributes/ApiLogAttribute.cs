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
    Stopwatch _stopwacth = Stopwatch.StartNew();

    string _input = string.Empty;

    bool _noLog = false;

    /// <summary>
    /// 执行顺序
    /// </summary>
    public new int Order => 99999;

    /// <summary>
    /// 当方法执行时
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //检查无需记录日志的接口
        if (context.HasAttribute<NoApiLogAttribute>())
        {
            _noLog = true;
        }
        else
        {
            _noLog = false;
            try
            {
                _stopwacth.Restart();
                ServiceProviderUtil.GetExceptionScope().Exception = null;

                //记录IFile上传文件的参数
                bool isFile = false;
                if (context.HttpContext.Request.HasFormContentType)
                {
                    var files = context.HttpContext.Request.Form?.Files ?? null;
                    if (files != null && files.Count > 0)
                    {
                        isFile = true;
                        _input = files.Select(q => new { q.Name, q.FileName, q.ContentType, q.Length }).ToJson() ?? "文件";
                    }
                    else
                    {
                        isFile = false;
                    }
                }

                //base64上传文件另外处理
                var args = context.ActionArguments;
                if (args! != null && args.Count > 0)
                    foreach (var arg in args)
                    {
                        if (!isFile)
                        {
                            if (!string.IsNullOrEmpty(arg.Key) && arg.Key.IndexOf("base64") > -1)
                            {
                                isFile = true;
                                _input = "base64 文件";
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                //记录请求参数
                if (!isFile)
                {
                    if (args != null && args.Count > 0)
                    {
                        //排除不绑定模型的上传文件接口
                        if (context.HttpContext.Request.Path.Value.IndexOf("/api/ExtraFile/Upload", true) < 0)
                        {
                            _input = SerializeUtil.Serialize(args);
                        }
                    }
                    else
                    {
                        var body = await context.HttpContext.GetRequestBodyTextAsync();
                        if (body.IsNotNullOrEmpty())
                        {
                            _input = $"{_input},body={body}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"接口调用日志记录失败", ex);
            }
        }

        try
        {
            await next.Invoke();
        }
        catch (Exception ex)
        {
            ServiceProviderUtil.GetExceptionScope().Exception = ex;
        }
    }

    /// <summary>
    /// 当方法执行后,
    /// 若有异常则不进处后续处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
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
        _stopwacth.Stop();

        try
        {
            //叵不需要记录日志
            if (_noLog) return;

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
                host = host.Replace(":80", "");
            }

            if (host.EndsWith(":443"))
            {
                host = host.Replace(":443", "");
            }

            if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues values) && values.Count > 0)
            {
                var prefix = values.FirstOrDefault();
                if (prefix.IsNotNullOrEmpty())
                {
                    host = host + "/" + prefix;
                }
            }

            var url = $"https://{host}{context.HttpContext.Request.Path}{(context.HttpContext.Request.QueryString.HasValue ? context.HttpContext.Request.QueryString.Value : "")}";

            if (exception == null)
                exception = ServiceProviderUtil.GetExceptionScope()?.Exception ?? null;

            if (_input.IsNotNullOrEmpty() && _input.Length > 10240)
            {
                _input = _input.Substring(0, 10240);
            }

            Logger.ApiCallLog(context.HttpContext.TraceIdentifier,
                $"{context.HttpContext.GetClientIp()}:{context.HttpContext.Connection.RemotePort}",
                url,
                context.HttpContext.Request.Method,
                SerializeUtil.Serialize(context.HttpContext.Request.Headers),
                _input,
                _stopwacth.ElapsedMilliseconds,
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