/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumGender
*版本号： V1.0.0.0
*唯一标识：35d57b17-9f95-4adb-a626-bdae8b46a141
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:04:41
*描述：性别枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:04:41
*修改人： yswenli
*版本号： V1.0.0.0
*描述：性别枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 性别枚举
/// </summary>
[Description("性别枚举")]
public enum EnumGender
{
    /// <summary>
    /// -
    /// </summary>
    [Description("-")]
    Other = 0,

    /// <summary>
    /// 男
    /// </summary>
    [Description("男")]
    Male = 1,

    /// <summary>
    /// 女
    /// </summary>
    [Description("女")]
    Female = 2
}
