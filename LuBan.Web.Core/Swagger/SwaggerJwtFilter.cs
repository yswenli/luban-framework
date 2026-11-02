/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： SwaggerJwtFilter
*版本号： V1.0.0.0
*唯一标识：e8d6e625-9cff-4c41-9986-300e94b5e614
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/26 14:27:51
*描述：自动添加Authorization到Header里面去
*
*=================================================
*修改标记
*修改时间：2024/7/26 14:27:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自动添加Authorization到Header里面去
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// 自动添加Authorization到Header里面去
/// </summary>
public class SwaggerJwtFilter : IOperationFilter
{
    /// <summary>
    /// This adds the "Padlock" icon to the endpoint in swagger
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var declaringType = context.MethodInfo.DeclaringType;
        if (declaringType == null) return;

        var isNotAuth = context.MethodInfo.AttributeHasEffect<AllowAnonymousAttribute, AuthorizeAttribute>();
        if (!isNotAuth)
        {
            isNotAuth = declaringType.AttributeHasEffect<AllowAnonymousAttribute, AuthorizeAttribute>();
        }

        if (isNotAuth) return;

        //添加默认的jwt示例
        //operation.Parameters.Add(new OpenApiParameter()
        //{
        //    Name = "Authorization",
        //    In = ParameterLocation.Header,
        //    Description = "jwt",
        //    Required = true,
        //    Schema = new OpenApiSchema
        //    {
        //        Type = "string",
        //        Default = new OpenApiString("Bearer <your-jwt-token>"),
        //        Title = "Authorization"
        //    }
        //});

        operation!.Responses!.Add("401", new OpenApiResponse { Description = "Unauthorized. 当前请求需要在Header中传入jwt" });
        operation!.Responses!.Add("403", new OpenApiResponse { Description = "Forbidden. 当前请求权限不足" });

    }
}
