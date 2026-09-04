/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.ChatAssistant
*文件名： UpdateChatAssistantRequest.cs
*版本号： V1.0.0.0
*唯一标识：ebdef067-aa07-46ca-b100-1bbd397a49d8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：UpdateChatAssistantRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：UpdateChatAssistantRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手更新请求模型
/// </summary>
public class UpdateChatAssistantRequest
{
    /// <summary>
    /// 聊天id
    /// </summary>
    [JsonPropertyName("chat_id")]
    public string ChatId { get; set; }

    /// <summary>
    /// 聊天助手名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 聊天助手头像（可选）
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 数据集ID列表
    /// </summary>
    [JsonPropertyName("dataset_ids")]
    public List<string> DataSetIds { get; set; }

    /// <summary>
    /// 聊天助手描述（可选）
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 聊天助手配置（可选）
    /// </summary>
    [JsonPropertyName("config")]
    public object? Config { get; set; }
}