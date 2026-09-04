/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Session
*文件名： SessionMessage.cs
*版本号： V1.0.0.0
*唯一标识：0d02686f-2a5a-4a9d-b406-94b41134b053
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：SessionMessage 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SessionMessage 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话消息模型
/// </summary>
public class SessionMessage
{
    /// <summary>
    /// 消息角色
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}