/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.Extentions
*文件名： AspNetCoreMiddlewareExtention
*版本号： V1.0.0.0
*唯一标识：f2290534-2277-4fea-929d-381de5abe6fb
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/8 9:34:00
*描述：中间件扩展类
*
*=====================================================================
*修改标记
*修改时间：2022/7/8 9:34:00
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：中间件扩展类
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore.Extentions;

/// <summary>
/// 中间件扩展类
/// </summary>
public sealed class AspNetCoreMiddlewareExtention
{
    RequestDelegate _next;

    Action<HttpContext> _onInvoking;

    Action<HttpContext> _onInvoked;

    /// <summary>
    /// 中间件扩展类
    /// </summary>
    /// <param name="nextDelegate"></param>
    /// <param name="onInvoking"></param>
    /// <param name="onInvoked"></param>
    public AspNetCoreMiddlewareExtention(RequestDelegate nextDelegate,
        Action<HttpContext> onInvoking,
        Action<HttpContext> onInvoked)
    {
        _next = nextDelegate;

        _onInvoking = onInvoking;

        _onInvoked = onInvoked;
    }

    /// <summary>
    /// 处理
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext httpContext)
    {
        _onInvoking?.Invoke(httpContext);

        if (_next != null)
            await _next.Invoke(httpContext);

        _onInvoked?.Invoke(httpContext);
    }
}
/// <summary>
/// 中间件扩展类
/// </summary>
public static class AspNetCoreMiddlewareExtionsService
{
    /// <summary>
    /// 中间件扩展类
    /// </summary>
    /// <param name="app"></param>
    /// <param name="onInvoking"></param>
    /// <param name="onInvoked"></param>
    public static void AddCustomerMiddleware(this IApplicationBuilder app,
        Action<HttpContext> onInvoking,
        Action<HttpContext> onInvoked)
    {
        app.UseMiddleware<AspNetCoreMiddlewareExtention>(onInvoking, onInvoked);
    }
}
