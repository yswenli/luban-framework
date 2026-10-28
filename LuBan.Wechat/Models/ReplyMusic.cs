/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReplyMusic
*版本号： V1.0.0.0
*唯一标识：a11da5db-b10d-4226-82cc-6b42c92e5909
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:59:26
*描述：ReplyMusic 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:59:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReplyMusic 类
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReplyMusic 类
/// </summary>
    public class ReplyMusic : Reply
    {
        /// <summary>
        /// 音乐标题（回复协议字段 Title）。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 音乐描述文本（回复协议字段 Description）。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 音乐播放链接（回复协议字段 MusicUrl）。
        /// </summary>
        public string MusicUrl { get; set; }

        /// <summary>
        /// 高品质音乐链接，WiFi 环境下优先播放（回复协议字段 HQMusicUrl）。
        /// </summary>
        public string HQMusicUrl { get; set; }

        /// <summary>
        /// 音乐封面缩略图的素材 ID（回复协议字段 ThumbMediaId）。
        /// </summary>
        public string ThumbMediaId { get; set; }

        /// <summary>
        /// 构造音乐回复消息，并把消息类型置为 music。
        /// </summary>
        public ReplyMusic()
        {
            base.MsgType = EnumReplyMsgType.music;
        }
    }
}
