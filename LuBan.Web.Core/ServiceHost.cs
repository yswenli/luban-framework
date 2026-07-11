/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core
*文件名： ServiceHost
*版本号： V1.0.0.0
*唯一标识：51df356d-d1e0-416d-a9e7-669c318d1902
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/2 16:33:38
*描述：服务容器
*
*=================================================
*修改标记
*修改时间：2025/7/2 16:33:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：服务容器
*
*****************************************************************************/
namespace LuBan.Web.Core;

/// <summary>
/// 服务容器
/// </summary>
internal static class ServiceHost
{

    static string _serviceName = string.Empty;

    /// <summary>
    /// 容器启动事件
    /// </summary>
    public static event Action? OnStarted = null;
    /// <summary>
    /// 容器关闭事件
    /// </summary>
    public static event Action? OnStopping = null;
    /// <summary>
    /// 容器关闭后事件
    /// </summary>
    public static event Action? OnStopped = null;


    /// <summary>
    /// WebApi辅助角色服务工具类
    /// </summary>
    static ServiceHost()
    {
        _serviceName = HostingOptions.ServiceName;
        ConsoleUtil.SetTitle(_serviceName);
        ConsoleUtil.PrintName();
        ConsoleUtil.IsEnabled = !HostingOptions.AppOptions.HideConsoleLog;
    }

    /// <summary>
    /// 配置
    /// </summary>
    public static HostingOptions HostingOptions
    {
        get
        {
            return HostingOptions.Default;
        }
    }

    /// <summary>
    /// 当前服务配置的名称
    /// </summary>
    public static string ServiceName
    {
        get
        {
            return _serviceName;
        }
    }

    /// <summary>
    /// 启动后打开的url
    /// </summary>
    public static string OpenUrl
    {
        get; set;
    }

    /// <summary>
    /// 是否是windows平台
    /// </summary>
    public static bool IsWinPlantform
    {
        get
        {
            return RuntimeUtil.IsWindows();
        }
    }


    static void Started(string[] args)
    {
        ConsoleUtil.WriteNewLine();
        //检查是否启用视频缩略图功能
        if (HostingOptions.EnableVideoThumbnail == true && VideoUtil.FFmpegPath.IsNullOrEmpty())
        {
            var videoConfigErrorMsg = "视频缩略图功能未能启用，请配置启动参数 --ffmpeg {ffmpeg完整地址}";
            Logger.Error(videoConfigErrorMsg);
        }
        //启动日志收集
        LoggerCollector.Instance.Start();
        ArgumentException.ThrowIfNullOrWhiteSpace(OpenUrl, "OpenUrl");
        ProcessUtil.Navigate($"{OpenUrl.TrimEnd('/')}{HostingOptions.AppOptions.StartPath}");
        Logger.Info($"{_serviceName} Api 服务已启动,接口文档地址: {OpenUrl.TrimEnd('/')}{HostingOptions.AppOptions.StartPath}{Environment.NewLine}", ConsoleColor.Green);
        OnStarted?.Invoke();
        //是否启用后台任务
        if (HostingOptions.EnableBackgroundJob)
        {
            JobServiceLoader.Init(HostingOptions.BackgroundJobNames);
            JobServiceLoader.Start();
            Logger.Info($"{_serviceName} Jobs 服务已启动", ConsoleColor.Green);
        }
        if (ApprovalFlowOptions.Default.AutoApproval == true)
        {
            FlowEngine.Instance.Start();
            Logger.Info("ApprovalFlowEngine 已启动", ConsoleColor.Green);
        }
        ConsoleUtil.WriteLine($"Ctrl+C 安全退出服务{Environment.NewLine}{Environment.NewLine}", color: ConsoleColor.Yellow);
    }

    static void Stopping()
    {
        ConsoleUtil.WriteLine($"{_serviceName} 正在停止...", color: ConsoleColor.Yellow);

        LocalCacheUtil.Close();

        ConsoleUtil.WriteLine($"{_serviceName} 正在保存缓存文件...", color: ConsoleColor.Yellow);

        OnStopping?.Invoke();
    }

    static void Stoped()
    {
        if (ApprovalFlowOptions.Default.AutoApproval == true)
        {
            FlowEngine.Instance.Stop();
            Logger.Info("ApprovalFlowEngine 已停止", ConsoleColor.DarkYellow);
        }
        if (HostingOptions.EnableBackgroundJob)
        {
            JobServiceLoader.Stop();
            Logger.Info($"{_serviceName} Jobs 服务已停止", ConsoleColor.DarkYellow);
        }
        OnStopped?.Invoke();
        Logger.Info($"{_serviceName} Api 服务已停止", ConsoleColor.DarkYellow);
        LoggerCollector.Instance.Stop();
        ConsoleUtil.Close();
    }

    /// <summary>
    /// 创建WebApp
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static WebApplication CreateWebApplication(string[] args)
    {
        //参数示例 --environment env
        EnvironmentUtil.SetEnvironment(args);
        //参数示例 --ffmpeg c:/bin/ffmpeg.exe
        VideoUtil.SetFFmpegPath(args);
        var builder = WebApplication.CreateBuilder(args);
        var service = builder.Services;
        service.ConfigureServices(builder.Configuration);

        //移除ms日志
        service.Remove((s) => s.ImplementationType == typeof(Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider));

        var webApp = builder.Build();
        webApp.Configure(webApp.Environment, null);

        //注册事件        
        webApp.Lifetime.ApplicationStarted.Register(() => Started(args));
        webApp.Lifetime.ApplicationStopping.Register(Stopping);
        webApp.Lifetime.ApplicationStopped.Register(Stoped);

        SetWebAppUrls(webApp, args);

        return webApp;
    }

    /// <summary>
    /// 设置webapp的urls
    /// </summary>
    /// <param name="webApp"></param>
    /// <param name="args"></param>
    static void SetWebAppUrls(WebApplication webApp, string[] args)
    {
        List<string> serviceUrls = [];
        //优先读取运行参数
        if (args != null && args.Length > 0)
        {
            var urls = ArgsUtil.Read<string>(args, "urls");
            if (urls.IsNotNullOrEmpty())
            {
                serviceUrls.AddRange([.. urls.Split([";", ","], StringSplitOptions.RemoveEmptyEntries)]);
            }
        }
        //读取配置文件appsettings.json
        if (serviceUrls.Count < 1) serviceUrls.AddRange([.. HostingOptions.AppOptions.Urls]);
        if (serviceUrls.Count < 1) throw new Exception("请配置启动参数 --urls {urls} 或 appsettings.json 中的 HostingOptions.AppOptions.Urls");
        webApp.Urls.Clear();
        foreach (var item in serviceUrls)
        {
            if (item.IsNullOrEmpty()) continue;
            OpenUrl = item;
            webApp.Urls.Add(item);
        }
    }

    /// <summary>
    /// 创建asp.net core服务容器,
    /// WebAppNet8
    /// </summary>
    /// <param name="args"></param>
    public static void RunWebHost(string[] args, Action? onStartuped = null)
    {
        CreateWebApplication(args).Run();
    }

    /// <summary>
    /// 创建asp.net core服务容器,
    /// WebAppNet8
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task RunWebHostAsync(string[] args, Action? onStartuped = null)
    {
        await CreateWebApplication(args).RunAsync();
    }



    static IHttpContextAccessor _accessor;

    /// <summary>
    /// 配置accessor
    /// </summary>
    /// <param name="accessor"></param>
    internal static void ConfigureAccessor(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// 配置accessor
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    internal static IApplicationBuilder SetAccessor(this IApplicationBuilder app)
    {
        var accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        ConfigureAccessor(accessor);
        return app;
    }


    static AsyncLocal<HttpContext> _httpContextAsyncLocal = new();

    /// <summary>
    /// 获取请求上下文
    /// </summary>
    public static HttpContext? HttpContext
    {
        get
        {
            var httpContext = _httpContextAsyncLocal.Value;
            if (httpContext == null && _accessor != null && _accessor.HttpContext != null)
            {
                httpContext = _accessor.HttpContext;
                if (httpContext != null)
                    _httpContextAsyncLocal.Value = httpContext;
            }
            return httpContext;
        }
    }

}
