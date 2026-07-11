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
