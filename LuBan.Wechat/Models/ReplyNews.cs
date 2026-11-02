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
*文件名： ReplyNews
*版本号： V1.0.0.0
*唯一标识：8b00d954-508a-4c5f-ab44-495cae1e839d
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 15:06:17
*描述：ReplyNews 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 15:06:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReplyNews 类
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReplyNews 类
/// </summary>
    public class ReplyNews : Reply
    {
        /// <summary>
        /// 图文消息的文章列表，按顺序渲染为多条图文（回复协议字段 item）。
        /// </summary>
        public List<RelyNewsArticle> item { get; set; }

        /// <summary>
        /// 构造图文回复消息，并把消息类型置为 news。
        /// </summary>
        public ReplyNews()
        {
            base.MsgType = EnumReplyMsgType.news;
        }
    }

    /// <summary>
    /// 图文消息中的单篇文章，承载标题、摘要、封面与跳转链接。
    /// </summary>
    public class RelyNewsArticle
    {
        /// <summary>
        /// 文章标题（回复协议字段 Title）。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 文章摘要文本（回复协议字段 Description）。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 文章封面图片链接（回复协议字段 PicUrl）。
        /// </summary>
        public string PicUrl { get; set; }

        /// <summary>
        /// 文章点击后跳转的链接（回复协议字段 Url）。
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 图文消息缩略图的素材 ID（回复协议字段 Thumb_media_id）。
        /// </summary>
        public string Thumb_media_id { get; set; }
    }
}
