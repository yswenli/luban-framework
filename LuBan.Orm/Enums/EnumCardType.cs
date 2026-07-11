/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumCardType
*版本号： V1.0.0.0
*唯一标识：7d00caff-33c4-426d-acee-22119d9ed223
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:04:09
*描述：证件类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:04:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：证件类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 证件类型枚举
/// </summary>
[Description("证件类型枚举")]
public enum EnumCardType
{
    /// <summary>
    /// 身份证
    /// </summary>
    [Description("身份证")]
    IdCard = 0,

    /// <summary>
    /// 护照
    /// </summary>
    [Description("护照")]
    PassportCard = 1,

    /// <summary>
    /// 出生证
    /// </summary>
    [Description("出生证")]
    BirthCard = 2,

    /// <summary>
    /// 港澳台通行证
    /// </summary>
    [Description("港澳台通行证")]
    GatCard = 3,

    /// <summary>
    /// 外国人居留证
    /// </summary>
    [Description("外国人居留证")]
    ForeignCard = 4
}
