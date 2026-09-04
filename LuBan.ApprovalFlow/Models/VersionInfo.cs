/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： VersionInfo.cs
*版本号： V1.0.0.0
*唯一标识：b620d391-7f18-4f5b-8a6a-aa1980348295
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：VersionInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：VersionInfo 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 版本信息，描述流程定义的版本详情。
/// </summary>
public class VersionInfo
{
    /// <summary>
    /// 版本ID。
    /// </summary>
    public long VersionId { get; set; }
    /// <summary>
    /// 版本号。
    /// </summary>
    public int Version { get; set; }
    /// <summary>
    /// 是否当前启用版本。
    /// </summary>
    public bool IsCurrent { get; set; }
    /// <summary>
    /// 激活时间。
    /// </summary>
    public DateTime? ActivatedAt { get; set; }
    /// <summary>
    /// 激活操作人用户ID。
    /// </summary>
    public long? ActivatedBy { get; set; }
    /// <summary>
    /// 激活操作人名称。
    /// </summary>
    public string? ActivatedByName { get; set; }
    /// <summary>
    /// 版本变更说明。
    /// </summary>
    public string? ChangeLog { get; set; }
    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }
}