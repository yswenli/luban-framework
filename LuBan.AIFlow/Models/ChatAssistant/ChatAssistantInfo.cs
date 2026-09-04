/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.ChatAssistant
*文件名： ChatAssistantInfo.cs
*版本号： V1.0.0.0
*唯一标识：cdbb2e06-f210-4051-b731-40ae50e82e0f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ChatAssistantInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ChatAssistantInfo 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.ChatAssistant;

/// <summary>
/// 聊天助手信息模型
/// </summary>
public class ChatAssistantInfo
{
    /// <summary>
    /// 聊天助手唯一标识
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 聊天助手名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

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
    /// 聊天助手配置信息
    /// </summary>
    [JsonPropertyName("config")]
    public object Config { get; set; } = new();

    /// <summary>
    /// 创建时间（Unix时间戳，毫秒）
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间（Unix时间戳，毫秒）
    /// </summary>
    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }
}