/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： StartupUtil
*版本号： V1.0.0.0
*唯一标识：ba773eaf-9688-41bc-b9a6-1139cd402004
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 13:45:37
*描述：asp.net core 初始化
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 13:45:37
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：asp.net core 初始化
*
*****************************************************************************/

namespace LuBan.Web.Core.Utils;

/// <summary>
/// asp.net core 初始化
/// </summary>
public static class StartupUtil
{
    static HostingOptions _hostingConfig;

    /// <summary>
    /// asp.net core 初始化
    /// </summary>
    static StartupUtil()
    {
        _hostingConfig = HostingOptions.Default;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            //设置服务提供容器
            services.InitServiceProvider();

            //设置全局读写配置工具
            configuration.InitConfigUtil();

            //设置全局文化信息
            services.SetCurrentCulture();

            //添加配置中心
            services.AddNacosConfigCenterService(configuration, _hostingConfig);

            //全局自动注入di，带InjectionAttribute的IScoped，ISingleton，ITransient
            services.AutoInjectAllCustomerServices();

            //全局自动注入事件总线
            services.AddEventBus(options =>
            {
                options.MaxQueueCapacity = 10000;      // 传送带最多堆多少包裹
                options.EnablePersistence = true;       // 要不要留个底单存档
            });
            //全局自动注入事件处理器
            services.AddEventHandlers();

            //添加mapper对象映射
            services.AddObjectMapper();

            //配置controllers
            services.SetControllers(_hostingConfig);

            //jwt校验
            services.AddJwt(configuration, _hostingConfig);

            //添加swagger.json文件生成
            services.AddSwaggerDocGen(_hostingConfig);

            //配置请求大小限制,
            /// 设置接收文件长度的最大值,另外需要在具体的action方法上添加:
            //[RequestSizeLimit()],
            //[DisableRequestSizeLimit]
            services.ConfigServerHost(_hostingConfig);

            //配置跨域策略
            services.SetDefaultCors();

            //图形验证码
            ConsoleUtil.WriteLineWithCount("正在初始化图形验证码", color: ConsoleColor.Green);
            services.AddCaptcha();

            //配置db
            services.InitDataBaseOrm();

            //配置缓存
            services.AddServiceCache();

            //动态报表处理业务
            services.AddLuBanReporting();

            //正在初始化全局DI集合
            services.BuildProvider();

        }
        catch (Exception ex)
        {
            ConsoleUtil.WriteLine($"An exception occurred during the api initialization of CylopsFramework: {ex}", color: ConsoleColor.Red);
            Thread.Sleep(5000);
            Environment.Exit(-1);
        }
    }


    /// <summary>
    /// 注册中间件
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="onStartuped"></param>
    public static void Configure(this IApplicationBuilder app,
        IWebHostEnvironment env,
        Action<IApplicationBuilder, IWebHostEnvironment>? onStartuped = null)
    {
        try
        {
            //是否启用开发者异常页面
            if (env.IsDevelopment() || env.IsEnvironment("dev"))
            {
                app.UseDeveloperExceptionPage();
            }

            //是否启用大文件的分段获取
            app.SetEnablePartialRequest(_hostingConfig);

            //自定义swagger ui界面
            app.UseCustomerSwaggerUI(_hostingConfig);

            app.UseHsts();

            app.UseRouting();

            //异常捕获
            app.UseErrorHandler();

            //使用默认策略跨域
            app.UseCors();

            //app.UseHttpsRedirection();

            //启用jwt验证
            app.UseAuthentication();

            //在配置中指定静态文件目录
            app.SetStaticFileConfig(env, _hostingConfig);

            //启用内置集成的认证
            app.UseAuthorization();

            //启用在线用户中间件
            app.UseOnlineUserMiddleware();
            
            //数据范围权限
            app.UseDataScopePermission();

            //接口执行前后
            //app.AddCustomerMiddleware(null,null);

            //添加hub
            app.SetEndPoints(_hostingConfig);

            //启用
            app.SetServerInfo();

            //配置全局accessor
            app.SetAccessor();

            //快捷自定义处理
            onStartuped?.Invoke(app, env);
        }

        catch (Exception ex)
        {
            Logger.Error($"CylopsFramework api configure error", ex);
        }
    }
}
