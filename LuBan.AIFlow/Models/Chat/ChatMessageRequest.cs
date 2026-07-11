/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI.Models.Chat
*文件名： ChatMessageRequest
*版本号： V1.0.0.0
*唯一标识：b6bc81c0-6eca-45f8-a215-641dba5ca427
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/1 14:21:00
*描述：
*
*=================================================
*修改标记
*修改时间：2025/7/1 14:21:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using Newtonsoft.Json;

namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天消息请求模型
/// </summary>
public class ChatMessageRequest
{
    /// <summary>
    /// 输入参数字典
    /// </summary>
    [JsonProperty("inputs")]
    public object Inputs { get; set; } = new();

    /// <summary>
    /// 查询内容
    /// </summary>
    [JsonProperty("query")]
    public string Query { get; set; }

    /// <summary>
    /// 响应模式
    /// 默认值: "blocking"
    /// </summary>
    [JsonProperty("response_mode")]
    public string ResponseMode { get; set; } = "blocking";

    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonProperty("conversation_id")]
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// 用户标识
    /// </summary>
    [JsonProperty("user")]
    public string User { get; set; } = "yswenli";

    /// <summary>
    /// 聊天文件列表
    /// </summary>
    [JsonProperty("files")]
    public List<ChatFile> Files { get; set; } = new();
}
/// <summary>
/// 聊天文件模型
/// </summary>
public class ChatFile
{
    /// <summary>
    /// 文件类型
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = "image";
    /// <summary>
    /// 传输方式
    /// </summary>
    [JsonProperty("transfer_method")]
    public string TransferMethod { get; set; } = "remote_url";
    /// <summary>
    /// 地址
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }
}
