/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Agent
*文件名： AgentInfo.cs
*版本号： V1.0.0.0
*唯一标识：3cbd3209-092b-4166-96cf-de548ff3895c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：AgentInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AgentInfo 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// Agent 信息模型
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// Agent ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

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

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    [JsonPropertyName("create_date")]
    public string CreateDate { get; set; } = string.Empty;

    /// <summary>
    /// 更新日期
    /// </summary>
    [JsonPropertyName("update_date")]
    public string UpdateDate { get; set; } = string.Empty;

    /// <summary>
    /// 用户 ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Agent 头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// Canvas 类型
    /// </summary>
    [JsonPropertyName("canvas_type")]
    public string? CanvasType { get; set; }
}