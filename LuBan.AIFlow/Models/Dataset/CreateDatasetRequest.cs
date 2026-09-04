/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Dataset
*文件名： CreateDatasetRequest.cs
*版本号： V1.0.0.0
*唯一标识：5be4840d-12bc-4ac9-863d-f93d3c1cc0c3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：CreateDatasetRequest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：CreateDatasetRequest 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集创建请求模型
/// </summary>
public class CreateDatasetRequest
{
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
}