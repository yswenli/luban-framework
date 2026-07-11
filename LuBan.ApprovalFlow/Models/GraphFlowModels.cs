/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： GraphFlowModels
*版本号： V1.0.0.0
*唯一标识：6c4a7b27-2e03-4f07-8d6e-9b0f0f6573af
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30
*描述：基于节点/边的图式审批流模型（兼容当前 data.json）
*
*=================================================
*修改标记
*修改时间：2025/10/30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：基于节点/边的图式审批流模型（兼容当前 data.json）
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 图式审批流定义，包含节点与连线集合。
/// </summary>
public class GraphFlowDefinition
{
    /// <summary>
    /// 流程编码，唯一标识此图式审批流。
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }
    /// <summary>
    /// 流程名称，用于展示与识别。
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    /// <summary>
    /// 备注说明，可选。
    /// </summary>
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
    /// <summary>
    /// 节点集合，描述流程中的各个步骤。
    /// </summary>
    [JsonPropertyName("nodes")]
    public List<GraphNode> Nodes { get; set; } = new();
    /// <summary>
    /// 连线集合，描述节点间的连接关系。
    /// </summary>
    [JsonPropertyName("edges")]
    public List<GraphEdge> Edges { get; set; } = new();
    /// <summary>
    /// 流程事件配置集合，描述流程生命周期事件回调。
    /// </summary>
    [JsonPropertyName("events")]
    public Dictionary<string, object>? Events { get; set; }
    /// <summary>
    /// 全局变量配置，用于URL占位符等。
    /// </summary>
    [JsonPropertyName("globalVariables")]
    public Dictionary<string, object>? GlobalVariables { get; set; }
}


