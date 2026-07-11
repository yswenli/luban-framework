/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Enums
*文件名： EnumUserAuditStatus
*版本号： V1.0.0.0
*唯一标识：2be0073b-1d2b-4060-929e-96eaed50d20d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/14 10:23:14
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/14 10:23:14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Orm.Enums;


/// <summary>
/// 用户审核状态枚举
/// </summary>
[Description("用户审核状态枚举")]
public enum EnumUserAuditStatus
{
    /// <summary>
    /// 待审核
    /// </summary>
    [Description("待审核")]
    Waiting = 1,

    /// <summary>
    /// 审核通过
    /// </summary>
    [Description("审核通过")]
    Success = 2,

    /// <summary>
    /// 审核不通过
    /// </summary>
    [Description("审核不通过")]
    Fail = 3

}