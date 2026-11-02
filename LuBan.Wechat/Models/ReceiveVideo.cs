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
*文件名： ReceiveVideo
*版本号： V1.0.0.0
*唯一标识：08be66fb-1914-431b-bd47-aab2954b065f
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:51:12
*描述：ReceiveVideo 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:51:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveVideo 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReceiveVideo 类
/// </summary>
    public class ReceiveVideo : Receive
    {
        /// <summary>
        /// 视频消息的素材 ID，可调用素材接口下载视频（协议字段 MediaId）。
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// 视频消息缩略图的素材 ID（协议字段 ThumbMediaId）。
        /// </summary>
        public string ThumbMediaId { get; set; }
    }
}
