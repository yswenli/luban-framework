/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
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
using LuBan.Common.Errors;

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
        // 如果标记了 NoOpenApiAccess 或 OpenEasyApiAccess，则跳过此验证
        if (context.HasAttribute<NoOpenApiAccessAttribute>() || context.HasAttribute<OpenEasyApiAccessAttribute>())
        {
            await next.Invoke();
            return;
        }
        try
        {
            var jwtConfig = HostingOptions.Default.AppOptions.JwtAuthConfig;
            var token = context.HttpContext.Request.GetJwtTokenString()?.Replace("Bearer ", "") ?? "";
            var payload = JwtEncryption.Parse(token, jwtConfig.Secret);
            var data = (JwtUserInfo)payload;
            if (data == null)
                throw FriendlyError.Ex(FrameworkErrors.Auth.NotLoggedIn);
        }
        catch
        {
            throw FriendlyError.Ex(FrameworkErrors.Auth.NotLoggedIn);
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



