/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Speech
*文件名： ISpeechService
*版本号： V1.0.0.0
*唯一标识：e7000da0-a81a-459e-9d16-2fbc2dc6263d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/26 11:09:00
*描述：语音识别
*
*=================================================
*修改标记
*修改时间：2023/12/26 11:09:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：语音识别
*
*****************************************************************************/

namespace LuBan.Speech
{
    /// <summary>
    /// 语音识别
    /// </summary>
    public interface ISpeechService
    {

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="speechConfig"></param>
        void SetConfig(SpeechConfig speechConfig);

        /// <summary>
        /// 获取token
        /// </summary>
        /// <returns></returns>
        Task<string> GetToken();
        /// <summary>
        /// 识别
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        Task<Result> RecognitionAsync(Stream stream, string format = "pcm");
    }
}