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
*文件名： ReplyVideo
*版本号： V1.0.0.0
*唯一标识：2340cc77-7704-4932-afc4-e1495b65f738
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:59:51
*描述：微信视频回复消息实体。
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:59:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信视频回复消息实体。
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// ReplyVideo
    /// </summary>
    public class ReplyVideo : Reply
    {
        /// <summary>
        /// 回复视频的素材 ID，需为已上传的视频素材（回复协议字段 MediaId）。
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// 视频标题（回复协议字段 Title）。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 视频描述文本（回复协议字段 Description）。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ReplyVideo
        /// </summary>
        public ReplyVideo()
        {
            base.MsgType = EnumReplyMsgType.video;
        }
    }
}
