/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Session
*文件名： SessionChatResponse.cs
*版本号： V1.0.0.0
*唯一标识：87a2ef44-9590-4c5e-a071-a60cc8aeec2a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：SessionChatResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SessionChatResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话聊天响应模型
/// </summary>
public class SessionChatResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public SessionChatData? Data { get; set; }
}