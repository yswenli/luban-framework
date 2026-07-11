/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.Extentions
*文件名： StaticFileMiddlewareExtention
*版本号： V1.0.0.0
*唯一标识：329f17e1-609c-413b-868e-2693f42a9681
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/7 10:08:51
*描述：在配置中指定静态文件目录
*
*=================================================
*修改标记
*修改时间：2024/8/7 10:08:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：在配置中指定静态文件目录
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore.Extentions;

public static class StaticFileMiddlewareExtention
{
    /// <summary>
    /// 在配置中指定静态文件目录
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="hostingOptions"></param>
    /// <returns></returns>
    public static IApplicationBuilder SetStaticFileConfig(this IApplicationBuilder app, IWebHostEnvironment env, HostingOptions hostingOptions)
    {
        //静态目录相关
        if (hostingOptions.AppOptions.UserStaticPath)
        {
            //根目录指向默认wwwroot目录
            app.UseStaticFiles();
            //自定义其它静态目录
            if (hostingOptions.AppOptions.StaticPaths != null && hostingOptions.AppOptions.StaticPaths.Length > 0)
            {
                foreach (var path in hostingOptions.AppOptions.StaticPaths)
                {
                    if (string.IsNullOrEmpty(path) || path == "wwwroot") continue;

                    var staticPath = Path.Combine(env.ContentRootPath, path);

                    PathUtil.Create(staticPath);

                    app.UseStaticFiles(new StaticFileOptions()
                    {
                        FileProvider = new PhysicalFileProvider(staticPath),
                        RequestPath = "/" + path
                    });
                }
            }
            //校验需要登录后的文件访问
            return app.UseStaticFiles(new StaticFileOptions()
            {
                //校验需要登录后的文件访问
                OnPrepareResponse = (c) =>
                {
                    var request = c.Context.Request;
                    var path = request.Path.Value;
                    if (string.IsNullOrEmpty(path)) return;

                    //静态资源访问权限控制
                    if (path.StartsWith("/admin/") && !path.StartsWith("/admin/#/login"))
                    {
                        if (SessionUser.UserId < 1 || SessionUser.CurrentUser == null || SessionUser.CurrentUser.Status != EnumEnableStatus.Enable)
                        {
                            var response = c.Context.Response;
                            response.Clear();
                            response.StatusCode = 401;
                            response.Redirect("/admin/#/login");
                            return;
                        }
                    }
                }
            });
        }
        return app;
    }
}
