/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.File
*文件名： ListFilesQuery.cs
*版本号： V1.0.0.0
*唯一标识：9ddf66fa-2c3b-4499-b5b5-78a0f3c9d2c0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListFilesQuery 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListFilesQuery 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.File;

/// <summary>
/// 文件列表查询参数
/// </summary>
public class ListFilesQuery
{
    /// <summary>
    /// 页码，默认为 1
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量，默认为 30
    /// </summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 30;

    /// <summary>
    /// 排序字段，可选值：create_time（默认）、update_time
    /// </summary>
    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; } = "create_time";

    /// <summary>
    /// 是否降序排序，默认为 true
    /// </summary>
    [JsonPropertyName("desc")]
    public bool Desc { get; set; } = true;

    /// <summary>
    /// 文件名称过滤
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 文件 ID 过滤
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}