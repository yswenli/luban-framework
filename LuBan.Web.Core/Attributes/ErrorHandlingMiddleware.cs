/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Logs
*文件名： ErrorHandlingMiddleware
*版本号： V1.0.0.0
*唯一标识：5404f4d4-de9e-4d0a-b519-e35f0b304eba
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 14:03:14
*描述：异常处理中间件
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 14:03:14
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：异常处理中间件
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 异常处理中间件
/// </summary>
/// <param name="next"></param>
public class ErrorHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Invoke
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        var userId = 0L;
        var url = string.Empty;
        string input = string.Empty;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (_next != null)
            {
                if (SessionUser.UserId > 0)
                {
                    userId = SessionUser.UserId;
                }
                url = context.Request.GetRequestUrl();
                input = await context.GetRequestBodyTextAsync();
                await _next.Invoke(context);
            }
        }
        catch (Exception ex)
        {
            try
            {
                await HandleExceptionAsync(context, ex, url, input, userId, stopwatch);
            }
            catch { }
        }
    }

    /// <summary>
    /// 异常处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ex"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex,
        string url,
        string input,
        long userId,
        Stopwatch stopwatch)
    {
        if (context == null || ex == null) return;
        //如果是自定义异常，则返回自定义异常信息
        if (ex is FriendlyException friendlyException)
        {
            var message = new Fail(friendlyException.Message, (int)friendlyException.ErrorCode).ToJson();
            if (message.IsNotNullOrEmpty())
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(message);
                stopwatch.Stop();
                return;
            }
        }
        if (ex is Microsoft.AspNetCore.Http.BadHttpRequestException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            stopwatch.Stop();
            Logger.Warn($"BadHttpRequestException: {ex.Message}");
            return;
        }
        //如果是其他异常，则返回默认异常信息
        var result = SerializeUtil.Serialize(new Fail("Server API error, please contact administrator support to resolve this issue.", 999));
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(result);
        stopwatch.Stop();
        try
        {
            Logger.ApiErrorLog(context.TraceIdentifier,
                    $"{context.GetClientIp()}:{context.Connection.RemotePort}",
                    url,
                    context.Request.Method,
                    SerializeUtil.Serialize(context.Request.Headers),
                    input,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode,
                    result,
                    userId.ToString(),
                    ex);
        }
        catch
        {
            Logger.ErrorWithOutEvent(ex);
        }
    }
}

/// <summary>
/// 异常处理中间件
/// </summary>
public static class ErrorHandlingExtends
{
    /// <summary>
    /// 异常处理中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
