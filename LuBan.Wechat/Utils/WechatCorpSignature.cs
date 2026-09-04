/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Wechat.Utils
*文件名： WechatCorpSignature.cs
*版本号： V1.0.0.0
*唯一标识：ffb9fb69-5498-43d3-967e-395927c5d4e2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:29
*描述：WechatCorpSignature 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：WechatCorpSignature 类
*
*****************************************************************************/

using SKIT.FlurlHttpClient.Wechat.Work.Utilities;

namespace LuBan.Wechat.Utils
{
    /// <summary>
    /// 微信js sdk 签名
    /// </summary>
    public static class WechatCorpSignature
    {
        /// <summary>
        /// 计算签名
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="nonceStr"></param>
        /// <param name="timestamp"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string CalcSignature(string ticket, string nonceStr, string timestamp, string url)
        {
            return SHA1Utility.Hash($"jsapi_ticket={ticket}&noncestr={nonceStr}&timestamp={timestamp}&url={url.Split('#')[0]}").Value?.ToLower() ?? "";
        }
    }
}
