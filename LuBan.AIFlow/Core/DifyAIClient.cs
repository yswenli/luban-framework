/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI.Core
*文件名： DifyAIClient
*版本号： V1.0.0.0
*唯一标识：a9a020c0-0510-475a-b8a3-df776d7f0499
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/1 14:19:24
*描述：
*
*=================================================
*修改标记
*修改时间：2025/7/1 14:19:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using System.Text;

namespace LuBan.AIFlow.Core;

public class DifyAIClient : IAIClient
{
    HttpClientProxy _httpClient;
    Dictionary<string, string> _headers;


    /// <summary>
    /// AI客户端选项
    /// </summary>
    public AIOptions Options { get; }

    /// <summary>
    /// dify客户端
    /// </summary>
    /// <param name="option"></param>
    public DifyAIClient(AIOptions? option)
    {
        if (option == null || option.BaseUrl.IsNullOrEmpty()) throw new ArgumentNullException(nameof(option), "DifyConfig cannot be null");
        Options = option;
        _httpClient = HttpClientProxy.Create(option.BaseUrl, useLog: true);
        _headers = new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {option.ApiKey}" },
        { "Content-Type", "application/json" }
    };
    }
    /// <summary>
    /// dify客户端
    /// </summary>
    public DifyAIClient() : this(NacosConfigUtil.Read<AIOptions>())
    {

    }


    /// <summary>
    /// 发送聊天消息
    /// </summary>
    /// <param name="request">聊天消息请求</param>
    /// <returns>聊天响应结果</returns>
    public async Task<ChatResponse> SendChatMsgAsync(ChatMessageRequest request)
    {
        return await _httpClient.PostAsync<ChatResponse>("/v1/chat-messages", request, _headers);
    }
    /// <summary>
    /// 发送聊天消息
    /// </summary>
    /// <param name="urls"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<EnumAnswerType> SendChatMsgAsync(List<string> urls)
    {
        if (urls == null || urls.Count == 0) throw new ArgumentNullException(nameof(urls), "URLs cannot be null or empty");
        var imageSb = new StringBuilder();
        for (int i = 0; i < (urls.Count > 10 ? 10 : urls.Count); i++)
        {
            imageSb.Append($"\"image{(i + 1)}\":{{\"type\":\"image\",\"transfer_method\":\"remote_url\",\"url\":\"{urls[i]}\"}},");
        }
        var imageStr = imageSb.ToString().TrimEnd(',');
        var json = $"{{\"inputs\":{{{imageStr}}},\"query\":\"请帮我判断这些图片中是否有验光单或眼部生物测量图\",\"response_mode\":\"blocking\",\"conversation_id\":\"\",\"user\":\"yswenli\"}}";

        var response = await _httpClient.PostAsync("/v1/chat-messages", json, _headers);
        if (response.IsNullOrEmpty()) return 0;
        var result = SerializeUtil.Deserialize<ChatResponse>(response);
        if (result == null) return 0;
        var answer = result.Answer?.Trim();
        if (answer.IsNullOrEmpty()) return 0;
        switch (answer)
        {
            case "0,0":
                return EnumAnswerType.Default;
            case "1,0":
                return EnumAnswerType.IsEyePrescription;
            case "0,1":
                return EnumAnswerType.IsIOLMaster;
            case "1,1":
                return EnumAnswerType.All;
            default:
                return EnumAnswerType.Default;
        }
    }
}
