/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Wechat
*文件名： WechatClientInterceptor
*版本号： V1.0.0.0
*唯一标识：3c835f92-33aa-4287-8fe0-cec075c46a17
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/5/28 14:07:04
*描述：微信调用日志拦截器
*
*=================================================
*修改标记
*修改时间：2024/5/28 14:07:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信调用日志拦截器
*
*****************************************************************************/
namespace LuBan.Wechat;

/// <summary>
/// 微信调用日志拦截器
/// </summary>
public class WechatClientInterceptor : HttpInterceptor
{
    /// <summary>
    /// 在 HTTP 请求发出前回调，此处不做任何处理直接返回已完成任务。
    /// </summary>
    /// <param name="context">当前 HTTP 调用上下文，含请求地址、请求体等信息。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>已完成的空任务。</returns>
    public override Task BeforeCallAsync(HttpInterceptorContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 记录调用日志
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task AfterCallAsync(HttpInterceptorContext context, CancellationToken cancellationToken)
    {
        var responseContent = context.FlurlCall.HttpResponseMessage?.Content != null 
            ? await context.FlurlCall.HttpResponseMessage.Content.ReadAsStringAsync(cancellationToken)
            : "";
        Logger.ApiCallLog(GuidUtil.GuidString,
            "127.0.0.1",
            context.FlurlCall.Request.Url,
            context.FlurlCall.Request.Verb.Method,
            context.FlurlCall.Request.Headers.ToJson() ?? "",
            context.FlurlCall.RequestBody,
            Convert.ToInt64(context.FlurlCall.Duration?.TotalMilliseconds ?? 0),
            context.FlurlCall.Response.StatusCode,
            responseContent,
            "",
            null);
    }
}
