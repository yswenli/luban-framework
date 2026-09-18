/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Attributes
*文件名： ApiLogMiddleware
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000008
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/09/11 00:00:00
*描述：接口调用日志中间件
*
*=================================================
*修改标记
*修改时间：2026/09/11 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：接口调用日志中间件
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 接口调用日志中间件，包裹下游管道统一记录所有 HTTP 状态码的 API 日志。
/// 标记 [NoApiLog] 的端点与 WebSocket 请求跳过缓冲与日志记录。
/// </summary>
/// <param name="next"></param>
public class ApiLogMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    private const int ResponseBufferLimit = 64 * 1024;

    private const int LogBodyMaxLength = 10 * 1024;

    /// <summary>
    /// Invoke
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (context.WebSockets.IsWebSocketRequest
            || (endpoint != null && endpoint.Metadata.OfType<NoApiLogAttribute>().Any()))
        {
            await _next(context);
            return;
        }

        //仅当请求体需要被日志读取时才启用缓冲：避免上传等大文件被 EnableBuffering
        //整体缓冲到内存/磁盘（FileBufferingReadStream 超过阈值会落盘），产生额外 IO。
        if (ApiLogAttribute.ShouldBufferRequestBody(context))
        {
            context.Request.EnableBuffering();
        }

        var originalBodyStream = context.Response.Body;
        using var responseStream = new CappedResponseStream(originalBodyStream, ResponseBufferLimit);
        context.Response.Body = responseStream;

        var stopwatch = Stopwatch.StartNew();

        string input;
        try
        {
            input = await ApiLogAttribute.ReadBodyForLogAsync(context);
        }
        catch
        {
            input = string.Empty;
        }

        var url = context.Request.GetRequestUrl();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;

            string output;
            if (responseStream.IsOverflow)
            {
                output = $"[response body not fully captured (streamed or > {ResponseBufferLimit} bytes)]";
            }
            else
            {
                output = Encoding.UTF8.GetString(responseStream.GetBufferedContent());
                if (output.Length > LogBodyMaxLength)
                    output = output[..LogBodyMaxLength];
            }

            if (!responseStream.IsOverflow)
            {
                var buffered = responseStream.GetBufferedContent();
                await originalBodyStream.WriteAsync(buffered);
            }

            context.Response.Body = originalBodyStream;

            long userId = 0;
            try
            {
                userId = SessionUser.UserId;
            }
            catch
            {
            }

            try
            {
                Logger.ApiCallLog(
                    context.TraceIdentifier,
                    $"{context.GetClientIp()}:{context.Connection.RemotePort}",
                    url,
                    context.Request.Method,
                    SerializeUtil.Serialize(context.Request.Headers),
                    input.Length > LogBodyMaxLength ? input[..LogBodyMaxLength] : input,
                    stopwatch.ElapsedMilliseconds,
                    statusCode,
                    output,
                    userId.ToString(),
                    null);
            }
            catch
            {
            }
        }
    }
}

/// <summary>
/// 接口调用日志中间件扩展
/// </summary>
public static class ApiLogMiddlewareExtensions
{
    /// <summary>
    /// 使用接口调用日志中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiLogMiddleware>();
    }
}