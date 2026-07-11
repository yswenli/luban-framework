/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： WeChatRobot
*版本号： V1.0.0.0
*唯一标识：fb3af6cf-4a34-44a8-9ba5-b9fc8f7f5888
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/6 10:10:47
*描述：企业微信机器人
*
*=================================================
*修改标记
*修改时间：2024/12/6 10:10:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：企业微信机器人
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 企业微信机器人
/// </summary>
public class WeChatRobot : BaseSingleInstance<WeChatRobot>
{
    static HttpClientProxy _httpClientUtil = HttpClientProxy.Create("https://qyapi.weixin.qq.com");

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="key"></param>
    /// <param name="mentioneds"></param>
    public void SendMsg(string msg, string key, params string[] mentioneds)
    {
        if (mentioneds == null || mentioneds.Length == 0)
        {
            mentioneds = ["@all"];
        }
        var data = new
        {
            msgtype = "text",
            text = new
            {
                content = msg,
                mentioned_list = mentioneds
            }
        };
        _ = _httpClientUtil.PostAsync<dynamic>($"/cgi-bin/webhook/send?key={key}", data).GetAwaiter().GetResult();
    }


    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="key"></param>
    public void SendMsg(string msg, params string[] mentioneds)
    {
        SendMsg(msg, "d1910225-bbc5-4c00-8889-c7f50359e163", mentioneds);
    }
}
