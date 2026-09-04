/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： VersionCompareResult.cs
*版本号： V1.0.0.0
*唯一标识：ba15c9eb-b7d7-4614-9441-1dff9dd71548
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：VersionCompareResult 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：VersionCompareResult 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 版本比较结果，描述两个版本之间的差异。
/// </summary>
public class VersionCompareResult
{
    /// <summary>
    /// 版本1号。
    /// </summary>
    public int Version1 { get; set; }
    /// <summary>
    /// 版本2号。
    /// </summary>
    public int Version2 { get; set; }
    /// <summary>
    /// 点变更信息列表。
    /// </summary>
    public List<NodeChangeInfo>? NodeChanges { get; set; }
    /// <summary>
    /// 边变更信息列表。
    /// </summary>
    public List<EdgeChangeInfo>? EdgeChanges { get; set; }
    /// <summary>
    /// 属性变更信息列表。
    /// </summary>
    public List<PropertyChangeInfo>? PropertyChanges { get; set; }
}

/// <summary>
/// 节点变更信息，描述单个节点的变更详情。
/// </summary>
public class NodeChangeInfo
{
    /// <summary>
    /// 节点ID。
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    /// <summary>
    /// 变更类型：added/removed/modified。
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;
    /// <summary>
    /// 变更前的值。
    /// </summary>
    public string? OldValue { get; set; }
    /// <summary>
    /// 变后的值。
    /// </summary>
    public string? NewValue { get; set; }
}

/// <summary>
/// 边变更信息，描述单个边的变更详情。
/// </summary>
public class EdgeChangeInfo
{
    /// <summary>
    /// 边ID。
    /// </summary>
    public string EdgeId { get; set; } = string.Empty;
    /// <summary>
    /// 变更类型：added/removed/modified。
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;
    /// <summary>
    /// 变更前的值。
    /// </summary>
    public string? OldValue { get; set; }
    /// <summary>
    /// 变后的值。
    /// </summary>
    public string? NewValue { get; set; }
}

/// <summary>
/// 属性变更信息，描述单个属性的变更详情。
/// </summary>
public class PropertyChangeInfo
{
    /// <summary>
    /// 属性名称。
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    /// <summary>
    /// 变更前的值。
    /// </summary>
    public string? OldValue { get; set; }
    /// <summary>
    /// 变后的值。
    /// </summary>
    public string? NewValue { get; set; }
}