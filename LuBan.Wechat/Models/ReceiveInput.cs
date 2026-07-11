/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： AccessValid
*版本号： V1.0.0.0
*唯一标识：b904a61f-fd3f-40c3-9135-8bbed1ff7ba8
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:23:57
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:23:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// 微信服务号传值
    /// </summary>
    public class ReceiveInput
    {
        [Required]
        public string? signature { get; set; }

        [Required]
        public string timestamp { get; set; }

        [Required]
        public string nonce { get; set; }

        public string echostr { get; set; }

        public string token { get; set; }
    }


    /// <summary>
    /// 企业微信传值
    /// </summary>
    public class BaseWorkReceiveInput
    {
        [Required]
        public string msg_signature { get; set; }

        [Required]
        public string timestamp { get; set; }

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
