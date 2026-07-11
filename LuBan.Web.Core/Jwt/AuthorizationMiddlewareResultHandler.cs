/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Jwt
*文件名： AuthorizationMiddlewareResultHandler
*版本号： V1.0.0.0
*唯一标识：c30372aa-f50e-4a1b-a505-fa2cdc9c3361
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/2 15:28:17
*描述：处理jwt登录逻辑
*
*=================================================
*修改标记
*修改时间：2025/12/2 15:28:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：处理jwt登录逻辑
*
*****************************************************************************/
namespace LuBan.Web.Core.Jwt;

/// <summary>
/// 处理jwt登录逻辑
/// </summary>
public class AuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    /// <summary>
    /// 处理jwt登录逻辑
    /// </summary>
    /// <param name="next"></param>
    /// <param name="context"></param>
    /// <param name="policy"></param>
    /// <param name="authorizeResult"></param>
    /// <returns></returns>
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        // 判断是否为"未登录"场景
        if (authorizeResult.AuthorizationFailure?.FailureReasons.Any(r =>
            r.Message.Contains("User is not authenticated") ||
            r.Message.Contains("未认证")) ?? false)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(new Fail<string>("当前操作未登录，请先登录", 401).ToJson());
            return;
        }

        // 验证当前登录用户状态
        if (SessionUser.UserId < 1 || SessionUser.CurrentUser == null || SessionUser.CurrentUser.Status != EnumEnableStatus.Enable)
        {
            var response = context.Response;
            response.Clear();
            response.StatusCode = StatusCodes.Status403Forbidden;
            response.ContentType = "application/json; charset=utf-8";
            var isApiRequest = context.Request.Path.StartsWithSegments("/api");
            var isHtmlRequest = context.Request.Headers.Accept.ToString().Contains("text/html");
            await response.WriteAsync(new Fail<string>("当前登录用户不存在或已被禁用", 403).ToJson());
            return;
        }

        // 认证通过，继续执行后续逻辑
        await next(context);
    }
}
