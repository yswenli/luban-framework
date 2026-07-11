/****************************************************************************
*Copyright @ 2023-2024 Oceania All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Wechat.Models
*文件名： Receive
*版本号： V1.0.0.0
*唯一标识：731bf4cb-0f4c-40f8-810d-3f401864dfa9
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:42:37
*描述：
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:42:37
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
    /// Receive
    /// </summary>
    public class Receive
    {
        /// <summary>
        /// MsgId
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// ToUserName
        /// </summary>
        public string ToUserName { get; set; }
        /// <summary>
        /// FromUserName
        /// </summary>
        public string FromUserName { get; set; }
        /// <summary>
        /// CreateTime
        /// </summary>
        public long CreateTime { get; set; }
        /// <summary>
        /// MsgType
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// ReceiveMsgTypeEnum
        /// </summary>
        public EnumReceiveMsgType EnumMsgType
        {
            get
            {
                try
                {
                    object obj = System.Enum.Parse(typeof(EnumReceiveMsgType), MsgType);
                    return (EnumReceiveMsgType)obj;
                }
                catch
                {
                    return EnumReceiveMsgType.None;
                }
            }
        }
        /// <summary>
        /// Bind
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Receive Bind(Receive obj)
        {
            ToUserName = obj.ToUserName;
            FromUserName = obj.FromUserName;
            CreateTime = obj.CreateTime;
            MsgType = obj.MsgType;
            MsgId = obj.MsgId;
            return this;
        }
    }
}
