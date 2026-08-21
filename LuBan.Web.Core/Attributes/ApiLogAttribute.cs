/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
public class ApiLogAttribute : BaseFilterAttribute, IAsyncActionFilter, IAsyncExceptionFilter, IAsyncResultFilter, IOrderedFilter
{
    //同一请求的action/result/exception过滤器运行在相互独立的异步流中，
    //AsyncLocal无法跨阶段传递（值在result/exception阶段读到null），
    //必须使用HttpContext.Items共享请求级状态。
    const string StopwatchItemKey = "__ApiLogAttribute_Stopwatch";
    const string InputItemKey = "__ApiLogAttribute_Input";
    const string NoLogItemKey = "__ApiLogAttribute_NoLog";
    const string StreamUploadPath = "/api/ExtraFile/Upload";

    public new int Order => 99999;


    /// <summary>
    /// 执行前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var items = context.HttpContext.Items;
        items[StopwatchItemKey] = stopwatch;
        items[InputItemKey] = string.Empty;
        items[NoLogItemKey] = false;

        if (context.HasAttribute<NoApiLogAttribute>())
        {
            items[NoLogItemKey] = true;
        }
        else
        {
            var input = string.Empty;
            try
            {
                bool isFile = false;
                //流式上传接口由MultipartReader自行处理，访问Request.Form会触发整个body的解析与缓冲，直接跳过
                var isStreamUpload = context.HttpContext.Request.Path.Value?.IndexOf(StreamUploadPath, true) >= 0;
                if (!isStreamUpload && context.HttpContext.Request.HasFormContentType)
                {
                    var files = context.HttpContext.Request.Form?.Files ?? null;
                    if (files != null && files.Count > 0)
                    {
                        isFile = true;
                        //ToJson失败时返回空字符串而非null，需显式判空兜底
                        var filesJson = files.Select(q => new { q.Name, q.FileName, q.ContentType, q.Length }).ToJson();
                        input = filesJson.IsNotNullOrEmpty() ? filesJson : "文件";
                    }
                }

                var args = context.ActionArguments;
                if (args! != null && args.Count > 0)
                    foreach (var arg in args)
                    {
                        if (!isFile)
                        {
                            if (!string.IsNullOrEmpty(arg.Key) && arg.Key.IndexOf("base64", StringComparison.OrdinalIgnoreCase) > -1)
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
                        if (!isStreamUpload)
                        {
                            input = SerializeUtil.Serialize(args);
                        }
                    }
                    else
                    {
                        input = await ReadBodyForLogAsync(context.HttpContext);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"接口调用日志记录失败", ex);
                //序列化失败时至少保留参数名，避免input完全丢失
                try
                {
                    var keys = context.ActionArguments?.Keys;
                    if (keys != null && keys.Count > 0)
                    {
                        input = $"args keys: {string.Join(",", keys)}";
                    }
                }
                catch
                {
                }
            }
            items[InputItemKey] = input;
        }

        await next.Invoke();
    }

    /// <summary>
    /// 读取请求body用于日志记录；multipart及二进制内容不读取原文，仅记录元信息
    /// </summary>
    internal static async Task<string> ReadBodyForLogAsync(HttpContext httpContext)
    {
        try
        {
            var request = httpContext.Request;
            var contentType = request.ContentType;
            //multipart可能包含文件二进制内容，不读原文
            if (contentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) == true)
            {
                return $"[multipart: {contentType}, {request.ContentLength?.ToString() ?? "unknown"} bytes]";
            }
            //非文本类型（如application/octet-stream）按二进制处理，避免日志乱码
            if (!request.HasFormContentType && !IsTextualContentType(contentType))
            {
                return $"[binary: {contentType}, {request.ContentLength?.ToString() ?? "unknown"} bytes]";
            }
            var body = await httpContext.GetRequestBodyTextAsync();
            return body.IsNotNullOrEmpty() ? $"body={body}" : string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Warn("读取请求body用于日志记录失败", ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// 判断是否为可安全按文本读取的ContentType
    /// </summary>
    static bool IsTextualContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return true;
        var mime = contentType.Split(';')[0].Trim();
        return mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || mime.Equals("application/json", StringComparison.OrdinalIgnoreCase)
            || mime.Equals("application/xml", StringComparison.OrdinalIgnoreCase)
            || mime.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
            || mime.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 异常处理
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        //异常可能发生在action过滤器之前（如模型绑定失败），此时NoLog标记未写入Items，需直接检查元数据
        var noLog = context.ActionDescriptor.EndpointMetadata.OfType<NoApiLogAttribute>().Any();

        if (context.HttpContext.Response.HasStarted)
        {
            //响应已开始输出，无法改写为标准错误响应；仅记录日志，Result留空由MVC以EmptyResult收尾
            context.ExceptionHandled = true;
            if (!noLog)
            {
                await GetResultLogTextAsync(context.HttpContext, null, context.Exception);
            }
            return;
        }

        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

        Exception? exception = null;

        if (context.Exception is FriendlyException friendlyException)
        {
            var message = new Fail(friendlyException).ToJson();
            context.HttpContext.Response.StatusCode = friendlyException.HttpStatusCode;
            context.Result = new ContentResult
            {
                Content = message,
                ContentType = "application/json; charset=utf-8",
                StatusCode = friendlyException.HttpStatusCode
            };
        }
        else if (context.Exception is Microsoft.AspNetCore.Http.BadHttpRequestException)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            exception = context.Exception;
        }
        else
        {
            var result = SerializeUtil.Serialize(new Fail("Server API error, please contact administrator support to resolve this issue.", 500));
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new ContentResult
            {
                Content = result,
                ContentType = "application/json",
                StatusCode = StatusCodes.Status500InternalServerError
            };
            exception = context.Exception;
        }

        context.ExceptionHandled = true;

        if (noLog)
        {
            return;
        }

        await GetResultLogTextAsync(context.HttpContext, context.Result, exception);
    }

    /// <summary>
    /// 执行后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();

        await GetResultLogTextAsync(context.HttpContext, context.Result, null);
    }


    async Task GetResultLogTextAsync(HttpContext httpContext, IActionResult? actionResult, Exception? exception)
    {
        var stopwatch = httpContext.Items[StopwatchItemKey] as Stopwatch;
        stopwatch?.Stop();

        try
        {
            if (httpContext.Items[NoLogItemKey] is bool noLog && noLog) return;

            var result = string.Empty;

            if (actionResult != null)
            {
                result = actionResult.GetResultLogText(httpContext.Request.Path);
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

            var host = httpContext.Request.Host.Value ?? "";

            if (host.EndsWith(":80"))
            {
                host = host[..^3];
            }

            if (host.EndsWith(":443"))
            {
                host = host[..^4];
            }

            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues values) && values.Count > 0)
            {
                var prefix = values.FirstOrDefault();
                if (prefix.IsNotNullOrEmpty())
                {
                    host = host + "/" + prefix;
                }
            }

            var url = $"{httpContext.Request.Scheme}://{host}{httpContext.Request.Path}{(httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : "")}";

            var input = httpContext.Items[InputItemKey] as string ?? string.Empty;

            //action过滤器未执行（外层过滤器短路）或异常发生在其之前（如模型绑定失败）时，input未捕获，兜底读取body
            if (!httpContext.Items.ContainsKey(InputItemKey))
            {
                input = await ReadBodyForLogAsync(httpContext);
            }

            if (input.IsNotNullOrEmpty() && input.Length > 10240)
            {
                input = input.Substring(0, 10240);
            }

            Logger.ApiCallLog(httpContext.TraceIdentifier,
                $"{httpContext.GetClientIp()}:{httpContext.Connection.RemotePort}",
                url,
                httpContext.Request.Method,
                SerializeUtil.Serialize(httpContext.Request.Headers),
                input,
                stopwatch?.ElapsedMilliseconds ?? 0,
                httpContext.Response.StatusCode,
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