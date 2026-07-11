/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： ReplyText
*版本号： V1.0.0.0
*唯一标识：386a5cd1-1a17-435b-a4e9-b9c7d6adbf0e
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:57:57
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:57:57
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    public class ReplyText : Reply
    {
        public string Content { get; set; }

        public ReplyText()
        {
            base.MsgType = EnumReplyMsgType.text;
        }
    }
}
