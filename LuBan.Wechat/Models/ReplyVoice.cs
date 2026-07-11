/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReplyVoice
*版本号： V1.0.0.0
*唯一标识：db50b581-48fd-49a6-aba6-96d72188c84b
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:58:54
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:58:54
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    public class ReplyVoice : Reply
    {
        public string MediaId { get; set; }

        public ReplyVoice()
        {
            base.MsgType = EnumReplyMsgType.voice;
        }
    }
}
