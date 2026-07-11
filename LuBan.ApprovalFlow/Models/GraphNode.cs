/*****************************************************************************
 * Copyright (c) YSWenli All Rights Reserved.
 * CLR版本： .net8.0
 * 机器名称：WALLE
 * Author：yswenli
 * 命名空间：LuBan.ApprovalFlow.Models
 * 文件名： GraphNode
 * 版本号： V1.0.0.0
 * 唯一标识：04c1b1a5-223e-4a00-bb9e-9ffadc984a54
 * 当前的用户域：WALLE
 * 创建人： yswenli
 * 电子邮箱：yswenli@outlook.com
 * 创建时间：2025/10/30 13:51:47
 * 描述：图节点
 *
 * =================================================
 * 修改标记
 * 修改时间：2025/10/30 13:51:47
 * 修改人： yswenli
 * 版本号： V1.0.0.0
 * 描述：图节点
 *****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 图节点，表示流程中的一个步骤或网关。
/// </summary>
public class GraphNode
{
    /// <summary>
    /// 节点唯一标识。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 节点类型：start-node/service-node/user-node/gateway-node/end-node/service-gateway-node。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    // start-node/service-node/user-node/gateway-node/end-node/service-gateway-node

    /// <summary>
    /// 节点在画布上的X坐标（可选）。
    /// </summary>
    [JsonPropertyName("x")]
    public double? X { get; set; }

    /// <summary>
    /// 节点在画布上的Y坐标（可选）。
    /// </summary>
    [JsonPropertyName("y")]
    public double? Y { get; set; }

    /// <summary>
    /// 节点属性字典，用于扩展业务配置。
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// 节点显示文本信息。
    /// </summary>
    [JsonPropertyName("text")]
    public GraphText? Text { get; set; }
}
