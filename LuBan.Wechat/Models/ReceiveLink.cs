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
*文件名： ReceiveLink
*版本号： V1.0.0.0
*唯一标识：096f24d6-72c7-449f-be89-e35971337441
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:53:28
*描述：ReceiveLink 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:53:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveLink 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReceiveLink 类
/// </summary>
    public class ReceiveLink : Receive
    {
        /// <summary>
        /// 链接消息的标题（协议字段 Title）。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 链接消息的描述文本（协议字段 Description）。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 链接消息的跳转地址（协议字段 Url）。
        /// </summary>
        public string Url { get; set; }
    }
}
