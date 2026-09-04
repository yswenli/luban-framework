/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Dataset
*文件名： ListDatasetsResponse.cs
*版本号： V1.0.0.0
*唯一标识：88796950-893b-4be4-943f-17743667109e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ListDatasetsResponse 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ListDatasetsResponse 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Models.Dataset;

/// <summary>
/// 数据集列表响应模型
/// </summary>
public class ListDatasetsResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 数据集列表数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<DatasetInfo> Data { get; set; } = new();
}