/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chunk
*文件名： RetrieveChunksRequest.cs
*版本号： V1.0.0.0
*唯一标识：9dd5becb-1186-4b80-a968-8b45cf33f949
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：RetrieveChunksRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：RetrieveChunksRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 检索分块请求模型
/// </summary>
public class RetrieveChunksRequest
{
    /// <summary>
    /// 用于相似度检索的查询文本（请求参数 query）。
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// 返回相似度最高的分块数量，默认 5（请求参数 top_k）。
    /// </summary>
    [JsonPropertyName("top_k")]
    public int TopK { get; set; } = 5;
}