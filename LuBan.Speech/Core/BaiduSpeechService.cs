/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Speech
*文件名： BaiduSpeech
*版本号： V1.0.0.0
*唯一标识：e7000da0-a81a-459e-9d16-2fbc2dc6263d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/26 11:09:00
*描述：百度语音识别服务
*
*=================================================
*修改标记
*修改时间：2023/12/26 11:09:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：百度语音识别服务
*
*****************************************************************************/



namespace LuBan.Speech.Core;

/// <summary>
/// 百度语音识别服务,
/// https://console.bce.baidu.com/tools/?_=1668482508529#/api?product=AI&project=%E8%AF%AD%E9%9F%B3%E6%8A%80%E6%9C%AF&parent=%E8%AF%AD%E9%9F%B3%E8%AF%86%E5%88%AB&api=/server_api%20&method=post
/// </summary>
public class BaiduSpeechService : BaseService<BaiduSpeechService>, ISpeechService
{
    SpeechConfig _speechConfig;

    /// <summary>
    /// 百度语音识别服务
    /// </summary>
    public BaiduSpeechService()
    {

    }

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="speechConfig"></param>
    public void SetConfig(SpeechConfig speechConfig)
    {
        _speechConfig = speechConfig;
    }

    /// <summary>
    /// 获取token
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetToken()
    {
        var client = HttpClientProxy.Create($"https://aip.baidubce.com");
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _speechConfig.ClientId },
            { "client_secret", _speechConfig.ClientSecret }
        };
        var data = await client.PostFormForParamsAsync($"/oauth/2.0/token?grant_type=client_credentials&client_id=11305335&client_secret=", formData);
        var bytes = data.ToStr();
        if (bytes == null)
        {
            return string.Empty;
        }
        var result = JsonConvert.DeserializeObject<dynamic>(bytes);
        if (result == null) return string.Empty;
        return result.access_token.ToString();
    }
    /// <summary>
    /// 识别
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public async Task<Result> RecognitionAsync(Stream stream, string format = "pcm")
    {
        return await GetResultAsync(async () =>
        {
            var client = HttpClientProxy.Create($"https://vop.baidu.com");
            var token = await GetToken();
            var len = stream.Length;
            var speech = stream.ToBase64Str();
            var jsonObj = new
            {
                format,
                rate = 16000,
                channel = 1,
                cuid = GuidUtil.New,
                token,
                speech,
                len
            };
            var result = await client.PostJsonAsync<BaiduSpeechResult>("/server_api", jsonObj.ToJson());
            if (result.ErrNo == 0)
                return result.Result?.Join(",") ?? "";
            else
                throw FriendlyError.Ex(result.ErrMsg);
        });
    }
}



