/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： GraphText
*版本号： V1.0.0.0
*唯一标识：33563b92-d38a-411c-b5b7-dde5a72d0597
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:53:35
*描述：文本标注信息
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:53:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文本标注信息
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 文本标注信息，通常用于节点或连线的显示标签。
/// </summary>
public class GraphText
{
    /// <summary>
    /// 文本在画布上的X坐标（可选）。
    /// </summary>
    [JsonPropertyName("x")]
    public double? X { get; set; }
    /// <summary>
    /// 文本在画布上的Y坐标（可选）。
    /// </summary>
    [JsonPropertyName("y")]
    public double? Y { get; set; }
    /// <summary>
    /// 文本内容（可选）。
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}