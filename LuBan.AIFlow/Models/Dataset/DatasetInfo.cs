/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Dataset
*文件名： DatasetInfo.cs
*版本号： V1.0.0.0
*唯一标识：024b0601-4ce9-48a1-85b1-3737da769292
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：DatasetInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DatasetInfo 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集信息模型
/// </summary>
public class DatasetInfo
{
    /// <summary>
    /// 数据集 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 数据集名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据集描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

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
}