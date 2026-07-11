/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Wechat.Models
*文件名： EnumWechatType
*版本号： V1.0.0.0
*唯一标识：ffcdbc9d-9249-46f6-85ab-e50f5cc3ea48
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/14 10:59:32
*描述：微信类型
*
*=================================================
*修改标记
*修改时间：2023/12/14 10:59:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信类型
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    /// <summary>
    /// 微信类型
    /// </summary>
    public enum EnumWechatType
    {
        /// <summary>
        /// 公众号
        /// </summary>
        [Description("公众号")]
        Api = 1,
        /// <summary>
        /// 小程序
        /// </summary>
        [Description("小程序")]
        App = 2,
        /// <summary>
        /// 微信支付
        /// </summary>
        [Description("微信支付")]
        Pay = 3,
        /// <summary>
        /// 企业微信
        /// </summary>
        [Description("企业微信")]
        Corp = 4
    }
}
