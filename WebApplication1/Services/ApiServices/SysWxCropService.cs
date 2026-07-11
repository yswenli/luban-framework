using LuBan.Wechat;
using LuBan.Wechat.Models;

using SKIT.FlurlHttpClient.Wechat.Work.Events;

namespace WebApplication1.Services.ApiServices;

/// <summary>
/// 企业微信业务
/// </summary>
public class SysWxCropService : BaseService<SysWxCropService>
{
    private readonly WechatCorpClient _wechatCorpClient;

    private readonly WechatCorpCallBackCall _wechatCorpCallBackCall;

    /// <summary>
    /// 企业微信业务
    /// </summary>
    public SysWxCropService()
    {
        _wechatCorpClient = (WechatCorpClient)WechatClientFactory.Create(EnumWechatType.Corp);
        _wechatCorpCallBackCall = new WechatCorpCallBackCall(_wechatCorpClient);
    }


    /// <summary>
    /// 验证消息企业微信服务器回调消息或者回调事件通知
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public string Receive(TestWorkReceiveInput input)
    {
        try
        {
            return _wechatCorpCallBackCall.AccessValid(input) ?? "";
        }
        catch (Exception ex)
        {
            Logger.Error("SysWxCropService.Receive", ex, input);
        }
        return string.Empty;
    }

    /// <summary>
    /// 接收微信服务器回调消息或者回调事件通知
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<string> Receive(BaseWorkReceiveInput input)
    {
        try
        {
            var context = WebApp.HttpContext;
            if (context == null) throw new Exception("context is null");
            using (var stream = await context.GetRequestBodyAsync())
            {
                if (stream == null || stream.Length < 1 || !stream.CanRead)
                {
                    Logger.Error("SysWxCropService.Receive", "stream is null or stream.Length <1 or !stream.CanRead");
                    return string.Empty;
                }
                return _wechatCorpCallBackCall.Receive<TextMessageEvent, TextMessageReply>(input, stream, isJson: false,
                    (client, msg, ev) =>
                    {
                        var userId = ev.FromUserName;

                        //todo:处理接收内容业务
                        //OnReceiveKF(ev);

                    },
                    (client, ev) =>
                    {
                        //todo:处理回复业务

                        TextMessageReply replay = new TextMessageReply()
                        {
                            ToUserName = ev.FromUserName,
                            FromUserName = ev.ToUserName,
                            Content = $"LuBan 测试回复【{ev.Content}】",
                            MessageType = "text",
                            CreateTimestamp = ev.CreateTimestamp,
                            Event = ev.Event,
                            InfoTimestamp = ev.InfoTimestamp,
                            InfoType = ev.InfoType
                        };

                        return replay;
                    });
            }

        }
        catch (Exception ex)
        {
            Logger.Error("SysWxCropService.Receive", ex);
        }
        return string.Empty;
    }

}
