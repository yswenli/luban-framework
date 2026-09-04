/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Session
*文件名： CreateSessionRequest.cs
*版本号： V1.0.0.0
*唯一标识：d1cd6d5e-7f4b-4e8f-b449-13fefacec6f4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：CreateSessionRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CreateSessionRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 创建会话请求模型
/// </summary>
public class CreateSessionRequest
{
    /// <summary>
    /// 要创建的聊天会话的名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// 可选的用户定义 ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}