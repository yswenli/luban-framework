/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chat
*文件名： ChatCompletionResponse.cs
*版本号： V1.0.0.0
*唯一标识：dbf3d385-ae0e-4167-a031-7e3569a425d7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ChatCompletionResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ChatCompletionResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天补全响应模型
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// 响应 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 响应选项列表
    /// </summary>
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = new();

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 响应对象类型
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    /// <summary>
    /// Token 使用情况统计
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}