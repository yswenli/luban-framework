/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:56:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// Reply
    /// </summary>
    public class Reply
    {
        public string ToUserName { get; set; }

        public string FromUserName { get; set; }

        internal long CreateTime { get; set; }

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
