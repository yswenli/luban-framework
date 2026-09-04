/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chunk
*文件名： RetrieveChunksResponse.cs
*版本号： V1.0.0.0
*唯一标识：767789f4-b6f2-40b0-b397-f7e95ced8a37
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：RetrieveChunksResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：RetrieveChunksResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 检索分块响应模型
/// </summary>
public class RetrieveChunksResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public List<ChunkInfo> Data { get; set; } = new();
}