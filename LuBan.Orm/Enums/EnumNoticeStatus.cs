/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumNoticeStatus
*版本号： V1.0.0.0
*唯一标识：4ebb5ec5-57db-4aac-b628-8c3c8abac572
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:11:43
*描述：通知公告状态枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:11:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：通知公告状态枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 通知公告状态枚举
/// </summary>
[Description("通知公告状态枚举")]
public enum EnumNoticeStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    [Description("草稿")]
    DRAFT = 0,

    /// <summary>
    /// 发布
    /// </summary>
    [Description("发布")]
    PUBLIC = 1,

    /// <summary>
    /// 撤回
    /// </summary>
    [Description("撤回")]
    CANCEL = 2,

    /// <summary>
    /// 删除
    /// </summary>
    [Description("删除")]
    DELETED = 3
}
