/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Entities
*文件名： AgentChatInput
*版本号： V1.0.0.0
*唯一标识：Agent 对话入参
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：Agent 对话接口的请求体
*
*****************************************************************************/

namespace WebApplication1.Models.Entities;

/// <summary>
/// Agent 对话入参。
/// </summary>
public class AgentChatInput
{
    /// <summary>
    /// 用户输入。
    /// </summary>
    public string Prompt { get; set; } = "";
}