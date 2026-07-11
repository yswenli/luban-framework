/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:59:26
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    public class ReplyMusic : Reply
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string MusicUrl { get; set; }

        public string HQMusicUrl { get; set; }

        public string ThumbMediaId { get; set; }

        public ReplyMusic()
        {
            base.MsgType = EnumReplyMsgType.music;
        }
    }
}
