/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Speech
*文件名： SpeechFactory
*版本号： V1.0.0.0
*唯一标识：b6b01ba1-ed84-4d4a-a001-26ab067bac19
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/26 14:46:44
*描述：语音转换工厂类
*
*=================================================
*修改标记
*修改时间：2023/12/26 14:46:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：语音转换工厂类
*
*****************************************************************************/
namespace LuBan.Speech;

/// <summary>
/// 语音转换工厂类
/// </summary>
public static class SpeechFactory
{
    static SpeechConfig _speechConfig;

    /// <summary>
    /// 语音转换工厂类
    /// </summary>
    static SpeechFactory()
    {
        _speechConfig = ConfigUtil.Read<SpeechConfig>() ?? new SpeechConfig();
    }


    /// <summary>
    /// 创建服务
    /// </summary>
    /// <returns></returns>
    public static ISpeechService Create()
    {
        ISpeechService speechService;
        switch (_speechConfig.SpeechType)
        {
            case EnumSpeechType.Baidu:
            default:
                speechService = BaiduSpeechService.Instance;
                break;
        }
        speechService.SetConfig(_speechConfig);
        return speechService;
    }

    /// <summary>
    /// 创建服务
    /// </summary>
    /// <param name="speechConfig"></param>
    /// <returns></returns>
    public static ISpeechService Create(SpeechConfig speechConfig)
    {
        ISpeechService speechService;
        switch (speechConfig.SpeechType)
        {
            case EnumSpeechType.Baidu:
            default:
                speechService = BaiduSpeechService.Instance;
                break;
        }
        speechService.SetConfig(speechConfig);
        return speechService;
    }
}
