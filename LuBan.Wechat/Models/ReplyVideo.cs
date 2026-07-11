/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:59:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// ReplyVideo
    /// </summary>
    public class ReplyVideo : Reply
    {
        public string MediaId { get; set; }

        public string Title { get; set; }

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
