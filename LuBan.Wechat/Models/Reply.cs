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
*文件名： Reply
*版本号： V1.0.0.0
*唯一标识：85f1706b-83d0-4cdd-a9ff-f7d3a3756495
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:56:29
*描述：微信被动回复消息实体基类。
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:56:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信被动回复消息实体基类。
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// Reply
    /// </summary>
    public class Reply
    {
        /// <summary>
        /// 消息接收方，即用户的 OpenID（回复协议字段 ToUserName）。
        /// </summary>
        public string ToUserName { get; set; }

        /// <summary>
        /// 消息发送方，即公众号的原始 ID（回复协议字段 FromUserName）。
        /// </summary>
        public string FromUserName { get; set; }

        internal long CreateTime { get; set; }

        /// <summary>
        /// 回复消息的类型（回复协议字段 MsgType）。
        /// </summary>
        public EnumReplyMsgType MsgType { get; set; }
        /// <summary>
        /// Reply
        /// </summary>
        public Reply()
        {
            CreateTime = DateTime.Now.ToFileTime();
        }
    }
}
