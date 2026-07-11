/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 15:06:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    public class ReplyNews : Reply
    {
        public List<RelyNewsArticle> item { get; set; }

        public ReplyNews()
        {
            base.MsgType = EnumReplyMsgType.news;
        }
    }

    public class RelyNewsArticle
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string PicUrl { get; set; }

        public string Url { get; set; }

        public string Thumb_media_id { get; set; }
    }
}
