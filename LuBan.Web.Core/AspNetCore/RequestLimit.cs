/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： RequestLimit
*版本号： V1.0.0.0
*唯一标识：df844fa9-297d-4318-99d6-f84a22e12052
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/27 18:53:39
*描述：请求限制类
*
*=================================================
*修改标记
*修改时间：2023/12/27 18:53:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：请求限制类
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 请求限制类
/// </summary>
public static class RequestLimit
{
    static readonly UploadOptions _uploadOptions;

    /// <summary>
    /// 请求限制类
    /// </summary>
    static RequestLimit()
    {
        _uploadOptions = NacosConfigUtil.Read<UploadOptions>() ?? throw new Exception("读取上传配置失败");
    }

    /// <summary>
    /// 配置请求大小限制,
    /// 设置接收文件长度的最大值,另外需要在具体的action方法上添加:
    //[RequestSizeLimit()],
    //[DisableRequestSizeLimit]
    /// </summary>
    /// <param name="services"></param>
    /// <param name="hostingOptions"></param>
    public static void ConfigServerHost(this IServiceCollection services, HostingOptions hostingOptions)
    {
        ConsoleUtil.WriteLineWithCount("正在配置http宿主级请求限制", color: ConsoleColor.Green);
        //主机级
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = hostingOptions.AppOptions.MaxRequestSize;
            options.AddServerHeader = false;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);

            var urls = hostingOptions.AppOptions.Urls;
            foreach (var url in urls)
            {
                var port = 5000;
                if (int.TryParse(url.Split(":")[1], out port))
                {
                    options.ListenAnyIP(port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                        if (url.StartsWith("https"))
                            listenOptions.UseHttps();
                    });
                }
            }
        });
        //services.Configure<HttpSysOptions>();        

        //配置表单选项（包括单个文件大小等）
        services.Configure<FormOptions>(x =>
        {
            x.MultipartBodyLengthLimit = _uploadOptions.MaxSize;
            x.BufferBody = false;
            x.MemoryBufferThreshold = _uploadOptions.MemoryBufferThreshold;
            x.ValueLengthLimit = (int)_uploadOptions.MaxSize;
            x.MultipartHeadersLengthLimit = 64 * 1024;
        });
    }


    /// <summary>
    /// 配置服务信息
    /// </summary>
    /// <param name="app"></param>
    public static void SetServerInfo(this HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("WC1TZXJ2ZXI=".ConvertToUTF8Str(), "Q3ljbG9wc0ZyYW1ld29yZFNlcnZlcg==".ConvertToUTF8Str());
        httpContext.Response.Headers.Append("WC1EZXZlbG9wZWQtQnk=".ConvertToUTF8Str(), "eXN3ZW5saQ==".ConvertToUTF8Str());
    }

    /// <summary>
    /// 是否启用大文件的分段获取
    /// </summary>
    /// <param name="app"></param>
    /// <param name="hostingOptions"></param>
    /// <returns></returns>
    public static IApplicationBuilder SetEnablePartialRequest(this IApplicationBuilder app, HostingOptions hostingOptions)
    {
        //是否启用大文件的分段获取
        if (!hostingOptions.AppOptions.EnablePartialRequest)
        {
            return app.Use(async (context, next) =>
            {
                // 检查请求是否包含Range头部
                if (context.Request.Headers.ContainsKey("Range"))
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Scope request not supported");
                }
                else
                {
                    await next();
                }
            });
        }
        return app;
    }

    /// <summary>
    /// 配置服务信息
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder SetServerInfo(this IApplicationBuilder app)
    {
        return app.Use(next => context =>
        {
            var delete = next(context);
            context.SetServerInfo();
            return delete;
        });
    }

    /// <summary>
    /// 配置区域和hub
    /// </summary>
    /// <param name="app"></param>
    /// <param name="hostingOptions"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IApplicationBuilder SetEndPoints(this IApplicationBuilder app, HostingOptions hostingOptions)
    {
        return app.UseEndpoints(endpoints =>
        {
            //支持控制器
            endpoints.MapControllers();

            //支持默认路由
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //支持razor页面
            endpoints.MapRazorPages();

            //支持signalr hub
            if (hostingOptions.AppOptions.EnableSignalR)
            {
                if (hostingOptions.AppOptions.SignalROptions == null)
                {
                    throw new NotImplementedException("AppOptions.SignalROptions is null");
                }
                if (hostingOptions.AppOptions.SignalROptions.HubUrl.IsNullOrEmpty())
                {
                    endpoints.MapHub<CommonHub>("/hubs/common");
                }
                else
                {
                    endpoints.MapHub<CommonHub>($"{hostingOptions.AppOptions.SignalROptions.HubUrl}");
                }
                app.UseWebSockets();
            }
        });
    }
}
