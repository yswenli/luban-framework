/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： OpenApiAccessAttribute
*版本号： V1.0.0.0
*唯一标识：c373aa8e-7735-425e-a5f8-7417ab42e7c2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/10 10:16:47
*描述：开放接口验证
*
*=================================================
*修改标记
*修改时间：2025/4/10 10:16:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：开放接口验证
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 开放接口jwt验证，
/// 对应无需认证 NoOpenApiAccessAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class OpenApiAccessAttribute : BaseFilterAttribute
{
    /// <summary>
    /// 执行业务前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HasAttribute<NoOpenApiAccessAttribute>())
        {
            await next.Invoke();
            return;
        }
        try
        {
            var jwtConfig = HostingOptions.Default.AppOptions.JwtAuthConfig;
            var token = context.HttpContext.Request.GetJwtTokenString()?.Replace("Bearer ", "") ?? "";
            var data = JWTPackage<JwtUserInfo>.Parse(token, HostingOptions.Default.AppOptions.JwtAuthConfig.Secret).Payload.Data;
            if (data == null)
                throw FriendlyError.Ex("Unauthorized", EnumErrorCode.D1011, 401);
        }
        catch
        {
            throw FriendlyError.Ex("Unauthorized", EnumErrorCode.D1011, 401);
        }
        await next.Invoke();
    }

    /// <summary>
    /// 执行业务后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }
}



