/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Common
*文件名： ChatMessage.cs
*版本号： V1.0.0.0
*唯一标识：0803a5f5-eced-4673-bb90-fa92c3acfc6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ChatMessage 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ChatMessage 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Common;

/// <summary>
/// 聊天消息模型
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 消息角色，可选值：system、user、assistant
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}