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
*描述：
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统通知公告用户表
/// </summary>
[SugarTable("db_notice_user", "系统通知公告用户表")]
[SysTable]
public class DbNoticeUser : EntityBase
{
    /// <summary>
    /// 通知公告Id
    /// </summary>
    [SugarColumn(ColumnDescription = "通知公告Id")]
    public long NoticeId { get; set; }

    /// <summary>
    /// 通知公告
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(NoticeId)), JsonIgnore]
    public DbNotice SysNotice { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    [SugarColumn(ColumnDescription = "阅读时间")]
    public DateTime? ReadTime { get; set; }

    /// <summary>
    /// 状态（0未读 1已读）
    /// </summary>
    [SugarColumn(ColumnDescription = "状态（0未读 1已读）")]
    public EnumNoticeUserStatus ReadStatus { get; set; } = EnumNoticeUserStatus.UNREAD;
}