/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：系统通知公告表
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统通知公告表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统通知公告表
/// </summary>
[SugarTable("db_notice", "系统通知公告表")]
[SysTable]
public class DbNotice : EntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    [SugarColumn(ColumnDescription = "标题", Length = 32)]
    [Required, MaxLength(32)]
    public string Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnDescription = "内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [Required]
    public string Content { get; set; }

    /// <summary>
    /// 类型（1通知 2公告）
    /// </summary>
    [SugarColumn(ColumnDescription = "类型（1通知 2公告）")]
    public EnumNoticeType Type { get; set; }

    /// <summary>
    /// 发布人Id
    /// </summary>
    [SugarColumn(ColumnDescription = "发布人Id")]
    public long PublicUserId { get; set; }

    /// <summary>
    /// 发布人姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "发布人姓名", Length = 32)]
    [MaxLength(32)]
    public string? PublicUserName { get; set; }

    /// <summary>
    /// 发布机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "发布机构Id")]
    public long PublicOrgId { get; set; }

    /// <summary>
    /// 发布机构名称
    /// </summary>
    [SugarColumn(ColumnDescription = "发布机构名称", Length = 64)]
    [MaxLength(64)]
    public string? PublicOrgName { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [SugarColumn(ColumnDescription = "发布时间")]
    public DateTime? PublicTime { get; set; }

    /// <summary>
    /// 撤回时间
    /// </summary>
    [SugarColumn(ColumnDescription = "撤回时间")]
    public DateTime? CancelTime { get; set; }

    /// <summary>
    /// 状态（0草稿 1发布 2撤回 3删除）
    /// </summary>
    [SugarColumn(ColumnDescription = "状态（0草稿 1发布 2撤回 3删除）")]
    public EnumNoticeStatus Status { get; set; }
}