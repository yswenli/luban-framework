/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Session
*文件名： ListSessionsQuery.cs
*版本号： V1.0.0.0
*唯一标识：2d0bc4c0-d0f8-43a4-a58f-abdd4c49cf42
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListSessionsQuery 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListSessionsQuery 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Session;

/// <summary>
/// 会话列表查询模型
/// </summary>
public class ListSessionsQuery
{
    /// <summary>
    /// 页码
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("order_by")]
    public string OrderBy { get; set; } = string.Empty;

    /// <summary>
    /// 是否降序
    /// </summary>
    [JsonPropertyName("desc")]
    public bool Desc { get; set; }

    /// <summary>
    /// 会话名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 会话 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}