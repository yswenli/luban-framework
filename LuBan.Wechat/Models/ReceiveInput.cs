/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReceiveInput
*版本号： V1.0.0.0
*唯一标识：b904a61f-fd3f-40c3-9135-8bbed1ff7ba8
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:23:57
*描述：微信消息接收参数与基础工作消息实体。
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:23:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信消息接收参数与基础工作消息实体。
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// 微信服务号传值
    /// </summary>
    public class ReceiveInput
    {
        /// <summary>
        /// 微信服务器签名，用于校验请求来源的合法性（URL 参数 signature）。
        /// </summary>
        [Required]
        public string? signature { get; set; }

        /// <summary>
        /// 签名使用的时间戳（URL 参数 timestamp）。
        /// </summary>
        [Required]
        public string timestamp { get; set; }

        /// <summary>
        /// 参与签名计算的随机字符串（URL 参数 nonce）。
        /// </summary>
        [Required]
        public string nonce { get; set; }

        /// <summary>
        /// 服务器配置校验时的随机字符串，校验通过需原样返回（URL 参数 echostr）。
        /// </summary>
        public string echostr { get; set; }

        /// <summary>
        /// 公众号后台配置的 Token，用于验证请求签名。
        /// </summary>
        public string token { get; set; }
    }


    /// <summary>
    /// 企业微信传值
    /// </summary>
    public class BaseWorkReceiveInput
    {
        /// <summary>
        /// 企业微信消息签名，用于校验加密消息的真实性（URL 参数 msg_signature）。
        /// </summary>
        [Required]
        public string msg_signature { get; set; }

        /// <summary>
        /// 签名使用的时间戳（URL 参数 timestamp）。
        /// </summary>
        [Required]
        public string timestamp { get; set; }

        /// <summary>
        /// 参与签名计算的随机字符串（URL 参数 nonce）。
        /// </summary>
        [Required]
        public string nonce { get; set; }

    }

    /// <summary>
    /// 测试企业微信传值
    /// </summary>
    public class TestWorkReceiveInput : BaseWorkReceiveInput
    {
        /// <summary>
        /// 密文
        /// </summary>
        public string echostr { get; set; }
    }
}
