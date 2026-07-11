/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat
*文件名： WechatCallBackCall
*版本号： V1.0.0.0
*唯一标识：303c4539-b708-4a99-ab2e-448e0c72cb7a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 13:29:24
*描述：微信服务器回调处理类
*
*=================================================
*修改标记
*修改时间：2023/12/5 13:29:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信服务器回调处理类
*
*****************************************************************************/

namespace LuBan.Wechat;


/// <summary>
/// 回调消息接收委托类
/// </summary>
/// <param name="xml"></param>
/// <param name="t"></param>
public delegate void CallMessagesReceiveDelegate(string xml, Receive t);

/// <summary>
/// 回调消息回复委托类
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="xml"></param>
/// <param name="t"></param>
public delegate void CallMessagesReplyDelegate<T>(string xml, T t) where T : Reply;

/// <summary>
/// 微信服务器回调处理类
/// </summary>
public class WechatCallBackCall
{
    ReceiveInput _receiveInput;

    /// <summary>
    /// 微信服务器回调处理类
    /// </summary>
    /// <param name="input"></param>
    public WechatCallBackCall(ReceiveInput input)
    {
        _receiveInput = input;
    }

    /// <summary>
    /// 验证微信回调签名
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool AccessValid()
    {
        try
        {
            string[] array = [_receiveInput.token, _receiveInput.timestamp, _receiveInput.nonce];
            Array.Sort(array);
            string password = string.Join("", array);
            var p1 = SHAUtil.GetSHA1(password).ToLower();
            if (p1.Equals(_receiveInput?.signature ?? "".ToLower()))
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new Exception("WeChatCallBackCall.AccessValid:" + ex.Message);
        }
    }


    /// <summary>
    /// 接收微信回调请求流
    /// </summary>
    /// <param name="xmlBytes"></param>
    /// <param name="xml"></param>
    /// <param name="receive"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static object? XmlMsgRevolve(byte[] xmlBytes, out string xml, out Receive? receive, CallMessagesReceiveDelegate? callback = null)
    {
        xml = string.Empty;
        receive = null;
        if (xmlBytes != null && xmlBytes.Length > 0)
        {
            using var ms = new MemoryStream(xmlBytes);
            ms.Seek(0, SeekOrigin.Begin);
            using (StreamReader streamReader = new(ms))
            {
                xml = streamReader.ReadToEnd();
            }
            object? obj = xml.XmlMsgRevolve();
            if (obj == null) return obj;
            receive = (Receive)obj;
            callback?.Invoke(xml, receive);
            return obj;
        }
        return null;
    }


    /// <summary>
    /// 服务器回复
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string Reply<T>(T t, CallMessagesReplyDelegate<T>? callback = null) where T : Reply
    {
        try
        {
            string result = string.Empty;
            t.CreateTime = DateTime.Now.ToFileTime();
            switch (t.MsgType)
            {
                case EnumReplyMsgType.text:
                    result = "<xml>\r\n                                                <ToUserName><![CDATA[[ToUserName]]]></ToUserName>\r\n                                                <FromUserName><![CDATA[[FromUserName]]]></FromUserName>\r\n                                                <CreateTime>[CreateTime]</CreateTime>\r\n                                                <MsgType><![CDATA[[MsgType]]]></MsgType>\r\n                                                <Content><![CDATA[[Content]]]></Content>\r\n                                            </xml>".Replace("[ToUserName]", t.ToUserName).Replace("[FromUserName]", t.FromUserName).Replace("[CreateTime]", t.CreateTime.ToString())
                        .Replace("[MsgType]", t.MsgType.ToString())
                        .Replace("[Content]", (t as ReplyText)?.Content.TextValue() ?? "".Replace("[_openid_]", t.ToUserName));
                    break;
                case EnumReplyMsgType.image:
                    result = "<xml>\r\n                                                <ToUserName><![CDATA[[ToUserName]]]></ToUserName>\r\n                                                <FromUserName><![CDATA[[FromUserName]]]></FromUserName>\r\n                                                <CreateTime>[CreateTime]</CreateTime>\r\n                                                <MsgType><![CDATA[[MsgType]]]></MsgType>\r\n                                                <Image>\r\n                                                <MediaId><![CDATA[[MediaId]]]></MediaId>\r\n                                                </Image>\r\n                                            </xml>".Replace("[ToUserName]", t.ToUserName).Replace("[FromUserName]", t.FromUserName).Replace("[CreateTime]", t.CreateTime.ToString())
                        .Replace("[MsgType]", t.MsgType.ToString())
                        .Replace("[MediaId]", (t as ReplyImage)?.MediaId);
                    break;
                case EnumReplyMsgType.voice:
                    result = "<xml>\r\n                                                <ToUserName><![CDATA[[ToUserName]]]></ToUserName>\r\n                                                <FromUserName><![CDATA[[FromUserName]]]></FromUserName>\r\n                                                <CreateTime>[CreateTime]</CreateTime>\r\n                                                <MsgType><![CDATA[[MsgType]]]></MsgType>\r\n                                                <Voice>\r\n                                                <MediaId><![CDATA[[MediaId]]]></MediaId>\r\n                                                </Voice>\r\n                                            </xml>".Replace("[ToUserName]", t.ToUserName).Replace("[FromUserName]", t.FromUserName).Replace("[CreateTime]", t.CreateTime.ToString())
                        .Replace("[MsgType]", t.MsgType.ToString())
                        .Replace("[MediaId]", (t as ReplyVoice)?.MediaId);
                    break;
                case EnumReplyMsgType.video:
                    {
                        ReplyVideo? replyVideo = t as ReplyVideo;
                        result = "<xml>\r\n                                                <ToUserName><![CDATA[[ToUserName]]]></ToUserName>\r\n                                                <FromUserName><![CDATA[[FromUserName]]]></FromUserName>\r\n                                                <CreateTime>[CreateTime]</CreateTime>\r\n                                                <MsgType><![CDATA[[MsgType]]]></MsgType>\r\n                                                <Video>\r\n                                                <MediaId><![CDATA[[MediaId]]]></MediaId>\r\n                                                <Title><![CDATA[[Title]]]></Title>\r\n                                                <Description><![CDATA[[Description]]]></Description>\r\n                                                </Video> \r\n                                            </xml>".Replace("[ToUserName]", t.ToUserName).Replace("[FromUserName]", t.FromUserName).Replace("[CreateTime]", t.CreateTime.ToString())
                            .Replace("[MsgType]", t.MsgType.ToString())
                            .Replace("[MediaId]", replyVideo?.MediaId)
                            .Replace("[Title]", replyVideo?.Title)
                            .Replace("[Description]", replyVideo?.Description);
                        break;
                    }
                case EnumReplyMsgType.music:
                    {
                        ReplyMusic? replyMusic = t as ReplyMusic;
                        result = "<xml>\r\n                                                <ToUserName><![CDATA[[ToUserName]]]></ToUserName>\r\n                                                <FromUserName><![CDATA[[FromUserName]]]></FromUserName>\r\n                                                <CreateTime>[CreateTime]</CreateTime>\r\n                                                <MsgType><![CDATA[[MsgType]]]></MsgType>\r\n                                                <Music>\r\n                                                <Title><![CDATA[[Title]]]></Title>\r\n                                                <Description><![CDATA[[Description]]]></Description>\r\n                                                <MusicUrl><![CDATA[[MusicUrl]]]></MusicUrl>\r\n                                                <HQMusicUrl><![CDATA[[HQMusicUrl]]]></HQMusicUrl>\r\n                                                <ThumbMediaId><![CDATA[[ThumbMediaId]]]></ThumbMediaId>\r\n                                                </Music>\r\n                                            </xml>".Replace("[ToUserName]", t.ToUserName).Replace("[FromUserName]", t.FromUserName).Replace("[CreateTime]", t.CreateTime.ToString())
                            .Replace("[MsgType]", t.MsgType.ToString())
                            .Replace("[Title]", replyMusic?.Title)
                            .Replace("[Description]", replyMusic?.Description)
                            .Replace("[MusicUrl]", replyMusic?.MusicUrl)
                            .Replace("[HQMusicUrl]", replyMusic?.HQMusicUrl)
                            .Replace("[ThumbMediaId]", replyMusic?.ThumbMediaId);
                        break;
                    }
                case EnumReplyMsgType.news:
                    {
                        ReplyNews? replyNews = t as ReplyNews;
                        string text = string.Empty;
                        foreach (RelyNewsArticle item in replyNews?.item ??
                            [])
                        {
                            text += "<item>\r\n                                                    <Title><![CDATA[[Title]]]></Title> \r\n                                                    <Description><![CDATA[[Description]]]></Description>\r\n                                                    <PicUrl><![CDATA[[PicUrl]]]></PicUrl>\r\n                                                    <Url><![CDATA[[Url]]]></Url>\r\n                                                </item>".Replace("[Title]", item.Title).Replace("[Description]", item.Description).Replace("[PicUrl]", item.PicUrl)
                                .Replace("[Url]", item.Url);
                        }

                        result = "<xml>\r\n                                                <ToUserName><![CDATA[[ToUserName]]]></ToUserName>\r\n                                                <FromUserName><![CDATA[[FromUserName]]]></FromUserName>\r\n                                                <CreateTime>[CreateTime]</CreateTime>\r\n                                                <MsgType><![CDATA[[MsgType]]]></MsgType>\r\n                                                <ArticleCount>[ArticleCount]</ArticleCount>\r\n                                                <Articles>[Articles]</Articles>\r\n                                            </xml> ".Replace("[ToUserName]", t.ToUserName).Replace("[FromUserName]", t.FromUserName).Replace("[CreateTime]", t.CreateTime.ToString())
                            .Replace("[MsgType]", t.MsgType.ToString())
                            .Replace("[ArticleCount]", replyNews?.item.Count.ToString())
                            .Replace("[Articles]", text);
                        break;
                    }
                default:
                    return string.Empty;
            }

            callback?.Invoke(result, t);
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("Reply XML:" + ex.Message);
        }
    }


    /// <summary>
    /// Signature
    /// </summary>
    /// <param name="jsapi_ticket"></param>
    /// <param name="url"></param>
    /// <param name="noncestr"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string Signature(string jsapi_ticket, string url, out string noncestr, out long timestamp)
    {
        noncestr = Guid.NewGuid().ToString("N");
        timestamp = DateTime.Now.ToUnixTimeStamp(hasMilliseconds: false);
        string[] value =
        [
            "jsapi_ticket=" + jsapi_ticket,
            "noncestr=" + noncestr,
            "timestamp=" + timestamp,
            "url=" + url
        ];
        string data = string.Join("&", value);
        return SHAUtil.GetSHA1(data);
    }

}
