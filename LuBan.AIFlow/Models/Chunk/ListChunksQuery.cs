/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chunk
*文件名： ListChunksQuery.cs
*版本号： V1.0.0.0
*唯一标识：bf5b33b2-251b-4e7f-9591-661d69f10545
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListChunksQuery 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListChunksQuery 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 分块列表查询参数
/// </summary>
public class ListChunksQuery
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 30;

    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; } = "create_time";

    [JsonPropertyName("desc")]
    public bool Desc { get; set; } = true;

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}