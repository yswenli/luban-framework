/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerMiddleware
*版本号： V1.0.0.0
*唯一标识：058a760a-cf4b-4bdf-a8c6-101c23d7a0de
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/4/23 13:37:38
*描述：swagger配置相关中间件
*
*=====================================================================
*修改标记
*修改时间：2021/4/23 13:37:38
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：swagger配置相关中间件
*
*****************************************************************************/

namespace LuBan.Web.Core.Swagger;

/// <summary>
/// swagger配置相关中间件
/// </summary>
public static class SwaggerMiddlewareExtentions
{
    /// <summary>
    /// 自定义swagger ui界面
    /// </summary>
    /// <param name="app"></param>
    /// <param name="hostingOptions"></param>
    public static void UseCustomerSwaggerUI(this IApplicationBuilder app, HostingOptions hostingOptions)
    {
        if (hostingOptions.AppOptions.DisableSwagger)
        {
            return;
        }

        app.UseSwagger(c =>
        {
            c.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            c.PreSerializeFilters.Add((doc, httpReq) =>
            {
                var prefix = httpReq.Headers["X-Forwarded-Prefix"];
                doc.Servers?.Clear();
                doc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"/{prefix}" } };
            });
        });

        //swagger默认界面
        //if (hostingOptions.AppOptions.UseKnife4UI)
        //{
        //    //knife4ui界面
        //    app.UseKnife4UI(c =>
        //    {
        //        c.SwaggerEndpoint("swagger.json", hostingOptions.ServiceName);
        //        //限制swagger模型的深度
        //        c.DefaultModelRendering(IGeekFan.AspNetCore.Knife4jUI.ModelRendering.Model);
        //        c.DefaultModelsExpandDepth(1);
        //        c.DocExpansion(IGeekFan.AspNetCore.Knife4jUI.DocExpansion.Full);
        //        //添加自定义javascript
        //        //c.InjectJavascript("");                
        //    });
        //}
        //else
        //{
        app.UseSwaggerUI(c =>
        {
            //swagger分组
            foreach (var group in SwaggerGroupInfo.Default.Values)
            {
                c.SwaggerEndpoint($"{group.GroupName}/swagger.json", group.Title);
            }
            c.RoutePrefix = "swagger";
            c.InjectJavascript("/swagger/zh_CN.js"); // 加载中文包
            c.DocumentTitle = "http-api-文档";
            //设置swagger请求时，默认显示是EditValue还是Schema
            c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
            //显示模型深度，1表示只显示第一级，加快swagger ui的加载速度
            c.DefaultModelExpandDepth(1);
            //限制列表级模型的深度，-1隐藏
            c.DefaultModelsExpandDepth(-1);
            //swagger接口显示时是展开还是折叠
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.EnableDeepLinking();
            c.DisplayRequestDuration();
            //添加自定义javascript
            //c.InjectJavascript("");            
        });

        //}
    }
}
