/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Wechat
*文件名： WechatCorpCallBackCall
*版本号： V1.0.0.0
*唯一标识：8cc8a7f0-10f4-4f22-9735-a58f85b4caa0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/10 14:11:12
*描述：企业微信服务器回调处理类
*
*=================================================
*修改标记
*修改时间：2024/7/10 14:11:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信服务器回调处理类
*
*****************************************************************************/
namespace LuBan.Wechat;

/// <summary>
/// 企业微信服务器回调处理类
/// </summary>
/// <param name="WechatCorpClient"></param>
/// <param name="encryptText"></param>
/// <param name="ev"></param>
public delegate void WechatCorpReceive(IWechatCorpClient WechatCorpClient, string encryptText, dynamic ev);

/// <summary>
/// 企业微信服务器回复处理类
/// </summary>
/// <typeparam name="TReply"></typeparam>
/// <param name="WechatCorpClient"></param>
/// <param name="ev"></param>
/// <returns></returns>
public delegate TReply WechatCorpReply<TReply>(IWechatCorpClient WechatCorpClient, dynamic ev) where TReply : WechatWorkEvent, new();

/// <summary>
/// 企业微信服务器回调处理类
/// </summary>
public class WechatCorpCallBackCall
{
    IWechatCorpClient _WechatCorpClient;

    /// <summary>
    /// 企业微信服务器回调处理类
    /// </summary>
    /// <param name="input"></param>
    public WechatCorpCallBackCall(IWechatCorpClient wechatWorkClient)
    {
        _WechatCorpClient = wechatWorkClient;
    }

    /// <summary>
    /// 验证回调通知事件签名
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isJson"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string? AccessValid(TestWorkReceiveInput input)
    {
        var result = _WechatCorpClient.Client.VerifyEventSignatureForEcho(input.timestamp, input.nonce, input.echostr, input.msg_signature, out string? reply);
        if (result.Result)
        {
            return reply;
        }
        else
        {
            throw new Exception($"WorkWeChatCallBackCall.AccessValid:{result.Error}");
        }
    }




    /// <summary>
    /// 从流中获取抽象事件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encryptText"></param>
    /// <param name="isJson"></param>
    /// <returns></returns>
    public WechatWorkEvent? GetEventFromStream(Stream stream, out string encryptText, bool isJson = false)
    {
        encryptText = "";
        using (StreamReader streamReader = new StreamReader(stream))
        {
            encryptText = streamReader.ReadToEnd();
        }
        if (encryptText.IsNullOrEmpty()) return null;
        WechatWorkEvent ev;
        if (isJson)
        {
            ev = _WechatCorpClient.Client.DeserializeEventFromJson(encryptText);
        }
        else
        {
            ev = _WechatCorpClient.Client.DeserializeEventFromXml(encryptText);
        }
        return ev;
    }

    /// <summary>
    /// 从密文中获取事件
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="encryptText"></param>
    /// <param name="isJson"></param>
    /// <returns></returns>
    public TEvent GetEventFromEncryptText<TEvent>(string encryptText, bool isJson = false) where TEvent : WechatWorkEvent, new()
    {
        TEvent ev;
        if (isJson)
        {
            ev = _WechatCorpClient.Client.DeserializeEventFromJson<TEvent>(encryptText);
        }
        else
        {
            ev = _WechatCorpClient.Client.DeserializeEventFromXml<TEvent>(encryptText);
        }
        return ev;
    }





    /// <summary>
    /// 接收微信回调请求流
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    /// <param name="input"></param>
    /// <param name="stream"></param>
    /// <param name="isJson"></param>
    /// <param name="onReceive"></param>
    /// <param name="onReply"></param>
    /// <returns></returns>
    public string Receive<TEvent, TReply>(BaseWorkReceiveInput input, Stream stream,
        bool isJson = false,
        WechatCorpReceive? onReceive = null,
        WechatCorpReply<TReply>? onReply = null)
        where TEvent : WechatWorkEvent, new()
        where TReply : WechatWorkEvent, new()
    {
        try
        {
            var encryptText = "";
            using (StreamReader streamReader = new StreamReader(stream))
            {
                encryptText = streamReader.ReadToEnd();
            }

            //验证签名
            //var valideResult = _workClient.VerifyEventSignatureForEcho(input.timestamp, input.nonce, msg, input.msg_signature, out string? reply);

            TEvent ev;
            if (isJson)
            {
                ev = _WechatCorpClient.Client.DeserializeEventFromJson<TEvent>(encryptText);
                if (onReceive != null)
                {
                    onReceive.Invoke(_WechatCorpClient, encryptText, ev);
                }
            }
            else
            {
                ev = _WechatCorpClient.Client.DeserializeEventFromXml<TEvent>(encryptText);
                if (onReceive != null)
                {
                    onReceive.Invoke(_WechatCorpClient, encryptText, ev);
                }
            }
            if (onReply != null)
            {
                var replay = onReply.Invoke(_WechatCorpClient, ev);
                if (replay == null) return string.Empty;
                if (isJson)
                {
                    return _WechatCorpClient.Client.SerializeEventToJson(replay);
                }
                else
                {
                    return _WechatCorpClient.Client.SerializeEventToXml(replay);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("WorkWeChatCallBackCall.Receive<TEvent>", ex);
        }
        return string.Empty;
    }
}
