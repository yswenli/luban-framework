/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： OpenEasyApiAccessAttribute
*版本号： V1.0.0.0
*唯一标识：easy-open-api-access
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/20
*描述：简化版开放接口验证，只需传入 Bearer {RefreshToken}
*
*****************************************************************************/
using LuBan.Common.Errors;
using LuBan.Web.Core.Utils;

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 简化版开放接口验证，
/// 只需传入 Bearer {RefreshToken} 即可，
/// 无需走完整的 AccessToken 流程
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class OpenEasyApiAccessAttribute : BaseFilterAttribute
{
    /// <summary>
    /// 执行业务前
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var token = context.HttpContext.Request.GetJwtTokenString()?.Replace("Bearer ", "") ?? "";
            
            if (string.IsNullOrEmpty(token))
            {
                throw FriendlyError.Ex(FrameworkErrors.Auth.NotLoggedIn);
            }

            // 使用 RefreshToken 直接获取 AccessToken（验证 RefreshToken 有效性）
            var accessToken = await OpenApiAccessUtil.GetAccessTokenAsync(token, 7200);
            
            if (accessToken == null || string.IsNullOrEmpty(accessToken.Token))
            {
                throw FriendlyError.Ex(FrameworkErrors.Auth.NotLoggedIn);
            }

            // 将生成的 AccessToken 存入 HttpContext，供后续使用
            context.HttpContext.Items["EasyApiAccessToken"] = accessToken.Token;
            context.HttpContext.Items["EasyApiRefreshToken"] = token;
        }
        catch (FriendlyException)
        {
            throw;
        }
        catch (Exception)
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
