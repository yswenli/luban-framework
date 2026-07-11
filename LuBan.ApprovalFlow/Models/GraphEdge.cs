/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： GraphEdge
*版本号： V1.0.0.0
*唯一标识：caf96d7e-f86f-4a1a-af68-c65dd05f4b20
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:52:28
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:52:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;


/// <summary>
/// 图连线，连接源节点与目标节点并可携带动作标签。
/// </summary>
public class GraphEdge
{
    /// <summary>
    /// 连线唯一标识。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// 连线类型（例如 polyline）。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // polyline
    /// <summary>
    /// 连线属性字典，用于扩展业务配置。
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; set; }
    /// <summary>
    /// 源节点的Id。
    /// </summary>
    [JsonPropertyName("sourceNodeId")]
    public string SourceNodeId { get; set; } = string.Empty;
    /// <summary>
    /// 目标节点的Id。
    /// </summary>
    [JsonPropertyName("targetNodeId")]
    public string TargetNodeId { get; set; } = string.Empty;
    /// <summary>
    /// 源锚点Id（可选）。
    /// </summary>
    [JsonPropertyName("sourceAnchorId")]
    public string? SourceAnchorId { get; set; }
    /// <summary>
    /// 目标锚点Id（可选）。
    /// </summary>
    [JsonPropertyName("targetAnchorId")]
    public string? TargetAnchorId { get; set; }
    /// <summary>
    /// 连线上的文本信息（包含标签，如：通过/退回/不通过/已完成）。
    /// </summary>
    [JsonPropertyName("text")]
    public GraphText? Text { get; set; } // 包含 label，如：通过/退回/不通过/已完成
}