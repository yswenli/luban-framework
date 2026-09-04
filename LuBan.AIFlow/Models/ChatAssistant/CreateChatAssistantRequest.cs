/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.ChatAssistant
*文件名： CreateChatAssistantRequest.cs
*版本号： V1.0.0.0
*唯一标识：07d06817-7101-4cc3-8607-13b1b04e647b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：CreateChatAssistantRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CreateChatAssistantRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手创建请求模型
/// </summary>
public class CreateChatAssistantRequest
{
    /// <summary>
    /// 聊天助手名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据集ID列表
    /// </summary>
    [JsonPropertyName("dataset_ids")]
    public List<string> DataSetIds { get; set; }

    /// <summary>
    /// 聊天助手描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 聊天助手头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// 聊天助手配置
    /// </summary>
    [JsonPropertyName("config")]
    public object Config { get; set; } = new();
}