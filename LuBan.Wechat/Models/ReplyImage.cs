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
*文件名： ReplyImage
*版本号： V1.0.0.0
*唯一标识：6b494eec-d68c-4887-9b0e-61735d0f96de
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:58:35
*描述：ReplyImage 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:58:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReplyImage 类
*
*****************************************************************************/
namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReplyImage 类
/// </summary>
    public class ReplyImage : Reply
    {
        /// <summary>
        /// 回复图片的素材 ID，需为已上传的临时或永久图片素材（回复协议字段 MediaId）。
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// 构造图片回复消息，并把消息类型置为 image。
        /// </summary>
        public ReplyImage()
        {
            base.MsgType = EnumReplyMsgType.image;
        }
    }
}
