/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Speech
*文件名： Config
*版本号： V1.0.0.0
*唯一标识：47a594f3-f34f-4641-9b67-8008bd3daf4f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/26 14:43:28
*描述：语音配置
*
*=================================================
*修改标记
*修改时间：2023/12/26 14:43:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：语音配置
*
*****************************************************************************/
namespace LuBan.Speech
{
    /// <summary>
    /// 语音配置
    /// </summary>
    public class SpeechConfig
    {
        /// <summary>
        /// 语音转换类型
        /// </summary>
        public EnumSpeechType SpeechType { get; set; }

        /// <summary>
        /// appid
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
