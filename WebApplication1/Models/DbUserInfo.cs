/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：WebApplication1.Models
*文件名： DbUserInfo
*版本号： V1.0.0.0
*唯一标识：87ab33b0-f6f5-4ee3-ab78-05e4685bbf40
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 10:24:33
*描述：业务用户表
*
*=================================================
*修改标记
*修改时间：2025/10/14 10:24:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：业务用户表
*
*****************************************************************************/
namespace WebApplication1.Models;

/// <summary>
/// 业务用户表
/// </summary>
[SugarTable("db_user_info", "业务用户表")]
public class DbUserInfo : EntityTenant
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "用户ID")]
    public long UserId { get; set; }

    /// <summary>
    /// 认证状态
    /// </summary>
    [SugarColumn(ColumnDescription = "认证状态")]
    public EnumUserAuditStatus? AuditStatus { get; set; } = EnumUserAuditStatus.Waiting;

    /// <summary>
    /// 系统用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId), nameof(DbUser.Id), "is_delete=false")]
    public DbUser DbUser { get; set; }

    /// <summary>
    /// 民族
    /// </summary>
    [SugarColumn(ColumnDescription = "民族", Length = 32)]
    [MaxLength(32)]
    public string? Nation { get; set; }

    /// <summary>
    /// 证件类型
    /// </summary>
    [SugarColumn(ColumnDescription = "证件类型")]
    public EnumCardType CardType { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [SugarColumn(ColumnDescription = "身份证号", Length = 32)]
    [MaxLength(32)]
    public string? IdCardNum { get; set; }


    /// <summary>
    /// 文化程度
    /// </summary>
    [SugarColumn(ColumnDescription = "文化程度")]
    public EnumCultureLevel CultureLevel { get; set; }

    /// <summary>
    /// 政治面貌
    /// </summary>
    [SugarColumn(ColumnDescription = "政治面貌", Length = 16)]
    [MaxLength(16)]
    public string? PoliticalOutlook { get; set; }

    /// <summary>
    /// 毕业院校
    /// </summary>
    [SugarColumn(ColumnDescription = "毕业院校", Length = 128)]
    [MaxLength(128)]
    public string? College { get; set; }

    /// <summary>
    /// 办公电话
    /// </summary>
    [SugarColumn(ColumnDescription = "办公电话", Length = 16)]
    [MaxLength(16)]
    public string? OfficePhone { get; set; }

    /// <summary>
    /// 紧急联系人
    /// </summary>
    [SugarColumn(ColumnDescription = "紧急联系人", Length = 32)]
    [MaxLength(32)]
    public string? EmergencyContact { get; set; }

    /// <summary>
    /// 紧急联系人电话
    /// </summary>
    [SugarColumn(ColumnDescription = "紧急联系人电话", Length = 16)]
    [MaxLength(16)]
    public string? EmergencyPhone { get; set; }

    /// <summary>
    /// 紧急联系人地址
    /// </summary>
    [SugarColumn(ColumnDescription = "紧急联系人地址", Length = 256)]
    [MaxLength(256)]
    public string? EmergencyAddress { get; set; }

    /// <summary>
    /// 学生证/医师执业证书
    /// </summary>
    [SugarColumn(ColumnDescription = "学生证/医师执业证书", Length = 4000)]
    [MaxLength(4096)]
    public string? Introduction { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    [SugarColumn(ColumnDescription = "工号", Length = 32)]
    [MaxLength(32)]
    public string? JobNum { get; set; }

    /// <summary>
    /// 职级
    /// </summary>
    [SugarColumn(ColumnDescription = "职级", Length = 32)]
    [MaxLength(32)]
    public string? PosLevel { get; set; }
    /// <summary>
    /// 擅长领域
    /// </summary>
    [SugarColumn(ColumnDescription = "擅长领域", Length = 32)]
    [MaxLength(32)]
    public string? Expertise { get; set; }

    /// <summary>
    /// 医院级别
    /// </summary>
    [SugarColumn(ColumnDescription = "医院级别", Length = 32)]
    [MaxLength(32)]
    public string? OfficeZone { get; set; }

    /// <summary>
    /// 专业背景（科室，部门）
    /// </summary>
    [SugarColumn(ColumnDescription = "专业背景（科室，部门）", Length = 32)]
    [MaxLength(32)]
    public string? Office { get; set; }

    /// <summary>
    /// 入学年份
    /// </summary>
    [SugarColumn(ColumnDescription = "入学年份")]
    public DateTime? JoinDate { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    [SugarColumn(ColumnDescription = "省份", Length = 10)]
    [MaxLength(10)]
    public string? Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [SugarColumn(ColumnDescription = "城市", Length = 10)]
    [MaxLength(10)]
    public string? City { get; set; }
}