/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：WebApplication1.Models.Entities
*文件名： AgentConfirmInput
*版本号： V1.0.0.0
*唯一标识：Agent 工具确认入参
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：工具确认接口（api/agent/confirm）的请求体
*
*****************************************************************************/

namespace WebApplication1.Models.Entities;

/// <summary>
/// 工具确认入参（对应 <c>tool.confirm</c> 帧）。
/// </summary>
public class AgentConfirmInput
{
    /// <summary>会话标识；必须是当前用户自己的会话。</summary>
    public string SessionId { get; set; } = "";

    /// <summary>确认标识，取自 <c>tool.confirm</c> 帧的 <c>confirmationId</c>。</summary>
    public string ConfirmationId { get; set; } = "";

    /// <summary><c>allow</c> / <c>deny</c> / <c>allowAll</c> / <c>allowSession</c>。</summary>
    public string Result { get; set; } = "";
}