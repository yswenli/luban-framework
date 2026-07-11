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