/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chat
*文件名： ChatChoice.cs
*版本号： V1.0.0.0
*唯一标识：dbd1881e-703a-498c-9c1e-a3dbe8f03381
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ChatChoice 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ChatChoice 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天选择模型
/// </summary>
public class ChatChoice
{
    /// <summary>
    /// 聊天消息
    /// </summary>
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }

    /// <summary>
    /// 增量消息
    /// </summary>
    [JsonPropertyName("delta")]
    public ChatMessage? Delta { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary>
    /// 选项索引
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }
}