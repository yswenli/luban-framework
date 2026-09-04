/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Agent
*文件名： ListAgentsQuery.cs
*版本号： V1.0.0.0
*唯一标识：34a2269c-48be-42c2-8974-1c68b956c822
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListAgentsQuery 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListAgentsQuery 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// 获取Agent列表查询参数
/// </summary>
public class ListAgentsQuery
{
    /// <summary>
    /// Agent ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Agent 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    [JsonPropertyName("page_size")]
    public int? PageSize { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; }

    /// <summary>
    /// 是否降序
    /// </summary>
    [JsonPropertyName("desc")]
    public bool? Desc { get; set; }
}