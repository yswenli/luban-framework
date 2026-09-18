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
*修改时间：2026/09/11 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志记录迁移至 ApiLogMiddleware，此处仅保留异常处理
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 接口调用日志,可使用NoApiLogAttribute移除。
/// 日志记录已迁移至 ApiLogMiddleware，此处仅保留异常处理。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ApiLogAttribute : BaseFilterAttribute, IAsyncActionFilter, IAsyncExceptionFilter, IAsyncResultFilter, IOrderedFilter
{
    public new int Order => 99999;

    /// <summary>
    /// 执行前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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
    /// 判断请求体是否需要被日志读取（文本类且非 multipart）。
    /// 仅在需要读取时才启用缓冲：multipart/二进制大文件（如上传）不应被整体缓冲到内存/磁盘。
    /// </summary>
    internal static bool ShouldBufferRequestBody(HttpContext httpContext)
    {
        var request = httpContext.Request;
        var contentType = request.ContentType;
        //multipart可能包含文件二进制内容，日志不读原文，无需缓冲
        if (contentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) == true)
        {
            return false;
        }
        //非文本类型（如application/octet-stream）日志只记录元信息，无需缓冲
        if (!request.HasFormContentType && !IsTextualContentType(contentType))
        {
            return false;
        }
        return true;
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
        if (context.HttpContext.Response.HasStarted)
        {
            context.ExceptionHandled = true;
            return;
        }

        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

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
        }

        context.ExceptionHandled = true;
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
    }
}


/// <summary>
/// 不记录api日志
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NoApiLogAttribute : Attribute
{

}