/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： WechatPayOptions
*版本号： V1.0.0.0
*唯一标识：57b469f7-9196-4304-9150-1d035050e765
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/5 10:49:38
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/5 10:49:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    /// <summary>
    /// 微信支付配置选项
    /// </summary>
    public sealed class WechatPayOptions : WechatTenpayClientOptions
    {
        /// <summary>
        /// 微信公众平台AppId、开放平台AppId、小程序AppId、企业微信CorpId
        /// </summary>
        public string AppId { get; set; }


        /// <summary>
        /// 微信支付回调
        /// </summary>
        public string WechatPayUrl { get; set; }
    }
}
