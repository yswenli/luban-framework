/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumNoticeType
*版本号： V1.0.0.0
*唯一标识：096e6ba5-c71b-47e3-a40f-22558772f6e7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:12:24
*描述：通知公告状类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:12:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：通知公告状类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 通知公告状类型枚举
/// </summary>
[Description("通知公告状类型枚举")]
public enum EnumNoticeType
{
    /// <summary>
    /// 通知
    /// </summary>
    [Description("通知")]
    NOTICE = 1,

    /// <summary>
    /// 公告
    /// </summary>
    [Description("公告")]
    ANNOUNCEMENT = 2,
}
