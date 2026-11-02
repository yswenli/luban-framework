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
*文件名： ReceiveVoice
*版本号： V1.0.0.0
*唯一标识：0f628e8a-fcda-4ee4-b355-f05b26a186e6
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/5 14:50:33
*描述：ReceiveVoice 类
*
*=================================================
*修改标记
*修改时间：2022/7/5 14:50:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ReceiveVoice 类
*
*****************************************************************************/

namespace LuBan.Wechat.Models
{
    /// <summary>
/// ReceiveVoice 类
/// </summary>
    public class ReceiveVoice : Receive
    {
        /// <summary>
        /// 语音消息的素材 ID，可调用素材接口下载语音（协议字段 MediaId）。
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// 语音格式，如 amr、speex（协议字段 Format）。
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 语音识别结果文本，仅在公众号开通语音识别时返回（协议字段 Recognition）。
        /// </summary>
        public string Recognition { get; set; }
    }
}
