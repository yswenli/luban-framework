/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumNoticeUserStatus
*版本号： V1.0.0.0
*唯一标识：7826a18f-ec05-425a-8151-908661e2f8e4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:15:43
*描述：通知公告用户状态枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:15:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：通知公告用户状态枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 通知公告用户状态枚举
/// </summary>
[Description("通知公告用户状态枚举")]
public enum EnumNoticeUserStatus
{
    /// <summary>
    /// 未读
    /// </summary>
    [Description("未读")]
    UNREAD = 0,

    /// <summary>
    /// 已读
    /// </summary>
    [Description("已读")]
    READ = 1
}
