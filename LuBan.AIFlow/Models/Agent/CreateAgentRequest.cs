/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Agent
*文件名： CreateAgentRequest.cs
*版本号： V1.0.0.0
*唯一标识：35f70ca3-dc54-41ec-a57d-a7bc4e29b68a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：CreateAgentRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CreateAgentRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// Agent 创建请求模型
/// </summary>
public class CreateAgentRequest
{
    /// <summary>
    /// Agent 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Agent 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Agent 的 Canvas DSL 对象
    /// </summary>
    [JsonPropertyName("dsl")]
    public object Dsl { get; set; } = new();
}