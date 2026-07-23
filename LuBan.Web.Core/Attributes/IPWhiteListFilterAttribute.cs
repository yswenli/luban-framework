/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Auth
*文件名： IPWhiteListFilterAttribute
*版本号： V1.0.0.0
*唯一标识：3dd4d54a-540a-4f18-96a5-6ac6b822739b
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2023/1/11 11:46:09
*描述：ip白名单限制器
*
*=================================================
*修改标记
*修改时间：2023/1/11 11:46:09
*修改人： yswen
*版本号： V1.0.0.0
*描述：ip白名单限制器
*
*****************************************************************************/
using System.Net;

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// ip白名单限制器
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class IPWhiteListFilterAttribute : BaseFilterAttribute
{
    List<string> _ipWhiteList;

    /// <summary>
    /// ip白名单限制器
    /// </summary>
    /// <param name="order"></param>
    public IPWhiteListFilterAttribute(int order = 3)
    {
        _ipWhiteList = ConfigUtil.Read<List<string>>("IPWhiteList") ?? [];
        Order = order;
    }

    /// <summary>
    /// 拦截逻辑
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_ipWhiteList == null || _ipWhiteList.Count < 1) return;

        var remoteIpString = context.HttpContext.GetClientIp();

        if (remoteIpString.IsNullOrEmpty())
        {
            context.Result = new JsonResult(new Fail<string>(new Exception("无法获取ip")));
            return;
        }

        var remoteIps = remoteIpString.Split(',');

        if (remoteIps == null || remoteIps.Length < 1)
        {
            context.Result = new JsonResult(new Fail<string>(new Exception("无法获取ip")));
            return;
        }

        if (remoteIps[0].IsNullOrEmpty())
        {
            context.Result = new JsonResult(new Fail<string>(new Exception("无法获取ip")));
            return;
        }

        var remoteIp = remoteIps[0].Trim();
        if (!IPAddress.TryParse(remoteIp, out var remoteAddress))
        {
            context.Result = new JsonResult(new Fail<string>(new Exception("无法解析ip:" + remoteIp)));
            return;
        }

        if (_ipWhiteList.Exists(q =>
        {
            if (IPAddress.TryParse(q.Trim(), out var allowedAddress))
                return remoteAddress.Equals(allowedAddress);
            return false;
        }))
        {
            await next.Invoke();
            return;
        }

        if (IPAddress.TryParse("127.0.0.1", out var loopback) && remoteAddress.Equals(loopback))
        {
            await next.Invoke();
        }
        else
        {
            context.Result = new JsonResult(new Fail<string>(new Exception("非法的访问，ip:" + remoteIpString)));
        }
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
