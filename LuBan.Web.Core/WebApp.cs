/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：System
*文件名： WebApp
*版本号： V1.0.0.0
*唯一标识：546ec385-1c10-4a32-8fbb-61872973ce6f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 13:57:07
*描述：WebApi辅助角色服务工具类
*
*=================================================
*修改标记
*修改时间：2023/12/4 13:57:07
*修改人： yswenli
*版本号： V1.0.0.0
*描述：WebApi辅助角色服务工具类
*
*****************************************************************************/
namespace System;

/// <summary>
/// WebApi辅助角色服务工具类
/// </summary>
public static partial class WebApp
{
    static string[] _args;

    /// <summary>
    /// WebApi辅助角色服务工具类
    /// </summary>
    static WebApp()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            try
            {
                var msg = $"LuBan.Framework：发生了未处理的异常:{e}";
                Logger.Error(msg);
            }
            catch { }
        };
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

    #region service容器

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
    /// 创建asp.net core服务容器
    /// </summary>
    /// <param name="args"></param>
    public static void RunWebHost(string[] args)
    {
        _args = args;
        if (OnStarted != null)
        {
            ServiceHost.OnStarted += OnStarted;
        }
        if (OnStopping != null)
        {
            ServiceHost.OnStopping += OnStopping;
        }
        if (OnStopped != null)
        {
            ServiceHost.OnStopped += OnStopped;
        }
        ServiceHost.RunWebHost(args);
    }

    /// <summary>
    /// 创建asp.net core服务容器
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task RunWebHostAsync(string[] args)
    {
        _args = args;
        if (OnStarted != null)
        {
            ServiceHost.OnStarted += OnStarted;
        }
        if (OnStopping != null)
        {
            ServiceHost.OnStopping += OnStopping;
        }
        if (OnStopped != null)
        {
            ServiceHost.OnStopped += OnStopped;
        }
        await ServiceHost.RunWebHostAsync(args);
    }

    #endregion

    #region 程序集和类型


    /// <summary>
    /// 读取输入的参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="argsName"></param>
    /// <returns></returns>
    public static T? ReadArgs<T>(string argsName)
    {
        if (_args == null) return default;
        return ArgsUtil.Read<T>(_args, argsName);
    }

    /// <summary>
    /// 程序集
    /// </summary>
    public static List<Assembly>? Assemblies
    {
        get
        {
            return RuntimeUtil.GetAssemblies();
        }
    }
    /// <summary>
    /// 类型
    /// </summary>
    public static List<Type>? Types
    {
        get
        {
            return RuntimeUtil.GetTypes();
        }
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


    /// <summary>
    /// 是否是Linux平台
    /// </summary>
    public static bool IsLinux
    {
        get
        {
            return RuntimeUtil.IsLinux();
        }
    }

    #endregion

    #region aspnetcore

    /// <summary>
    /// 获取http上下文
    /// </summary>
    public static HttpContext? HttpContext
    {
        get
        {
            return ServiceHost.HttpContext;
        }
    }

    /// <summary>
    /// 获取请求jwt上下文数据
    /// </summary>
    /// <remarks>只有授权访问的页面或接口才存在值，否则为 null</remarks>
    public static ClaimsPrincipal User => HttpContext?.User ?? new ClaimsPrincipal();

    /// <summary>
    /// 环境
    /// </summary>
    public static IWebHostEnvironment WebHostEnvironment => ServiceProviderUtil.GetService<IWebHostEnvironment>() ?? throw new InvalidOperationException("初始化环境失败");

    /// <summary>
    /// 获取客户端ip
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string GetClientIp(this HttpContext httpContext)
    {
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (ip.IsNullOrWhiteSpace())
        {
            ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        }
        return ip;
    }

    /// <summary>
    /// 判断请求是否是ajax
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool IsAjaxRequest(this HttpRequest request)
    {
        if (request == null)
            return false;

        if (request.Headers != null && request.Headers.ContainsKey("X-Requested-With"))
        {
            var requestedWith = request.Headers["X-Requested-With"].ToString();
            return (requestedWith == "XMLHttpRequest" || requestedWith == "X-AjaxRequest" || requestedWith == "X-Html5Request");
        }
        return false;
    }

    /// <summary>
    /// 获取请求body的流
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<MemoryStream> GetRequestBodyAsync(this HttpContext context)
    {
        var ms = new MemoryStream();
        if (context.Request.ContentLength > 0 && context.Request.Body != null)
        {
            if (!context.Request.Body.CanSeek)
            {
                context.Request.EnableBuffering();
            }
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            await context.Request.Body.CopyToAsync(ms);
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            ms.Seek(0, SeekOrigin.Begin);
        }
        return ms;
    }

    /// <summary>
    /// 获取请求body的字节数组
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<byte[]> GetRequestBodyBytesAsync(this HttpContext context)
    {
        var ms = await context.GetRequestBodyAsync();
        if (ms == null) return [];
        var bytes = ms.ToArray();
        ms.Dispose();
        return bytes;
    }

    /// <summary>
    /// 获取请求body内容
    /// </summary>
    /// <param name="context"></param>
    /// <param name="maxLength"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static async Task<string> GetRequestBodyTextAsync(this HttpContext context, int maxLength = 64 * 1024, Encoding? encoding = null)
    {
        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }
        var txt = string.Empty;
        if (context.Request.ContentLength > 0 && context.Request.Body != null)
        {
            if (!context.Request.Body.CanSeek)
            {
                context.Request.EnableBuffering();
            }
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            txt = await context.Request.Body.ReadToEndAsync(encoding, maxLength, leaveOpen: true);
            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }
        if (txt.IsNotNullOrEmpty() && txt.Length > maxLength)
        {
            txt = txt[..maxLength];
        }
        return txt;
    }


    /// <summary>
    /// 获取回复body的流
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<MemoryStream> GetResposeBodyAsync(this HttpContext context)
    {
        var ms = new MemoryStream();
        if (context.Response.ContentLength > 0 && context.Response.Body != null)
        {
            if (!context.Response.Body.CanSeek)
            {
                return ms;
            }
            context.Response.Body.Position = 0;
            await context.Response.Body.CopyToAsync(ms);
            context.Response.Body.Position = 0;
            ms.Position = 0;
        }
        return ms;
    }

    /// <summary>
    /// 获取回复body的字节数组
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<byte[]> GetResposeBodyBytesAsync(this HttpContext context)
    {
        var ms = await context.GetResposeBodyAsync();
        if (ms == null) return [];
        ms.Position = 0;
        var bytes = ms.ToArray();
        ms.Dispose();
        return bytes;
    }

    /// <summary>
    /// 获取回复body内容
    /// </summary>
    /// <param name="context"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static async Task<string> GetResposeBodyTextAsync(this HttpContext context, int maxLength = 64 * 1024, Encoding? encoding = null)
    {
        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }
        var txt = string.Empty;
        if (context.Response.ContentLength > 0 && context.Response.Body != null)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            txt = await context.Response.Body.ReadToEndAsync(encoding, maxLength, leaveOpen: true);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
        }
        if (txt.IsNotNullOrEmpty() && txt.Length > maxLength)
        {
            txt = txt[..maxLength];
        }
        return txt;
    }



    /// <summary>
    /// 获取请求的JWT令牌
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetJwtToken(this HttpRequest request)
    {
        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return token.Substring("Bearer ".Length).Trim();
            }
        }
        if (request.Query.TryGetValue("access_token", out var accessToken))
        {
            return accessToken.ToString();
        }
        if (request.Query.TryGetValue("token", out var queryToken))
        {
            return queryToken.ToString();
        }
        return string.Empty;
    }


    /// <summary>
    /// 系统初始化时根据配置在aspnetcore的di中注入的缓存服务
    /// </summary>
    /// <returns></returns>
    public static IServiceCache ServiceCache
    {
        get
        {
            return ServiceProviderUtil.GetRequiredService<IServiceCache>();
        }
    }


    #endregion

    #region  Path

    /// <summary>
    /// 站点根目录
    /// </summary>
    public static string RootPath
    {
        get
        {
            return WebHostEnvironment?.ContentRootPath ?? "";
        }
    }

    /// <summary>
    /// 获取物理路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetPhysicalPath(string path)
    {
        var result = Path.Combine(RootPath, path);
        if (!Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }
        return result;
    }

    /// <summary>
    /// 获取物理路径
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string GetPhysicalPath(params string[] paths)
    {
        var result = Path.Combine(RootPath, Path.Combine(paths));
        if (!Directory.Exists(result))
        {
            Directory.CreateDirectory(result);
        }
        return result;
    }

    /// <summary>
    /// 获取文件物理路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetPhysicalFilePath(string path, string fileName)
    {
        return Path.Combine(Path.Combine(RootPath, path), fileName);
    }

    /// <summary>
    /// 获取文件物理路径
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetPhysicalFilePath(string fileName)
    {
        var fn = Path.GetFileName(fileName);
        var pt = Path.GetDirectoryName(fileName);
        if (pt.IsNotNullOrEmpty())
        {
            var result = Path.Combine(RootPath, pt);
            return Path.Combine(result, fn);
        }
        else
        {
            return Path.Combine(RootPath, fn);
        }
    }

    #endregion
}
