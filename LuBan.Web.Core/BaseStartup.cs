/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core
*文件名： BaseStartup
*版本号： V1.0.0.0
*唯一标识：42618885-1eab-41dd-9736-3c36fca53e8b
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/8 13:03:34
*描述：aspnetcore api启动类1
*
*=====================================================================
*修改标记
*修改时间：2022/7/8 13:03:34
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：aspnetcore api启动类1
*
*****************************************************************************/
namespace LuBan.Web.Core;

/// <summary>
/// aspnetcore api启动类1
/// </summary>
public abstract class BaseStartup
{
    /// <summary>
    /// aspnetcore api启动类
    /// </summary>
    /// <param name="configuration"></param>
    public BaseStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Configuration
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureServices(Configuration);
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Configure(env, null);
    }
}
