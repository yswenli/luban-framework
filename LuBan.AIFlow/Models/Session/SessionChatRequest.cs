/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Session
*文件名： SessionChatRequest.cs
*版本号： V1.0.0.0
*唯一标识：b8132818-f76d-4b79-9db7-a424a4867c91
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：SessionChatRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SessionChatRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话聊天请求模型
/// </summary>
public class SessionChatRequest
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 是否流式响应
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}