/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Attributes
*文件名： OnlineUserMiddleware
*版本号： V1.0.0.0
*唯一标识：0506282b-82ec-4dd9-aede-99d02655c571
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/6
*描述：在线用户验证中间件
*
*=================================================
*修改标记
*修改时间：2026/7/6
*修改人： yswenli
*版本号： V1.0.0.0
*描述：在线用户验证中间件
*
*****************************************************************************/

using LuBan.Web.Core.OnlineUser;

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 在线用户验证中间件
/// </summary>
public class OnlineUserMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 本地缓存过期时间（秒）
    /// </summary>
    private const int CacheExpiredSeconds = 30;

    /// <summary>
    /// 在线用户验证中间件
    /// </summary>
    /// <param name="next"></param>
    public OnlineUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 在线用户验证中间件
    /// </summary>
    /// <remarks>
    /// 异常处理策略：
    /// - FriendlyException: 返回401，表示验证失败（会话过期、被踢下线等）
    /// - 其他Exception: 返回500，表示系统故障（数据库错误、网络问题等）
    /// 采用fail-close策略确保安全性，但区分验证失败和系统故障避免误判。
    /// </remarks>
    /// <param name="context"></param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        var userId = SessionUser.UserId;
        if (userId == 0)
        {
            await _next(context);
            return;
        }

        var tenantId = SessionUser.TenantId;
        var cacheKey = $"{CacheConst.KeyUserOnline}{tenantId}:{userId}";

        try
        {
            var session = MemoryCache.Instance.Get<OnlineUserSession>(cacheKey);

            if (session == null)
            {
                session = await OnlineUserStoreProvider.Store.ReadAsync(tenantId, userId);
            }

            var expireSeconds = HostingOptions.Default.AppOptions.JwtAuthConfig?.AccessExpiration ?? 7200;
            if (session == null || session.LastActiveTime.AddSeconds(expireSeconds) < DateTime.UtcNow)
            {
                await UnauthorizedAsync(context);
                return;
            }

            await OnlineUserStoreProvider.Store.RefreshAsync(tenantId, userId);
            session.LastActiveTime = DateTime.UtcNow;
            var cacheSeconds = Math.Min(expireSeconds, CacheExpiredSeconds);
            MemoryCache.Instance.Set(cacheKey, session, TimeSpan.FromSeconds(cacheSeconds));

            await _next(context);
        }
        catch (FriendlyException ex)
        {
            await UnauthorizedAsync(context);
        }
        catch (Exception ex)
        {
            Logger.Error($"在线用户验证系统错误：{cacheKey}", ex);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(new Fail<string>("系统错误，请稍后重试", 500).ToJson());
        }
    }

    private static async Task UnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(new Fail<string>("当前操作未登录或会话已过期，请重新登录", 401).ToJson());
    }
}


/// <summary>
/// 在线用户验证中间件
/// </summary>
public static class OnlineUserMiddlewareExtensions
{
    /// <summary>
    /// 使用在线用户验证中间件
    /// </summary>
    /// <param name="builder"></param>
    public static IApplicationBuilder UseOnlineUserMiddleware(this IApplicationBuilder builder)
    {
        if (HostingOptions.Default.AppOptions.OnlineUser.Enabled)
        {
            return builder.UseMiddleware<OnlineUserMiddleware>();
        }
        return builder;
    }
}