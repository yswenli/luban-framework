/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.ChatAssistant
*文件名： ListChatAssistantsResponse.cs
*版本号： V1.0.0.0
*唯一标识：14d6a289-1935-4eab-9914-36d36bd5a424
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListChatAssistantsResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListChatAssistantsResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手列表响应模型
/// </summary>
public class ListChatAssistantsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public List<ChatAssistantInfo> Data { get; set; } = new();
}