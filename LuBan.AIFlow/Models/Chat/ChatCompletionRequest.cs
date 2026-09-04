/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chat
*文件名： ChatCompletionRequest.cs
*版本号： V1.0.0.0
*唯一标识：567a7696-af44-4c01-a57f-d78871cfea7a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ChatCompletionRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ChatCompletionRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天补全请求模型
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// 模型名称
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "model";

    /// <summary>
    /// 聊天消息列表
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// 是否使用流式响应
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}