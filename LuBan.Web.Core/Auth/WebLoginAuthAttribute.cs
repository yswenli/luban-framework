/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Auth
*文件名： LoginAuthAttribute
*版本号： V1.0.0.0
*唯一标识：9a1ac990-92de-4b68-8030-165e9ac4a19b
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/9/6 11:15:59
*描述：用户登录较验
*
*=================================================
*修改标记
*修改时间：2022/9/6 11:15:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：用户登录较验
*
*****************************************************************************/
namespace LuBan.Web.Core.Auth;

/// <summary>
/// 用户登录较验,对应的是NoLoginAuthAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class WebLoginAuthAttribute : BaseFilterAttribute
{
    string _redirect;

    /// <summary>
    /// 用户登录较验
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="order"></param>
    /// <param name="redirect"></param>
    public WebLoginAuthAttribute(int order = 2, string redirect = "/management/account/login")
    {
        Order = order;
        _redirect = redirect;
    }

    /// <summary>
    /// 拦截处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HasAttribute<NoWebLoginAuthAttribute>())
        {
            await next.Invoke();
            return;
        }

        var userIdKey = "UserID".GetMD5Str();
        var cuid = context.HttpContext.Request.Cookies[userIdKey];
        if (cuid.IsNullOrEmpty())
        {
            var relativePath = context.HttpContext.Request.Path;
            var query = relativePath + context.HttpContext.Request.QueryString;
            var redirectUrl = _redirect + "?redirect=" + query.UrlEncode();
            if (!redirectUrl.StartsWith("/") || redirectUrl.Contains("://"))
            {
                redirectUrl = "/management/account/login";
            }
            context.Result = new RedirectResult(redirectUrl);
            return;
        }

        if (!long.TryParse(AESUtil.Decrypt(cuid.UrlDecode(), KeyIvExtensions.DEFAULTKEY), out _))
        {
            context.HttpContext.Response.Cookies.Delete(userIdKey);
            var relativePath = context.HttpContext.Request.Path;
            var query = relativePath + context.HttpContext.Request.QueryString;
            context.Result = new RedirectResult(_redirect + "?redirect=" + query.UrlEncode());
            return;
        }

        await next.Invoke();
    }

    /// <summary>
    /// 执行后
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next.Invoke();
    }
}

/// <summary>
/// 标记无需登录,对应的是LoginAuth
/// AllowAnonymous对应的是Authorize
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NoWebLoginAuthAttribute : Attribute
{

}
