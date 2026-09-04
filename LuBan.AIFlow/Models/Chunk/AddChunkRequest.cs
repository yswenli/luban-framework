/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Chunk
*文件名： AddChunkRequest.cs
*版本号： V1.0.0.0
*唯一标识：c98dedb8-1b89-406c-a673-6aec4fedf4f1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：AddChunkRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AddChunkRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Chunk;

/// <summary>
/// 添加分块请求模型
/// </summary>
public class AddChunkRequest
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }
}