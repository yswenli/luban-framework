/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Dataset
*文件名： UpdateDatasetRequest.cs
*版本号： V1.0.0.0
*唯一标识：0e5bb885-ee98-4f0e-adac-1ac1d9a26a7a
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：UpdateDatasetRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：UpdateDatasetRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集更新请求模型
/// </summary>
public class UpdateDatasetRequest
{
    /// <summary>
    /// 数据集名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 数据集描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}